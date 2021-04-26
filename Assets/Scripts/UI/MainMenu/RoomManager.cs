using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    PhotonView PV;

    int playersLoaded = 0;
    
    void Awake()
    {
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex == 1) // We're in the game scene
        {
            DisconnectHandler.latestRoomName = PhotonNetwork.CurrentRoom.Name;
            DisconnectHandler.inGame = true;
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerManager"), Vector3.zero, Quaternion.identity);
            if (PhotonNetwork.IsMasterClient) 
            {
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "ObjectManager"), Vector3.zero, Quaternion.identity);
                playersLoaded++;
                if (playersLoaded == PhotonNetwork.CurrentRoom.PlayerCount) PV.RPC("OnAllLoaded", RpcTarget.AllViaServer);
            }
            else PV.RPC("OnLoaded", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void OnLoaded()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playersLoaded++;
        if (playersLoaded == PhotonNetwork.CurrentRoom.PlayerCount) PV.RPC("OnAllLoaded", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void OnAllLoaded()
    {
        TaskManager.Instance.startCountDown = true;
        CanvasManager.Instance.countDownObj.SetActive(true);
    }
}
