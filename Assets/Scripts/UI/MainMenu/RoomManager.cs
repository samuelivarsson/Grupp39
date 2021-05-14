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
            else
            {
                bool gameStarted = (bool) PhotonNetwork.CurrentRoom.CustomProperties["gameStarted"];
                if (!gameStarted) PV.RPC("OnLoaded", RpcTarget.MasterClient);
                // else GetGameState();
            }
        }
        if(scene.buildIndex == 2) // Tutorial scene
        {
            DisconnectHandler.latestRoomName = PhotonNetwork.CurrentRoom.Name;
            DisconnectHandler.inGame = true;
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerManager"), Vector3.zero, Quaternion.identity);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "ObjectManager"), Vector3.zero, Quaternion.identity);
            }
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

    // void GetGameState()
    // {
    //     foreach (PhotonView view in PhotonNetwork.PhotonViewCollection)
    //     {
    //         if (view.IsRoomView) PV.RPC("GetTransform", RpcTarget.MasterClient, view.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
    //     }
    // }

    // [PunRPC]
    // void GetTransform(int viewID, int senderActorNumber)
    // {
    //     GameObject obj = PhotonView.Find(viewID).gameObject;
    //     Transform trans = obj.transform;
    //     string parentName = transform.parent == null ? "" : transform.parent.name;
    //     Vector3 pos = trans.position;
    //     Quaternion rot = trans.rotation;
    //     Vector3 scale = trans.localScale;
    //     object[] data = {parentName, pos, rot, scale};
    //     PV.RPC("TransformResponse", RpcTarget.Others, viewID, senderActorNumber, data);
    // }

    // [PunRPC]
    // void TransformResponse(int viewID, int senderActorNumber, object[] data)
    // {
    //     if (PhotonNetwork.LocalPlayer.ActorNumber != senderActorNumber) return;

    //     string parentName = (string) data[0];
    //     Vector3 pos = (Vector3) data[1];
    //     Quaternion rot = (Quaternion) data[2];
    //     Vector3 scale = (Vector3) data[3];
    //     GameObject obj = PhotonView.Find(viewID).gameObject;
    //     Transform trans = obj.transform;
    //     Transform parent = parentName == "" ? null : GameObject.Find(parentName).transform;
    //     trans.parent = parent;
    //     trans.position = pos;
    //     trans.rotation = rot;
    //     trans.localScale = scale;
    //     Debug.LogError("parent: "+parentName);
    //     Debug.LogError("pos: "+pos);
    //     Debug.LogError("rot: "+rot);
    //     Debug.LogError("scale: "+scale);
    // }
}
