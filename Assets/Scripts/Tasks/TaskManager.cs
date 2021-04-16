using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class TaskManager : MonoBehaviour
{
    GameObject canvasManager;

    PhotonView PV;
 
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
        if (PhotonNetwork.IsMasterClient)
        {
            CreateTasks();
        }
    }

    void CreateTasks()
    {
        GameObject task1Obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity);
        task1Obj.tag = "Task1";
        task1Obj.GetComponent<TaskController>().taskNr = 1;
        GameObject task2Obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity);
        task2Obj.tag = "Task2";
        task2Obj.GetComponent<TaskController>().taskNr = 2;
        GameObject task3Obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity);
        task3Obj.tag = "Task3";
        task3Obj.GetComponent<TaskController>().taskNr = 3;
        GameObject task4Obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity);
        task4Obj.tag = "Task4";
        task4Obj.GetComponent<TaskController>().taskNr = 4;
    }
}
