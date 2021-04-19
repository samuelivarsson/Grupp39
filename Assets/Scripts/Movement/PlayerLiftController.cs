using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLiftController : MonoBehaviour
{
    // The PhotonView.viewID of the object the player is carrying, set to -1 if the player isn't carrying anything
    public int liftingID {get; set;} = -1;
    // The PhotonView.viewID of the object the player's PickUpCheck hitbox is triggering with, set to -1 when the player's PickUpCheck hitbox "exits" the object
    public int canLiftID {get; set;} = -1;
    
    // Latest tile the player's ObjectTrigger triggered with
    public GameObject latestTile {get; set;}

    // Latest object the player's PickUpCheck hitbox triggered with
    public GameObject latestCollision {get; set;}
    // Latest object the player lifted
    public GameObject latestObject {get; set;}

    // A static offset where the player's "hand" is, used as object's localPosition when lifting
    public Transform hand {get; set;}

    // This player's PhotonView.
    PhotonView PV;

    // This player's character traits.
    Character character;

    // This player's climb controller
    PlayerClimbController playerCC;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        hand = gameObject.transform.GetChild(0);
        character = GetComponent<Character>();
        playerCC = GetComponent<PlayerClimbController>();
    }

    void Update()
    {
        if (PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) return;

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
            ICreateController manager = GetManager(latestCollision);
            if (manager.CreateController()) Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && IsLifting(-1) && CanHelp(controller) && character.characterType != "Long" && !playerCC.isCrouching)
        {
            HelpLift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && IsHelper(controller))
        {
            DropHelp();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && IsLifting(-1) && !IsPackaged(controller) && !controller.isLifted && !playerCC.isCrouching)
        {
            Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && !TileIsOccupied() && (latestTile.CompareTag("PlaceableTile") || latestTile.CompareTag("DropZone")))
        {
            Drop();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && latestTile.CompareTag("TapeTile") && !TileIsOccupied())
        {
            DropOnTapeTable();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && latestTile.CompareTag("TableTile") && !TileIsOccupied())
        {
            DropOnTable();
        }
    }

    void _Lift(GameObject obj, float eulerY)
    {
        // Make child and change position & rotation
        obj.transform.parent = gameObject.transform;
        obj.transform.localPosition = hand.transform.localPosition;
        obj.transform.localRotation = Quaternion.Euler(0, eulerY, 0);

        // If package -> add lifter
        PackageController packageController = obj.GetComponent<PackageController>();
        if (packageController != null) packageController.AddLifter(this);

        // Set booleans and liftingID
        liftingID = obj.GetComponent<PhotonView>().ViewID;
        Liftable controller = GetController(obj);
        controller.isLifted = true;
        LiftablePackage pkgController = controller as LiftablePackage;
        if (pkgController != null)
        {
            pkgController.canTape = false;
        }
    }

    void Lift()
    {
        latestObject = latestCollision;
        float eulerY = ClosestAngle(latestObject.transform.rotation.eulerAngles.y - gameObject.transform.rotation.eulerAngles.y);
        _Lift(latestObject, eulerY);
        PV.RPC("OnLift", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnLift(int viewID, float eulerY)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        _Lift(obj, eulerY);
    }

    void _HelpLift(GameObject obj)
    {
        PackageController packageController = obj.GetComponent<PackageController>();
        packageController.AddHelper(this);

        // Set booleans and liftingID
        liftingID = obj.GetComponent<PhotonView>().ViewID;
    }

    void HelpLift()
    {
        latestObject = latestCollision;
        _HelpLift(latestObject);
        PV.RPC("OnHelpLift", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnHelpLift(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        latestObject = obj;
        _HelpLift(obj);
    }

    void _Drop(GameObject obj, float eulerY, GameObject tile, Vector3 offset)
    {
        // Make child and fix position & rotation
        obj.transform.parent = tile.transform;
        obj.transform.localPosition = offset;
        obj.transform.rotation = Quaternion.Euler(0, eulerY, 0);

        // If package -> add lifter
        PackageController packageController = obj.GetComponent<PackageController>();
        if (packageController != null) packageController.RemoveLifter(this);

        // Set booleans
        canLiftID = -1;
        liftingID = -1;
        Liftable controller = GetController(obj);
        controller.isLifted = false;
        LiftablePackage pkgController = controller as LiftablePackage;
        if (pkgController != null && tile.CompareTag("TapeTile"))
        {
            pkgController.canTape = true;
        }
    }

    void Drop()
    {
        if (latestObject == null) return;

        // not able to drop products on dropzones
        if (latestTile.CompareTag("DropZone") && latestObject.CompareTag("ProductController")) return; 
        
        // Long player unable to drop stuff on the floor
        if (latestTile.CompareTag("PlaceableTile") && character.characterType.Equals("Long")) return;

        float eulerY = ClosestAngle(latestObject.transform.rotation.eulerAngles.y);
        Vector3 offset = GetTileOffset(latestObject);
        if (latestTile.CompareTag("DropZone") && latestObject.CompareTag("PackageController"))
        {
            bool delivered = latestObject.GetComponent<PackageController>().OrderDelivery(latestTile);
            if (delivered)
            {
                _Drop(latestObject, eulerY, latestTile, offset);
                latestCollision = null;
                latestObject = null;
            }
            return;
        }
        _Drop(latestObject, eulerY, latestTile, offset);
        PV.RPC("OnDrop", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, eulerY, latestTile.name, offset);
    }

    [PunRPC]
    void OnDrop(int viewID, float eulerY, string tileName, Vector3 offset)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        if (obj == null) return;
        GameObject tile = GameObject.Find(tileName);
        _Drop(obj, eulerY, tile, offset);
    }

    void DropOnTable()
    {
        if (latestObject == null) return;

        float eulerY = ClosestAngle(latestObject.transform.rotation.eulerAngles.y);
        Vector3 offset = GetTableOffset(latestObject);
        _Drop(latestObject, eulerY, latestTile, offset);
        PV.RPC("OnDropOnTable", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, eulerY, latestTile.name, offset);
    }

    [PunRPC]
    void OnDropOnTable(int viewID, float eulerY, string tileName, Vector3 offset)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        GameObject tile = GameObject.Find(tileName);
        _Drop(obj, eulerY, tile, offset);
    }

    void DropOnTapeTable()
    {
        if (latestObject == null) return;

        if (latestObject.CompareTag("PackageController"))
        {
            float eulerY = ClosestAngle(latestObject.transform.rotation.eulerAngles.y);
            Vector3 offset = PackageController.tapeOffset;
            _Drop(latestObject, eulerY, latestTile, offset);
            PV.RPC("OnDropOnTapeTable", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID, eulerY, latestTile.name, offset);
        }
    }

    [PunRPC]
    void OnDropOnTapeTable(int viewID, float eulerY, string tileName, Vector3 offset)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        GameObject tile = GameObject.Find(tileName);
        _Drop(obj, eulerY, tile, offset);
    }

    void _DropHelp(GameObject obj)
    {
        PackageController packageController = obj.GetComponent<PackageController>();
        packageController.RemoveHelper(this);

        // Set booleans and liftingID
        liftingID = -1;
    }

    void DropHelp()
    {
        latestObject = latestCollision;
        _DropHelp(latestObject);
        PV.RPC("OnDropHelp", RpcTarget.OthersBuffered, latestObject.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnDropHelp(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        latestObject = obj;
        _DropHelp(obj);
    }

    public static float ClosestAngle(float a)
    {
        float[] w = {-360, -270, -180, -90, 0, 90, 180, 270, 360};
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

    ICreateController GetManager(GameObject obj)
    {
        ICreateController result;
        if (obj.CompareTag("ProductManager")) result = obj.GetComponent<ProductManager>();
        else result = obj.GetComponent<PackageManager>();
        return result;
    }

    Vector3 GetTileOffset(GameObject obj)
    {
        Vector3 result;
        if (obj.CompareTag("ProductController")) result = ProductController.tileOffset;
        else result = PackageController.tileOffset;
        return result;
    }

    Vector3 GetTableOffset(GameObject obj)
    {
        Vector3 result;
        if (obj.CompareTag("ProductController")) result = ProductController.tableOffset;
        else result = PackageController.tableOffset;
        return result;
    }

    bool TileIsOccupied()
    {
        PackageController pkg = latestTile.GetComponentInChildren<PackageController>();
        ProductController prdct = latestTile.GetComponentInChildren<ProductController>();
        return pkg != null || prdct != null;
    }

    public bool IsLifting(int _liftingID)
    {
        return liftingID == _liftingID;
    }

    bool CanLift(int _canLiftID)
    {
        return canLiftID == _canLiftID;
    }

    bool IsPackaged(Liftable controller)
    {
        bool isPackaged = false;
        LiftableProduct productController = controller as LiftableProduct;
        if (productController != null) isPackaged = productController.isPackaged;
        return isPackaged;
    }

    bool CanHelp(Liftable controller)
    {
        bool canHelp = false;
        LiftablePackage packageController = controller as LiftablePackage;
        if (packageController != null) canHelp = packageController.tooHeavy && packageController.lifters.Count > 0;
        return canHelp;
    }

    bool IsHelper(Liftable controller)
    {
        bool isHelper = false;
        LiftablePackage packageController = controller as LiftablePackage;
        if (packageController != null) isHelper = packageController.lifters.Count > 1;
        return isHelper;
    }
}