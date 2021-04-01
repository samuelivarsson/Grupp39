using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class PackageController : MonoBehaviour, LiftablePackage
{
    public bool isLifted {get; set;} = false;
    public bool isTaped {get; set;} = false;

    PhotonView PV;
    public Transform pic1 {get; set;}
    public Transform pic2 {get; set;}
    public Transform pic3 {get; set;}

    [SerializeField] public Image timebar;
    TapeTimer tapeTimer;
    public bool bpic1 {get; set;} = false;
    public bool bpic2 {get; set;} = false;
    public bool bpic3 {get; set;} = false;
    public int productCount {get; set;} = 0;
    public bool canTape { get; set; } = false;

    public static Vector3 tileOffset = new Vector3(1.5f, 0.35f, 1.5f);
    public static Vector3 cabinetOffset = new Vector3(1.5f, 0.6f, 1.5f);

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        pic1 = gameObject.transform.GetChild(0);
        pic2 = gameObject.transform.GetChild(1);
        pic3 = gameObject.transform.GetChild(2);
    }

    // Start is called before the first frame update
    void Start()
    {
        timebar.enabled = false;
    }

    public void OrderDelivery(GameObject latestTile)
    {
        string num = latestTile.name.Substring(latestTile.name.Length - 1);
        PackageController package = GetComponent<PackageController>();
        TaskController task = GameObject.FindGameObjectWithTag("Task"+num).GetComponent<TaskController>();
        Delivery(package, task);
    }

    private void Delivery(PackageController package, TaskController task)
    {
        if (package.CompareProductsWithTask(task))
        {
            package.Deliver(1);
            Debug.Log("The package contained all products!");
        }
        else
        {
            package.Deliver(-1);
            Debug.Log("The package did not contain all products!");
        }
    }

    public void Deliver(int score)
    {
        //droppedDeliveries.Add(gameObject);
        Destroy(gameObject);
        ScoreController.Instance.IncrementScore(score);
        PV.RPC("OnDeliver", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnDeliver()
    {
        Destroy(gameObject);
    }

    public bool CompareProductsWithTask(TaskController task)
    {
        var orderedProducts = task.GetOrderedProducts();
        var deliveredProducts = GetAllDeliveredProducts(gameObject.transform);

        foreach(string product in deliveredProducts)
        {
            if(!orderedProducts.Contains(product))
            {
                return false;
            }
        }

        return true;
    }

    public List<string> GetAllDeliveredProducts(Transform package)
    {
        List<string> deliveredProducts = new List<string>();
        
        foreach(Transform child in package)
        {
            if (child.CompareTag("ProductController"))
            {
                deliveredProducts.Add(child.GetComponent<ProductController>().type);
            }
        }

        return deliveredProducts;
    }
}
