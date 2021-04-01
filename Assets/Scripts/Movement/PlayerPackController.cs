using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerPackController : MonoBehaviour
{
    PhotonView PV;
    PlayerLiftController playerLiftController;
    PlayerController playerController;

    // Latest package collided with
    public GameObject latestCollision {get; set;}
    // Latest package that you packed something to
    GameObject latestPackage;
    public int canPackID {get; set;}
    public int canTapeID {get; set;}
    public bool isTaping { get; set; } = false;


    void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerLiftController = GetComponentInParent<PlayerLiftController>();
        playerController = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        CheckPack();
        CheckTape();
    }

    void CheckPack()
    {
        if (!latestCollision) return;

        int latestColViewID = latestCollision.GetComponent<PhotonView>().ViewID;
        ProductController productController = GetComponentInChildren<ProductController>();
        // If you have no product in your hand -> return
        if (productController == null) return;
        
        PackageController packageController = latestCollision.GetComponent<PackageController>();
        if (Input.GetKeyDown(PlayerController.packButton) && CanPack(latestColViewID) && !productController.isPackaged && packageController.productCount < 3)
        {
            Pack(productController);
        }
    }

    void CheckTape()
    {
        if (!latestCollision) return;

        int latestColViewID = latestCollision.GetComponent<PhotonView>().ViewID;
        PackageController packageController = latestCollision.GetComponent<PackageController>();    
        if (Input.GetKeyDown(PlayerController.tapeButton) && CanTape(latestColViewID) && !packageController.isTaped && packageController.canTape)
        {
            Tape(packageController);
        }
       

        
    }

    void Pack(ProductController productController)
    {
        latestPackage = latestCollision;
        productController.transform.parent = latestPackage.gameObject.transform;
        productController.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        PlayerManager.myPlayerLiftController.liftingID = -1;
        productController.isLifted = false;
        productController.isPackaged = true;

        PackageController packageController = latestPackage.GetComponent<PackageController>();
        if (!packageController.bpic1)
        {
            productController.transform.localPosition = packageController.pic1.transform.localPosition;
            packageController.bpic1 = true;
            packageController.productCount++;
        }
        else if (!packageController.bpic2)
        {
            productController.transform.localPosition = packageController.pic2.transform.localPosition;
            packageController.bpic2 = true;
            packageController.productCount++;
        }
        else if (!packageController.bpic3)
        {
            productController.transform.localPosition = packageController.pic3.transform.localPosition;
            packageController.bpic3 = true;
            packageController.productCount++;
        }
        PV.RPC("OnPack", RpcTarget.OthersBuffered, productController.GetComponent<PhotonView>().ViewID, latestPackage.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnPack(int productViewID, int packageViewID)
    {
        GameObject productControllerObj = PhotonView.Find(productViewID).gameObject;
        ProductController productController = productControllerObj.GetComponent<ProductController>();
        GameObject packageControllerObj = PhotonView.Find(packageViewID).gameObject;
        PackageController packageController = packageControllerObj.GetComponent<PackageController>();

        productController.isLifted = false;
        productController.isPackaged = true;

        productControllerObj.transform.parent = packageControllerObj.transform;
        productControllerObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        if (!packageController.bpic1)
        {
            productControllerObj.transform.localPosition = packageController.pic1.transform.localPosition;
            packageController.bpic1 = true;
            packageController.productCount++;
        }
        else if (!packageController.bpic2)
        {
            productControllerObj.transform.localPosition = packageController.pic2.transform.localPosition;
            packageController.bpic2 = true;
            packageController.productCount++;
        }
        else if (!packageController.bpic3)
        {
            productControllerObj.transform.localPosition = packageController.pic3.transform.localPosition;
            packageController.bpic3 = true;
            packageController.productCount++;
        }
    }

    void Tape(PackageController packageController)
    {
        packageController.timebar.enabled = true;
        packageController.isTaped = true;
        packageController.canTape = false;
        isTaping = true;
        PV.RPC("OnTape", RpcTarget.OthersBuffered, packageController.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnTape(int packageViewID)
    {
        GameObject packageControllerObj = PhotonView.Find(packageViewID).gameObject;
        PackageController packageController = packageControllerObj.GetComponent<PackageController>();
        isTaping = true;
        packageController.timebar.enabled = true;
        packageController.isTaped = true;
    }

    bool CanPack(int _canPackID)
    {
        return canPackID == _canPackID;
    }

    bool CanTape(int _canTapeID)
    {
        return canTapeID == _canTapeID;
    }
}
