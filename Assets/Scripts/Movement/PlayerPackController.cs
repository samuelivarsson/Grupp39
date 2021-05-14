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

    void Pack(ProductController productController)
    {
        latestPackage = latestCollision;
        float eulerY = PlayerLiftController.ClosestAngle(latestPackage.transform.rotation.eulerAngles.y);
        PV.RPC("OnPack", RpcTarget.AllViaServer, productController.GetComponent<PhotonView>().ViewID, latestPackage.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnPack(int productViewID, int packageViewID, float eulerY)
    {
        GameObject productControllerObj = PhotonView.Find(productViewID).gameObject;
        ProductController prd = productControllerObj.GetComponent<ProductController>();
        GameObject packageControllerObj = PhotonView.Find(packageViewID).gameObject;
        PackageController pkg = packageControllerObj.GetComponent<PackageController>();

        Vector3 pos;

        // Set position based on what's open and increment productCount
        if (!pkg.bpic1)
        {
            pos = pkg.pic1.transform.localPosition;
            pkg.bpic1 = true;
            pkg.productCount++;
        }
        else if (!pkg.bpic2)
        {
            pos = pkg.pic2.transform.localPosition;
            pkg.bpic2 = true;
            pkg.productCount++;
        }
        else if (!pkg.bpic3)
        {
            pos = pkg.pic3.transform.localPosition;
            pkg.bpic3 = true;
            pkg.productCount++;
        }
        else return;

        // Make child and set scale & rotation
        prd.transform.parent = pkg.transform;
        prd.transform.localScale = prd.transform.localScale*prd.scaleMultiplier;
        prd.transform.rotation = Quaternion.Euler(0, eulerY, 0);
        prd.transform.localPosition = pos;

        // Set booleans and liftingID
        prd.isLifted = false;
        prd.isPackaged = true;
        canPackID = -1;
        latestCollision = null;
        GetComponent<PlayerLiftController>().liftingID = -1;
    }

    void Tape(PackageController packageController)
    {
        PV.RPC("OnTape", RpcTarget.AllViaServer, packageController.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnTape(int packageViewID)
    {
        GameObject packageControllerObj = PhotonView.Find(packageViewID).gameObject;
        PackageController pkg = packageControllerObj.GetComponent<PackageController>();

        if (pkg.isTaped || !pkg.canTape) return;

        pkg.timebar.enabled = true;
        pkg.isTaped = true;
        pkg.canTape = false;
        isTaping = true;
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
