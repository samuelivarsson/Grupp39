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
        for (int i = 1; i < 5; i++)
        {
            GameObject taskObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity);
            string tag = "Task"+i;
            taskObj.tag = tag;
            taskObj.GetComponent<TaskController>().taskNr = i;
            taskObj.GetComponent<PhotonView>().RPC("OnInit", RpcTarget.OthersBuffered, tag, i);
        }
    }
}
