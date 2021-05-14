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

    // A static offset where the player's "hand" is, used as object's localPosition when lifting products
    public Transform hand;

    // Offset where the player's "hand" is, used as object's localPosition when lifting packages
    [SerializeField] Transform packageHand;

    // This player's PhotonView.
    PhotonView PV;

    // This player's character traits.
    Character character;

    // This player's climb controller
    PlayerClimbController playerCC;

    // This player's pack controller
    PlayerPackController playerPC;

    // This player's multi lift controller
    PlayerMultiLiftController playerMLC;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        character = GetComponent<Character>();
        playerCC = GetComponent<PlayerClimbController>();
        playerPC = GetComponent<PlayerPackController>();
        playerMLC = GetComponent<PlayerMultiLiftController>();
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
        PackageMultiLiftController packageMLC = latestCollision.GetComponent<PackageMultiLiftController>();
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && (latestCollision.CompareTag("ProductManager") || latestCollision.CompareTag("PackageManager")))
        {
            if (IsLifting())
            {
                PopupInfo.Instance.Popup("Du lyfter redan någonting!", 5);
                return;
            }
            ICreateController manager = GetManager(latestCollision);
            manager.CreateController(PV.ViewID, hand.position);
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && !IsLifting() && CanHelp(packageMLC) && !playerCC.isCrouching)
        {
            HelpLift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && playerMLC.isMultiLifting && playerMLC.iAmLifting)
        {
            DropHelp();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && CanLift(latestColViewID) && !IsLifting() && !IsPackaged(controller) && !controller.isLifted && !playerCC.isCrouching && !playerPC.isTaping)
        {
            Lift();
            return;
        }
        if (Input.GetKeyDown(PlayerController.useButton) && IsLifting(latestObjViewID) && latestTile && DropableTile(latestTile))
        {
            Drop();
            return;
        }
    }

    void Lift()
    {
        // Long player unable to lift stuff on the floor
        if (latestTile.CompareTag("PlaceableTile") && character.characterType.Equals("Long"))
        {
            PopupInfo.Instance.Popup("Den långa karaktären kan inte lyfta lådor från golvet", 7);
            return;
        }

        float eulerY = ClosestAngle(latestCollision.transform.rotation.eulerAngles.y - gameObject.transform.rotation.eulerAngles.y);
        PV.RPC("OnLift", RpcTarget.AllViaServer, latestCollision.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnLift(int viewID, float eulerY)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        latestObject = obj;
        Liftable controller = GetController(obj);
        if (controller.isLifted || playerCC.isCrouching) return;

        // Make child and change position & rotation
        obj.transform.parent = gameObject.transform;
        obj.transform.localPosition = obj.CompareTag("PackageController") ? packageHand.transform.localPosition : hand.transform.localPosition;
        obj.transform.localRotation = Quaternion.Euler(0, eulerY, 0);

        // If package -> add lifter
        PackageMultiLiftController packageMLC = obj.GetComponent<PackageMultiLiftController>();
        if (packageMLC != null)
        {
            Vector3 anchor = packageMLC.CalculateLocalAnchor(this);
            playerMLC.myAnchor = anchor;
            packageMLC.takenAnchors.Add(anchor);
            packageMLC.AddLifter(this);
        }

        // Set booleans and liftingID
        liftingID = obj.GetComponent<PhotonView>().ViewID;
        controller.isLifted = true;
        LiftablePackage pkgController = controller as LiftablePackage;
        if (pkgController != null)
        {
            pkgController.canTape = false;
        }

        // Disable highlight while lifting
        if (PV.IsMine)
        {
            IHighlight highlighter = PickUpCheck.GetHighlighter(obj);
            highlighter.Highlight(false);
        }
    }

    void HelpLift()
    {
        // Calculate anchor (which side of the package to help lift)
        PackageMultiLiftController packageMLC = latestCollision.GetComponent<PackageMultiLiftController>();
        Vector3 anchor = packageMLC.CalculateLocalAnchor(this);

        // Check if the side is taken
        if (packageMLC == null || packageMLC.takenAnchors.Contains(anchor)) return;

        // Add the side to the list of taken anchors and start helping.
        PV.RPC("OnHelpLift", RpcTarget.AllViaServer, latestCollision.GetComponent<PhotonView>().ViewID, anchor);
    }

    [PunRPC]
    void OnHelpLift(int viewID, Vector3 anchor)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        latestObject = obj;
        PackageMultiLiftController packageMLC = latestObject.GetComponent<PackageMultiLiftController>();

        // Set liftingID
        liftingID = obj.GetComponent<PhotonView>().ViewID;

        // Add helper
        playerMLC.myAnchor = anchor;
        packageMLC.takenAnchors.Add(anchor);
        packageMLC.AddHelper(this, anchor);

        // Disable highlight while lifting
        if (PV.IsMine)
        {
            IHighlight highlighter = PickUpCheck.GetHighlighter(obj);
            highlighter.Highlight(false);
        }
    }

    void Drop()
    {
        if (latestObject == null || latestTile == null) return;

        // Long player unable to drop stuff on the floor
        if (latestTile.CompareTag("PlaceableTile") && character.characterType.Equals("Long"))
        {
            PopupInfo.Instance.Popup("Den långa karaktären kan inte placera lådor på golvet", 7);
            return;
        }

        // Dropped on DropZone
        if (latestTile.CompareTag("DropZone"))
        {
            // Try to deliver if it's a package
            if (latestObject.CompareTag("PackageController"))
            {
                bool delivered = latestObject.GetComponent<PackageController>().OrderDelivery(latestTile, playerMLC);
                if (delivered)
                {
                    DropBooleans(latestObject);
                }
                return;
            }
            // Not able to drop products on dropzones
            else
            {
                PopupInfo.Instance.Popup("Man kan inte placera produkter i leveranszoner", 7);
                return;
            }
        }

        // Dropped a product on a tape table
        if (latestTile.CompareTag("TapeTile") && latestObject.CompareTag("ProductController"))
        {
            PopupInfo.Instance.Popup("Man kan inte placera produkter på tejpborden", 5);
            return;
        }

        // Dropped on trashcan -> trash it
        if (latestTile.CompareTag("TrashTile"))
        {
            playerMLC.tooHeavy = false;
            PV.RPC("OnTrash", RpcTarget.All, latestObject.GetComponent<PhotonView>().ViewID);
            return;
        }

        float eulerY = ClosestAngle(latestObject.transform.rotation.eulerAngles.y);
        Vector3 offset = GetTileOffset(latestObject, latestTile);
        PV.RPC("OnDrop", RpcTarget.AllViaServer, latestObject.GetComponent<PhotonView>().ViewID, eulerY, latestTile.name, offset);
    }

    [PunRPC]
    void OnDrop(int viewID, float eulerY, string tileName, Vector3 offset)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        if (obj == null) return;
        GameObject tile = GameObject.Find(tileName);
        if (tile == null || TileIsOccupied(tile)) return;

        // Make child and fix position & rotation
        obj.transform.parent = tile.transform;
        obj.transform.localPosition = offset;
        obj.transform.rotation = Quaternion.Euler(0, eulerY, 0);

        // If package -> remove lifter
        PackageMultiLiftController packageMLC = obj.GetComponent<PackageMultiLiftController>();
        if (packageMLC != null)
        {
            packageMLC.takenAnchors.Remove(playerMLC.myAnchor);
            playerMLC.myAnchor = Vector3.zero;
            packageMLC.RemoveLifter(this);
        }

        // Set booleans
        DropBooleans(obj);
        Liftable controller = GetController(obj);
        LiftablePackage pkgController = controller as LiftablePackage;
        if (pkgController != null && tile.CompareTag("TapeTile"))
        {
            pkgController.canTape = true;
        }
    }

    void DropBooleans(GameObject obj)
    {
        canLiftID = -1;
        liftingID = -1;
        Liftable controller = GetController(obj);
        controller.isLifted = false;
        latestObject = null;
        latestCollision = null;
    }

    void DropHelp()
    {
        PV.RPC("OnDropHelp", RpcTarget.AllViaServer, latestObject.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnDropHelp(int objViewID)
    {
        GameObject obj = PhotonView.Find(objViewID).gameObject;

        // Set liftingID
        liftingID = -1;

        // Remove helper from package
        PackageMultiLiftController packageMLC = obj.GetComponent<PackageMultiLiftController>();
        if (packageMLC == null) return;
        packageMLC.takenAnchors.Remove(playerMLC.myAnchor);
        playerMLC.myAnchor = Vector3.zero;
        if (packageMLC.lifters.Count > 1) packageMLC.RemoveHelper(this);
        else Drop();
    }

    [PunRPC]
    void OnTrash(int objViewID)
    {
        PhotonView objPV = PhotonView.Find(objViewID);
        if (objPV.CompareTag("PackageController")) objPV.GetComponent<PackageController>().DestroyPackage();
        else if (objPV.CompareTag("ProductController")) objPV.GetComponent<ProductController>().DestroyProduct();
        DropBooleans(latestObject);
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

    Vector3 GetTileOffset(GameObject obj, GameObject tile)
    {
        if (obj.CompareTag("ProductController")) return ProductController.GetTileOffset(tile);
        else return PackageController.GetTileOffset(tile);
    }

    bool TileIsOccupied(GameObject tile)
    {
        PackageController pkg = tile.GetComponentInChildren<PackageController>();
        ProductController prdct = tile.GetComponentInChildren<ProductController>();
        return pkg != null || prdct != null;
    }

    public static bool DropableTile(GameObject tile)
    {
        return tile.CompareTag("PlaceableTile") || tile.CompareTag("PlaceableOutsideTile") || tile.CompareTag("DropZone") || tile.CompareTag("TableTile") || tile.CompareTag("TapeTile") || tile.CompareTag("TrashTile");
    }

    public bool IsLifting()
    {
        return liftingID != -1;
    }

    bool IsLifting(int _liftingID)
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

    bool CanHelp(PackageMultiLiftController packageMLC)
    {
        bool canHelp = false;
        if (packageMLC != null) canHelp = packageMLC.tooHeavy && packageMLC.lifters.Count > 0;
        return canHelp;
    }
}