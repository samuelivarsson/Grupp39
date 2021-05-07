using UnityEngine;
using Photon.Pun;

public class PlayerPackController : MonoBehaviour
{
    PhotonView PV;

    // Latest package collided with
    public GameObject latestCollision {get; set;}
    // Latest package that you packed something to
    GameObject latestPackage;
    public int canPackID {get; set;}
    public int canTapeID {get; set;}
    public bool isTaping { get; set; } = false;

    static Vector3 packScale = new Vector3(0.3f, 0.3f, 0.3f);

    void Awake()
    {
        PV = GetComponent<PhotonView>();
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
        if (Input.GetKeyDown(PlayerController.packButton) && CanPack(latestColViewID) && !productController.isPackaged && packageController.productCount < 3 && !packageController.isTaped)
        {
            Pack(productController);
        }
    }

    void CheckTape()
    {
        if (!latestCollision) return;

        int latestColViewID = latestCollision.GetComponent<PhotonView>().ViewID;
        PackageController packageController = latestCollision.GetComponent<PackageController>();    
        if (Input.GetKeyDown(PlayerController.tapeButton) && CanTape(latestColViewID) && !packageController.isTaped && packageController.canTape && packageController.productCount > 0)
        {
            Tape(packageController);
        }
    }

    void _Pack(PackageController pkg, ProductController prd, float eulerY)
    {
        // Make child and set scale & rotation
        prd.transform.parent = pkg.transform;
        prd.transform.localScale = prd.transform.localScale*prd.scaleMultiplier;
        prd.transform.rotation = Quaternion.Euler(0, eulerY, 0);

        // Set position based on what's open and increment productCount
        if (!pkg.bpic1)
        {
            prd.transform.localPosition = pkg.pic1.transform.localPosition;
            pkg.bpic1 = true;
            pkg.productCount++;
        }
        else if (!pkg.bpic2)
        {
            prd.transform.localPosition = pkg.pic2.transform.localPosition;
            pkg.bpic2 = true;
            pkg.productCount++;
        }
        else if (!pkg.bpic3)
        {
            prd.transform.localPosition = pkg.pic3.transform.localPosition;
            pkg.bpic3 = true;
            pkg.productCount++;
        }

        // Set booleans and liftingID
        prd.isLifted = false;
        prd.isPackaged = true;
        GetComponent<PlayerLiftController>().liftingID = -1;
    }

    void Pack(ProductController productController)
    {
        latestPackage = latestCollision;
        float eulerY = PlayerLiftController.ClosestAngle(latestPackage.transform.rotation.eulerAngles.y);
        _Pack(latestPackage.GetComponent<PackageController>(), productController, eulerY);
        PV.RPC("OnPack", RpcTarget.OthersBuffered, productController.GetComponent<PhotonView>().ViewID, latestPackage.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnPack(int productViewID, int packageViewID, float eulerY)
    {
        GameObject productControllerObj = PhotonView.Find(productViewID).gameObject;
        ProductController productController = productControllerObj.GetComponent<ProductController>();
        GameObject packageControllerObj = PhotonView.Find(packageViewID).gameObject;
        PackageController packageController = packageControllerObj.GetComponent<PackageController>();

        _Pack(packageController, productController, eulerY);
    }

    void _Tape(PackageController pkg)
    {
        pkg.timebar.enabled = true;
        pkg.isTaped = true;
        pkg.canTape = false;
        isTaping = true;
    }

    void Tape(PackageController packageController)
    {
        _Tape(packageController);
        PV.RPC("OnTape", RpcTarget.OthersBuffered, packageController.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnTape(int packageViewID)
    {
        GameObject packageControllerObj = PhotonView.Find(packageViewID).gameObject;
        PackageController packageController = packageControllerObj.GetComponent<PackageController>();
        _Tape(packageController);
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
