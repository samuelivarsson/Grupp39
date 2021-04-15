using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.IO;

public class CanvasManager : MonoBehaviourPunCallbacks
{
    public static CanvasManager Instance;
    Menu gameOverMenu;
    Menu escMenu;
    PhotonView PV;
    [SerializeField] TMP_Text endScore;

    public static KeyCode escButton = KeyCode.Escape;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        gameOverMenu = transform.GetChild(0).GetComponent<Menu>();
        escMenu = transform.GetChild(1).GetComponent<Menu>();
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
        
    void Start()
    {
       if (PV.IsMine) CreateUI();
    }

    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !escMenu.open && !gameOverMenu.open) 
        {
            OpenEscMenu();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && escMenu.open && !gameOverMenu.open) CloseEscMenu();
    }

    void CreateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject healthObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "HealthController"), Vector3.zero, Quaternion.identity);
            GameObject scoreObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "ScoreController"), Vector3.zero,  Quaternion.identity);
            GameObject taskObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "TaskManager"), Vector3.zero,  Quaternion.identity);
        }
    }

    
    public void LeaveRoom()
    {
        PhotonNetwork.Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void OpenGameOverMenu() 
    {   
        endScore.text = gameObject.GetComponentInChildren<ScoreController>().score.ToString();
        PauseGame();        
        gameOverMenu.Open();
    }

    public void OpenEscMenu() 
    {
        escMenu.Open();
    }

    public void CloseEscMenu() 
    {        
        escMenu.Close();
    }

    public void PauseGame()
    {
        foreach (Transform child in transform)
            if(child.gameObject.activeInHierarchy) Destroy(child.gameObject);
        
        foreach (GameObject players in GameObject.FindGameObjectsWithTag("PlayerController"))
            Destroy(players);
    }
}
