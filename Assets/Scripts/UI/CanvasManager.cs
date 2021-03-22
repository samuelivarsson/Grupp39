using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();

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

    void CreateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject scoreObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "ScoreController"), Vector3.zero,  Quaternion.identity);
            GameObject taskObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "TaskManager"), Vector3.zero,  Quaternion.identity);
        }
    }
}
