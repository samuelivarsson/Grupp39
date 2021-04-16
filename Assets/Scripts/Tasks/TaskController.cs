using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class TaskController : MonoBehaviourPunCallbacks
{
    GameObject canvasManager;
    [SerializeField] Material[] materials;
    [SerializeField] Image bg;
    public int taskNr {get; set;}
    [SerializeField] Text textProducts;
    TaskTimer taskTimer;
    PhotonView PV;

    // The objects in the order
    public List<string> orderedProducts {get; set;} = new List<string>();

    private List<string> possibleProducts = new List<string>() {"Blue", "Red", "Cyan", "Green", "Yellow", "Pink"};

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        taskTimer = GetComponentInChildren<TaskTimer>();
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
    }

    void Start()
    {
        GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-160, -110 - 250 * (taskNr-1), 0);
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        GenerateOrderedProducts();
        bg.color = materials[taskNr-1].color;
    }

    private void GenerateOrderedProducts()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            System.Random rnd = new System.Random();
            //int amount = rnd.Next(3) + 1;
            int amount = UnityEngine.Random.Range(1, 3);

            for(int i = 0; i < amount; i++)
            {
                orderedProducts.Add(possibleProducts[UnityEngine.Random.Range(0, possibleProducts.Count-1)]);
            }

            string text = String.Join("\n", orderedProducts);
            textProducts.text = text;
            int time = 20 + (amount * 20);
            taskTimer.maxTime = time;
            taskTimer.timeLeft = time;
            taskTimer.startTimer = true;

            PV.RPC("OnGenerateOrderedProducts", RpcTarget.OthersBuffered, text, time);
        }
    }

    [PunRPC]
    void OnGenerateOrderedProducts(string text, int time)
    {
        print("I am here");

        textProducts.text = text;
        orderedProducts = text.Split('\n').ToList();

        taskTimer.maxTime = time;
        taskTimer.timeLeft = time;
        taskTimer.startTimer = true;
    }

    [PunRPC]
    void OnInit(string tag, int _taskNr)
    {
        gameObject.tag = tag;
        taskNr = _taskNr;
    }

    public enum TaskColors
    {
        Red = 1,
        Blue = 2,
        Pink = 3,
        Green = 4
    }
}
