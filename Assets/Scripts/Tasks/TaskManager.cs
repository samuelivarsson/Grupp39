using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class TaskManager : MonoBehaviour
{
    GameObject canvasManager;

    PhotonView PV;

    [SerializeField] bool stopGenerating = false;
    [SerializeField] float generateTime;
    [SerializeField] float generateDelay;
 
    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        PV = GetComponent<PhotonView>();
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }
        
    void Start()
    {
       if (PV.IsMine)
       {
            InvokeRepeating("CreateTasks", generateTime, generateDelay);
       }
    }

    void CreateTasks()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject task1Obj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task1"), Vector3.zero,  Quaternion.identity);
            GameObject task2Obj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task2"), Vector3.zero,  Quaternion.identity);
            GameObject task3Obj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task3"), Vector3.zero,  Quaternion.identity);
            GameObject task4Obj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task4"), Vector3.zero,  Quaternion.identity);
        }
        if (stopGenerating)
        {
            CancelInvoke("GenerateTask");
        }
    }
}
