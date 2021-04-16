using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField createNickNameInputField;
    [SerializeField] TMP_InputField findNickNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;
 
    List<string> characterList = new List<string> {"Long", "Normal", "Strong", "Weak"};
    List<int> spawnPointList = new List<int> {0, 1, 2, 3};

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //Connecting to master server (Set to eu in PhotonServerSettings)
        if(!PhotonNetwork.IsConnected) 
        {
            Debug.Log("Connecting to the master server...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        print(PhotonNetwork.InLobby);
        if(!PhotonNetwork.InLobby) 
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("Connected to the " + PhotonNetwork.CloudRegion + " server!");
            PhotonNetwork.AutomaticallySyncScene = true;
        }        
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined Lobby");
    }
  
    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text) || string.IsNullOrEmpty(createNickNameInputField.text))
        {
          return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        PhotonNetwork.NickName = createNickNameInputField.text;
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom() 
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu("room");


        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartGame()
    {
        //if (PhotonNetwork.PlayerList.Length != 4) return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Hashtable hash = new Hashtable();

            int randomCharacterIndex = Random.Range(0, characterList.Count-1);
            string character = characterList[randomCharacterIndex];
            characterList.RemoveAt(randomCharacterIndex);
            hash.Add("character", character);

            int randomSpawnPointIndex = Random.Range(0, spawnPointList.Count-1);
            int spawnPoint = spawnPointList[randomSpawnPointIndex];
            spawnPointList.RemoveAt(randomSpawnPointIndex);
            hash.Add("spawnPoint", spawnPoint);

            player.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber == PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length-1].ActorNumber) PhotonNetwork.LoadLevel(1);
    }

    
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        if(string.IsNullOrEmpty(findNickNameInputField.text) || PhotonNetwork.PlayerList.Length >= 4)
        {
          return;
        }
        PhotonNetwork.JoinRoom(info.Name);
        PhotonNetwork.NickName = findNickNameInputField.text;
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        
        for (int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].RemovedFromList) continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public void quitGame()
    {
        Application.Quit();
    }
    
}

