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
    [SerializeField] int taskNr;
    [SerializeField] Text textProducts;

    PhotonView PV;

    // The objects in the order
    private List<string> orderedProducts = new List<string>();

    private List<string> possibleProducts = new List<string>() { "Blå", "Röd", "Turkos", "Grön", "Gul", "Rosa"};

    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        PV = GetComponent<PhotonView>();
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-160, -110 - 250 * (taskNr-1), 0);
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        GenerateOrderedProducts();
    }

    // Update is called once per frame
    void Update()
    {
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

    public List<string> GetOrderedProducts()
    {
        return orderedProducts;
    }
}
