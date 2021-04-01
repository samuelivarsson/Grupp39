using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLiftController : MonoBehaviour
{
    public int liftingID {get; set;} = -1;
    public int canLiftID {get; set;} = -1;
    
    public GameObject latestTile {get; set;}

    public GameObject latestCollision {get; set;}
    GameObject latestObject;
    Transform hand;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        hand = gameObject.transform.GetChild(0);
    }

    void Update()
    {
        if (!PV.IsMine) return;

        CheckLiftAndDrop();
    }

    void CheckLiftAndDrop() 
    {
        if (!latestCollision) return;
        
        int latestColViewID = latestCollision.GetComponent<PhotonView>().ViewID;
        int latestObjViewID = latestObject ? latestObject.GetComponent<PhotonView>().ViewID : -1;
        Liftable controller = GetController(latestCollision);
        bool isPackaged = false;
        LiftableProduct productController = controller as LiftableProduct;
        if (productController != null) isPackaged = productController.isPackaged;
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && (latestCollision.CompareTag("ProductManager") || latestCollision.CompareTag("PackageManager")))
        {
            ProductManager productManager = latestCollision.GetComponent<ProductManager>();
            if (productManager.CreateController()) Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && IsLifting(-1) && !isPackaged && !controller.isLifted)
        {
            Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && (latestTile.CompareTag("PlaceableTile") || latestTile.CompareTag("DropZone")))
        {
            Drop();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile &&  latestTile.CompareTag("TapeTile"))
        {
            DropOnTapeTable();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && latestTile.CompareTag("TableTile"))
        {
            DropOnTable();
        }
    }

    void Lift()
    {
        latestObject = latestCollision;
        latestObject.transform.parent = gameObject.transform;
        latestObject.transform.localPosition = hand.transform.localPosition;
        float eulerY = ClosestAngle(latestObject.transform.localRotation.eulerAngles.y);
        latestObject.transform.localRotation = Quaternion.Euler(0, eulerY, 0);

        Liftable controller = GetController(latestObject);
        controller.isLifted = true;
        LiftablePackage packageController = controller as LiftablePackage;
        if (packageController != null) packageController.canTape = false;
        liftingID = latestObject.GetComponent<PhotonView>().ViewID;
        PV.RPC("OnLift", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnLift(int viewID, float eulerY)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        obj.transform.parent = gameObject.transform;
        obj.transform.localPosition = hand.transform.localPosition;
        obj.transform.localRotation = Quaternion.Euler(0, eulerY, 0);
        Liftable controller = GetController(obj);
        controller.isLifted = true;
        LiftablePackage packageController = controller as LiftablePackage;
        if (packageController != null) packageController.canTape = false;
    }

    public void Drop()
    {
        if (latestObject == null) return;
        
        Liftable controller = GetController(latestObject);
        controller.isLifted = false;
        canLiftID = -1;
        liftingID = -1;
        if (latestTile.CompareTag("DropZone") && latestObject.CompareTag("PackageController"))
        {
            latestObject.GetComponent<PackageController>().OrderDelivery(latestTile);
            latestCollision = null;
            latestObject = null;
            return;
        }
                
        // Add parent and fix position & rotation
        latestObject.transform.parent = latestTile.transform;
        latestObject.transform.localPosition = ProductController.tileOffset;
        float eulerY = ClosestAngle(gameObject.transform.rotation.eulerAngles.y);
        latestObject.transform.rotation = Quaternion.Euler(0, eulerY, 0);
        PV.RPC("OnDrop", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, latestTile.name, eulerY);
    }

    [PunRPC]
    void OnDrop(int viewID, string tileName, float eulerY)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        GameObject tile = GameObject.Find(tileName);
        obj.transform.parent = tile.transform;
        obj.transform.localPosition = ProductController.tileOffset;
        obj.transform.rotation = Quaternion.Euler(0, eulerY, 0);

        Liftable controller = GetController(obj);
        controller.isLifted = false;
    }

    public void DropOnTable()
    {
        Liftable controller = GetController(latestObject);
        controller.isLifted = false;
        canLiftID = -1;
        liftingID = -1;
        latestObject.transform.parent = latestTile.transform;
        if (latestObject.CompareTag("PackageController"))
        {
            latestObject.transform.localPosition = PackageController.cabinetOffset;
        }
        if(latestObject.CompareTag("ProductController"))
        {
            latestObject.transform.localPosition = ProductController.cabinetOffset;
        }
        float eulerYt = ClosestAngle(gameObject.transform.rotation.eulerAngles.y);
        latestObject.transform.rotation = Quaternion.Euler(0, eulerYt, 0);
        PV.RPC("OnDropOnTable", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, latestTile.name, eulerYt);
    }

    [PunRPC]
    void OnDropOnTable(int viewID, string tileName, float eulerY)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        GameObject tile = GameObject.Find(tileName);
        obj.transform.parent = tile.transform;
        obj.transform.localPosition = ProductController.cabinetOffset;
        obj.transform.rotation = Quaternion.Euler(0, eulerY, 0);

        Liftable controller = GetController(obj);
        controller.isLifted = false;
    }

    public void DropOnTapeTable()
    {
        
        if (latestObject.CompareTag("PackageController"))
        {
            Liftable controller = GetController(latestObject);
            controller.isLifted = false;
            LiftablePackage packageController = controller as LiftablePackage;
            if (packageController != null) packageController.canTape = true;
            canLiftID = -1;
            liftingID = -1;

            latestObject.transform.parent = latestTile.transform;
            latestObject.transform.localPosition = PackageController.cabinetOffset;
            float eulerYt = ClosestAngle(gameObject.transform.rotation.eulerAngles.y);
            latestObject.transform.rotation = Quaternion.Euler(0, eulerYt, 0);
            PV.RPC("OnDropOnTapeTable", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, latestTile.name, eulerYt);
        }

    }

    [PunRPC]
    void OnDropOnTapeTable(int viewID, string tileName, float eulerY)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        GameObject tile = GameObject.Find(tileName);
        obj.transform.parent = tile.transform;
        obj.transform.localPosition = PackageController.cabinetOffset;
        obj.transform.rotation = Quaternion.Euler(0, eulerY, 0);

        Liftable controller = GetController(obj);
        controller.isLifted = false;
        LiftablePackage packageController = controller as LiftablePackage;
        if (packageController != null) packageController.canTape = true;

    }

    private float ClosestAngle(float a)
    {
        float[] w = {0, 90, 180, 270};
        float currentNearest = w[0];
        float currentDifference = Mathf.Abs(currentNearest - a);

        for (int i = 1; i < w.Length; i++)
        {
            float diff = Mathf.Abs(w[i] - a);
            if (diff < currentDifference)
            {
                currentDifference = diff;
                currentNearest = w[i];
            }
        }

        return currentNearest;
    }

    Liftable GetController(GameObject obj)
    {
        Liftable result;
        if (obj.CompareTag("ProductController")) result = obj.GetComponent<ProductController>();
        else result = obj.GetComponent<PackageController>();
        return result;
    }

    public bool IsLifting(int _liftingID)
    {
        return liftingID == _liftingID;
    }

    bool CanLift(int _canLiftID)
    {
        return canLiftID == _canLiftID;
    }
}