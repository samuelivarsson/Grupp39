using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.UI;

public class TaskController : MonoBehaviour
{
    GameObject canvasManager;
    [SerializeField] int taskNr;
    [SerializeField] Text textProducts;

    PhotonView PV;

    // The objects in the order
    private List<string> orderedProducts = new List<string>();

    private List<string> possibleProducts = new List<string>() { "Blue", "Red", "Cyan", "Green", "Yellow", "Pink"};

    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        PV = GetComponent<PhotonView>();
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-160, -110 - 250 * (taskNr-1), 0);
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        GenerateOrderedProducts();
        textProducts.text = String.Join("\n", orderedProducts);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void GenerateOrderedProducts()
    {
        System.Random rnd = new System.Random();
        int amount = rnd.Next(3) + 1;

        for(int i = 0; i < amount; i++)
        {
            orderedProducts.Add(possibleProducts[rnd.Next(possibleProducts.Count)]);
        }
        Debug.Log(String.Join(" ", orderedProducts));
        Debug.Log(amount);
        GetComponentInChildren<TaskTimer>().maxTime = 10 + (amount * 10);
        Debug.Log(GetComponentInChildren<TaskTimer>().maxTime);
    }

    public List<string> GetOrderedProducts()
    {
        return orderedProducts;
    }
}
