using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // Maximum amount of products per task
    const int maxProducts = 3;

    // Maximum number of tasks at the same time
    const int maxTasks = 4;

    // Delay in seconds before a new tasks spawns
    const float taskDelay = 5f;

    // Time in seconds
    const int baseTime = 60;
    const int amountMultiplier = 20;

    // The different products the task can require
    static List<string> possibleProducts = new List<string>() {"Blue", "Red", "Cyan", "Green", "Yellow", "Pink"};

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
            CreateTask(i);
        }
    }

    public void GenerateNewTask(int i)
    {
        countDownTimes[i] = taskDelay;
        countDownBools[i] = true;
    }

    void CreateTask(int i)
    {
        string tag = "Task"+i;
        int productAmount = Random.Range(1, maxProducts+1);
        string[] requiredProducts = GenerateRequiredProducts(productAmount);
        int time = baseTime + (productAmount * amountMultiplier);
        object[] initData = {tag, i, productAmount, requiredProducts, time};
        GameObject taskObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Tasks", "Task"), Vector3.zero,  Quaternion.identity, 0, initData);
    }

    string[] GenerateRequiredProducts(int productAmount)
    {
        string[] requiredProducts = new string[productAmount];
        for(int i = 0; i < productAmount; i++)
        {
            requiredProducts[i] = possibleProducts[Random.Range(0, possibleProducts.Count)];
        }
        return requiredProducts;
    }
}
