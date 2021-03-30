using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLiftController : MonoBehaviour
{
    public int liftingID {get; set;}
    public int canLiftID {get; set;}
    public GameObject latestTile {get; set;}

    public GameObject latestCollision {get; set;}
    GameObject latestObject;
    Transform hand;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        liftingID = -1;
        canLiftID = -1;
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
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && (latestCollision.CompareTag("ProductManager") || latestCollision.CompareTag("PackageManager")))
        {
            ProductManager productManager = latestCollision.GetComponent<ProductManager>();
            productManager.CreateController();
            Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && IsLifting(-1) && !controller.isPackaged && !controller.isLifted)
        {
            Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && (latestTile.CompareTag("PlaceableTile") || latestTile.CompareTag("DropZone")))
        {
            Drop();
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
    }

    public void Drop()
    {
        Liftable controller = GetController(latestObject);
        controller.isLifted = false;
        canLiftID = -1;
        liftingID = -1;

        if (latestTile.CompareTag("DropZone") && latestObject.CompareTag("PackageController"))
        {
            latestObject.GetComponent<PackageController>().OrderDelivery(latestTile);
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

    bool IsLifting(int _liftingID)
    {
        return liftingID == _liftingID;
    }

    bool CanLift(int _canLiftID)
    {
        return canLiftID == _canLiftID;
    }
}