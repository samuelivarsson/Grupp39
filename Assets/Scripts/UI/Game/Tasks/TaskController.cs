using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using String = System.String;
using UnityEngine.UI;

public class TaskController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public int taskNr {get; set;}
    public int productAmount {get; set;}

    [SerializeField] Material[] materials;
    [SerializeField] Image bg;
    [SerializeField] Text textProducts;

    GameObject canvasManager;
    TaskTimer taskTimer;
    PhotonView PV;

    // Maximum amount of products per task
    static int maxProducts = 3;

    // Time
    static int baseTime = 30;
    static int amountMultiplier = 20;

    // The different products the task can require
    static List<string> possibleProducts = new List<string>() {"Blue", "Red", "Cyan", "Green", "Yellow", "Pink"};

    // The objects in the order
    public HashSet<string> requiredProducts {get; set;} = new HashSet<string>();

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        taskTimer = GetComponentInChildren<TaskTimer>();
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
    }

    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = new Vector3(-160, -110 - 250 * taskNr, 0);
        rectTransform.localScale = new Vector3(1, 1, 1);
        if (PhotonNetwork.IsMasterClient) GenerateRequiredProducts();
        bg.color = materials[taskNr].color;
    }

    void GenerateRequiredProducts()
    {
        productAmount = Random.Range(1, maxProducts);

        for(int i = 0; i < productAmount; i++)
        {
            requiredProducts.Add(possibleProducts[Random.Range(0, possibleProducts.Count-1)]);
        }

        string text = String.Join("\n", requiredProducts);
        textProducts.text = text;
        int time = baseTime + (productAmount * amountMultiplier);
        taskTimer.maxTime = time;
        taskTimer.timeLeft = time;
        taskTimer.timerActive = true;

        PV.RPC("OnGenerateRequiredProducts", RpcTarget.OthersBuffered, text, time);
    }

    [PunRPC]
    void OnGenerateRequiredProducts(string text, int time)
    {
        textProducts.text = text;
        requiredProducts = new HashSet<string>(text.Split('\n'));

        taskTimer.maxTime = time;
        taskTimer.timeLeft = time;
        taskTimer.timerActive = true;
    }

    [PunRPC]
    void DestroyTask()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.Destroy(gameObject);
        TaskManager.Instance.GenerateNewTask(taskNr);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        string tag = (string) initData[0];
        gameObject.tag = tag;
        taskNr = (int) initData[1];
    }
}
