using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class PackageController : MonoBehaviour
{
    bool spaceKeyWasPressed;

    Rigidbody rb;
    PhotonView PV;

    Transform player;
    PlayerController playerController;
    Transform hand;
    Transform pic1;
    Transform pic2;
    Transform pic3;

    ProductController productController;

    private bool canPackage;
    public bool cantape = false;
    [SerializeField] Image timebar;
    TapeTimer tapeTimer;
    bool bpic1 = false;
    bool bpic2 = false;
    bool bpic3 = false;
    int productCount = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        pic1 = gameObject.transform.GetChild(0);
        pic2 = gameObject.transform.GetChild(1);
        pic3 = gameObject.transform.GetChild(2);
        player = PlayerManager.myPlayerController.transform;
        hand = player.GetChild(0);
        playerController = player.GetComponent<PlayerController>();
        productController = GetComponent<ProductController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        timebar.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckPacking(); 

        if (Input.GetKeyDown(KeyCode.LeftShift) && productController.GetCanPickUp())
        {
            timebar.enabled = true;
            cantape = true;
        }
    }

    private void CheckPacking()
    {
        if (canPackage)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && player.GetComponentInChildren<ProductController>() && productCount < 3)
            {
                ProductController prodController = player.GetComponentInChildren<ProductController>();
                playerController.SetLiftingID(-1);
                prodController.SetIsLifted(false);
                prodController.SetIsPackaged(true);
                Transform prod = prodController.transform;
                player.GetComponentInChildren<ProductController>().transform.parent = gameObject.transform;
                prod.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                if (!bpic1)
                {
                    prod.localPosition = pic1.transform.localPosition;
                    bpic1 = true;
                    productCount++;
                }
                else if (!bpic2)
                {
                    prod.localPosition = pic2.transform.localPosition;
                    bpic2 = true;
                    productCount++;
                }
                else if (!bpic3)
                {
                    prod.localPosition = pic3.transform.localPosition;
                    bpic3 = true;
                    productCount++;
                }
                PV.RPC("OnPacketing", RpcTarget.OthersBuffered, prodController.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    void OnPacketing(int viewID)
    {
        GameObject producController = PhotonView.Find(viewID).gameObject;
        producController.transform.parent = gameObject.transform;
        producController.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        if (bpic1)
        {
            producController.transform.localPosition = pic1.transform.localPosition;
        }
        if (bpic2)
        {
            producController.transform.localPosition = pic2.transform.localPosition;
        }
        if (bpic3)
        {
            producController.transform.localPosition = pic3.transform.localPosition;
        }
      
    }

    public void Deliver(int score)
    {
        //droppedDeliveries.Add(gameObject);
        Destroy(gameObject);
        ScoreController.Instance.IncrementScore(score);
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
                deliveredProducts.Add(child.GetComponent<ProductController>().GetProductType());
            }
        }

        return deliveredProducts;
    }

    [PunRPC]
    void OnDeliver()
    {
        Destroy(gameObject);
    }

    public void SetCanPackage(bool _canPackage)
    {
        canPackage = _canPackage;
    }

    public bool getCanTape()
    {
        return cantape;
    }
}
