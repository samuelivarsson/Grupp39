using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    const int maxTasks = 4;
    const float taskDelay = 5f;

    float[] countDownTimes = new float[maxTasks];
    bool[] countDownBools = new bool[maxTasks];

    GameObject canvasManager;
    PhotonView PV;
 
    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
    }
        
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        CreateTasks();
        for (int i = 0; i < maxTasks; i++)
        {
            countDownTimes[i] = taskDelay;
            countDownBools[i] = false;
        }
    }

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < maxTasks; i++)
        {
            if (countDownBools[i])
            {
                countDownTimes[i] -= Time.fixedDeltaTime;
                if (countDownTimes[i] < 0)
                {
                    countDownTimes[i] = taskDelay;
                    countDownBools[i] = false;
                    CreateTask(i);
                }
            }
        }
    }

    void CreateTasks()
    {
        for (int i = 0; i < maxTasks; i++)
        {
            string tag = "Task"+i;
            object[] initData = {tag, i};
            GameObject taskObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity, 0, initData);
        }
    }

    public void GenerateNewTask(int i)
    {
        countDownBools[i] = true;
    }

    void CreateTask(int i)
    {
        string tag = "Task"+i;
        object[] initData = {tag, i};
        GameObject taskObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity, 0, initData);
    }
}
