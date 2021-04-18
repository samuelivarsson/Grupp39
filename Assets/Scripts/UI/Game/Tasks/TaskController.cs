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

    // The objects in the order
    public string[] requiredProducts {get; set;}

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
        bg.color = materials[taskNr].color;
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
        productAmount = (int) initData[2];

        requiredProducts = (string[]) initData[3];
        string text = String.Join("\n", requiredProducts);
        textProducts.text = text;

        int time = (int) initData[4];
        taskTimer.maxTime = time;
        taskTimer.timeLeft = time;
        taskTimer.timerActive = true;
    }
}
