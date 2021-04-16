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

    PhotonView PV;

    // The objects in the order
    public List<string> orderedProducts {get; set;} = new List<string>();

    private List<string> possibleProducts = new List<string>() {"Blue", "Red", "Cyan", "Green", "Yellow", "Pink"};

    void Awake()
    {
        PV = GetComponent<PhotonView>();
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
            int timer = 20 + (amount * 20);
            GetComponentInChildren<TaskTimer>().maxTime = timer;

            PV.RPC("OnGenerateOrderedProducts", RpcTarget.OthersBuffered, text, timer);
        }
    }

    [PunRPC]
    void OnGenerateOrderedProducts(string _text, int _amount)
    {
        print("I am here");

        textProducts.text = _text;
        orderedProducts = _text.Split('\n').ToList();

        GetComponentInChildren<TaskTimer>().maxTime = _amount;
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
