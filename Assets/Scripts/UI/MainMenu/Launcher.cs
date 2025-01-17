﻿using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField createNickNameInputField;
    [SerializeField] TMP_InputField findNickNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text currPlayersInRoom;
    [SerializeField] Transform roomListContent;
    public Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject rejoinContainer;
    [SerializeField] GameObject settingsContainer;
    [SerializeField] GameObject notFourPlayersContainer;

    public const int maxPlayers = 4;
    int playersLeftToFillRoom;

    bool startAnyways = false;
    bool startGamePressed = false;
    bool rejoinCalled = false;
    string latestRoomName;

    bool connectedAfterStartup = false;
    const float timeBetweenRetries = 3f;
    float connectAgainTimer = timeBetweenRetries;

    List<string> characterList;
    List<int> spawnPointList;

    Dictionary<string, RoomListItem> cachedRoomList = new Dictionary<string, RoomListItem>();
    List<string> abandonedRooms = new List<string>();

    public static bool tutorial = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected) return;

        // Connecting to master server (Set to eu in PhotonServerSettings)
        Debug.Log("Connecting to the master server...");
        AuthenticationValues authValues = new AuthenticationValues(PlayerPrefs.GetString("userid", ""));
        PhotonNetwork.AuthValues = authValues;
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {
        if (PhotonNetwork.InRoom) currPlayersInRoom.text = PhotonNetwork.PlayerList.Length.ToString()+" / "+maxPlayers;
        if (!connectedAfterStartup)
        {
            connectAgainTimer -= Time.deltaTime;
            if (connectAgainTimer <= 0)
            {
                print("Retrying...");
                connectAgainTimer = timeBetweenRetries;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        connectedAfterStartup = true;
        if (PhotonNetwork.InLobby) return;

        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to the " + PhotonNetwork.CloudRegion + " server!");
        PlayerPrefs.SetString("userid", PhotonNetwork.LocalPlayer.UserId);
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("latestNick", "");
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined Lobby");
        ClearCachedRooms();
    }

    public override void OnLeftLobby()
    {
        ClearCachedRooms();
    }
  
    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text))
        {
            PopupInfo.Instance.Popup("Du måste ange ett namn till rummet innan du kan skapa det", 5);
            return;
        }

        if (string.IsNullOrEmpty(createNickNameInputField.text))
        {
            PopupInfo.Instance.Popup("Du måste ange ett smeknamn innan du kan skapa ett rum", 5);
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.PublishUserId = true;
        string[] roomPropsLobby = new string[maxPlayers+2];
        for (int i = 0; i < maxPlayers; i++)
        {
            roomPropsLobby[i] = "p"+i;
        }
        roomPropsLobby[maxPlayers] = "visible";
        roomPropsLobby[maxPlayers+1] = "gOver";
        roomOptions.CustomRoomPropertiesForLobby = roomPropsLobby;
        Hashtable hash = new Hashtable();
        hash.Add("visible", true);
        hash.Add("gOver", false);
        hash.Add("gameStarted", false);
        hash.Add("score", 0);
        roomOptions.CustomRoomProperties = hash;
        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        PhotonNetwork.NickName = createNickNameInputField.text;
        MenuManager.Instance.OpenMenu("loading");
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartAnyways()
    {
        startAnyways = true;
        notFourPlayersContainer.SetActive(false);
        StartGame();
        startAnyways = false;
    }

    public void CancelStart()
    {
        notFourPlayersContainer.SetActive(false);
    }

    public void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            PopupInfo.Instance.Popup("Du kan inte starta spelet ensam!", 5);
            return;
        }
        
        if (PhotonNetwork.CurrentRoom.PlayerCount != maxPlayers && !startAnyways && !startGamePressed)
        {
            notFourPlayersContainer.SetActive(true);
            return;
        }

        if (startGamePressed) return;

        startGamePressed = true;
        tutorial = false;
        SetLists();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Hashtable hash = new Hashtable();

            int randomCharacterIndex = Random.Range(0, characterList.Count);
            string character = characterList[randomCharacterIndex];
            characterList.RemoveAt(randomCharacterIndex);
            hash.Add(PhotonNetwork.PlayerList[i].UserId+"Character", character);

            int randomSpawnPointIndex = Random.Range(0, spawnPointList.Count);
            int spawnPoint = spawnPointList[randomSpawnPointIndex];
            spawnPointList.RemoveAt(randomSpawnPointIndex);
            hash.Add(PhotonNetwork.PlayerList[i].UserId+"SpawnPoint", spawnPoint);
            
            hash.Add("p"+i, PhotonNetwork.PlayerList[i].UserId);
            hash.Add("visible", false);

            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (tutorial) return;

        // When the last player has updated the room properties -> Start the game.
        string lastPlayerUserID = PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length-1].UserId;
        if (propertiesThatChanged[lastPlayerUserID+"Character"] != null) PhotonNetwork.LoadLevel(1);
        startGamePressed = false;
    }

    public void RejoinRoom()
    {
        PhotonNetwork.JoinRoom(latestRoomName);
        MenuManager.Instance.OpenMenu("loading");
        rejoinCalled = true;
    }

    public void AbandonGame()
    {
        rejoinContainer.SetActive(false);
        abandonedRooms.Add(latestRoomName);
        latestRoomName = "";
    }

    public void JoinRoom(RoomInfo info)
    {        
        if (string.IsNullOrEmpty(findNickNameInputField.text))
        {
            PopupInfo.Instance.Popup("Du måste ange ett smeknamn innan du kan gå med i rummet", 5);
            return;
        }
        
        PhotonNetwork.JoinRoom(info.Name);
        PhotonNetwork.NickName = findNickNameInputField.text;       
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        if(PhotonNetwork.OfflineMode)
        {
            MenuManager.Instance.OpenMenu("loading");

            Hashtable hash = new Hashtable();
            hash.Add("maxHealth", 5);
            hash.Add("baseTime", 360);
            hash.Add("amountMultiplier", 100);
            hash.Add("taskDelay", 30000f);
            hash.Add("gameStarted", true);
            hash.Add("score", 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            PhotonNetwork.LoadLevel(2);
            return;
        }
        if (rejoinCalled)
        {
            // UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            // if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(1);
            if (PhotonNetwork.IsMasterClient) UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            return;
        }
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu("room");

        SetNick(PhotonNetwork.LocalPlayer);
        PlayerPrefs.SetString("latestNick", PhotonNetwork.LocalPlayer.NickName);
        object[] initData = {PhotonNetwork.LocalPlayer.NickName};
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "MainMenu", "PlayerListItem"), Vector3.zero, Quaternion.identity, 0, initData);

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        settingsContainer.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        switch (returnCode)
        {
            // User does not exist in this game. (Rejoin error)
            case 32748:
                PhotonNetwork.JoinRoom(latestRoomName);
                return;

            // User already in game but not rejoining.
            case 32749:
                PhotonNetwork.RejoinRoom(latestRoomName);
                return;
        }
        errorText.text = "Join Room Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(false);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        settingsContainer.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }

    // ------------------------------------------------------ Helper Methods ------------------------------------------------------

    void SetLists()
    {
        characterList = new List<string> {"Long", "Normal", "Strong", "Fast"};
        spawnPointList = new List<int>();
        for (int i = 0; i < maxPlayers; i++)
        {
            spawnPointList.Add(i);
        }
    }

    void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    Destroy(cachedRoomList[info.Name].gameObject);
                    cachedRoomList.Remove(info.Name);
                }
                if (abandonedRooms.Contains(info.Name)) abandonedRooms.Remove(info.Name);
            }
            else
            {
                if (!cachedRoomList.ContainsKey(info.Name))
                {
                    RoomListItem item = Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>();
                    if (item != null)
                    {
                        bool visible = (bool) info.CustomProperties["visible"];
                        item.SetUp(info);
                        cachedRoomList[info.Name] = item;
                        item.gameObject.SetActive(visible);
                    }
                }
            }
        }
        rejoinContainer.SetActive(CanRejoin(cachedRoomList)); 
    }

    bool CanRejoin(Dictionary<string, RoomListItem> roomList)
    {
        foreach (RoomListItem item in roomList.Values)
        {
            if (CanRejoinRoom(item.info)) return true;
        }
        return false;
    }

    bool CanRejoinRoom(RoomInfo info)
    {
        if (abandonedRooms.Contains(info.Name)) return false;

        bool gameOver = (bool) info.CustomProperties["gOver"];
        if (gameOver == true) return false;

        for (int i = 0; i < maxPlayers; i++)
        {
            string userID = (string) info.CustomProperties["p"+i];
            if (userID == PhotonNetwork.LocalPlayer.UserId)
            {
                latestRoomName = info.Name;
                return true;
            }
        }
        return false;
    }

    void SetNick(Player player) 
    {
        if(!IsNameTaken(player.NickName)) return;

        int i = 0;
        string temp = player.NickName; 
        string newName = player.NickName;        
        while (IsNameTaken(temp))
        {   
            i++;
            temp = newName + i;            
        }
        newName += i;
        player.NickName = newName;
    }

    bool IsNameTaken(string name)
    {
        foreach (Player player in PhotonNetwork.PlayerListOthers)
        {
            if(player.NickName.Equals(name)) 
            {
                return true;
            }
        }
        return false;
    }

    void ClearCachedRooms()
    {
        foreach (RoomListItem item in cachedRoomList.Values)
        {
            Destroy(item.gameObject);
        }
        cachedRoomList.Clear();
    }


    // ------------------------------------------------------ Tutorial Methods ------------------------------------------------------

    public void StartTutorialScene()
    {
        tutorial = true;
        PhotonNetwork.Disconnect();
    }
}

