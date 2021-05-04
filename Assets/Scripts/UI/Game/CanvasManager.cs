using UnityEngine;
using TMPro;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.IO;

public class CanvasManager : MonoBehaviourPunCallbacks
{
    public static CanvasManager Instance;
    
    [SerializeField] TMP_Text endScore;
    public GameObject countDownObj;
    public TMP_Text countDownText;

    Menu gameOverMenu;
    Menu escMenu;
    PhotonView PV;

    bool intentionalLeave = false;

    public static KeyCode escButton = KeyCode.Escape;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        gameOverMenu = transform.GetChild(0).GetComponent<Menu>();
        escMenu = transform.GetChild(1).GetComponent<Menu>();
        Instance = this;
    }
        
    void Start()
    {
       if (PhotonNetwork.IsMasterClient) CreateUI();
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !escMenu.open && !gameOverMenu.open) OpenEscMenu();
        else if (Input.GetKeyDown(KeyCode.Escape) && escMenu.open && !gameOverMenu.open) CloseEscMenu();
    }

    void CreateUI()
    {
        GameObject healthObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Health", "HealthManager"), Vector3.zero, Quaternion.identity);
        GameObject scoreObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "ScoreController"), Vector3.zero,  Quaternion.identity);
        GameObject taskObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "TaskManager"), Vector3.zero,  Quaternion.identity);
    }
    
    public void LeaveRoom()
    {
        intentionalLeave = true;
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom(false);
    }

    public override void OnLeftRoom()
    {
        if (intentionalLeave)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            intentionalLeave = false;
            DisconnectHandler.inGame = false;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void _GameOver()
    {
        // Destory own player
        PhotonNetwork.Destroy(PlayerManager.myPlayerLiftController.gameObject);

        // Destory UI objects
        foreach (Transform child in transform)
            if(child.gameObject.activeInHierarchy) Destroy(child.gameObject);

        OpenGameOverMenu();
    }

    public void GameOver()
    {
        _GameOver();
        Hashtable hash = new Hashtable();
        hash.Add("gOver", true);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        PV.RPC("OnGameOver", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnGameOver()
    {
        _GameOver();
    }

    void OpenGameOverMenu() 
    {   
        endScore.text = gameObject.GetComponentInChildren<ScoreController>().score.ToString();       
        gameOverMenu.Open();
    }

    void OpenEscMenu() 
    {
        escMenu.Open();
    }

    void CloseEscMenu() 
    {        
        escMenu.Close();
    }
}
