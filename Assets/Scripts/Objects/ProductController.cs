using Photon.Pun;
using UnityEngine;

public class ProductController : MonoBehaviour
{
    //List<GameObject> droppedDeliveries = new List<GameObject>();
    //int gatheredPoints = 0;
    bool spaceKeyWasPressed;

    Rigidbody rb;
    PhotonView PV;
    Transform player;
    PlayerController playerController;
    Transform hand;
    bool isLifted = false;
    bool isPackaged = false;
    string type;

    bool canPickUp = false;
    Vector3 tileOffset = new Vector3(1.5f, 0.25f, 1.5f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerController = PlayerManager.myPlayerController;
        player = PlayerManager.myPlayerController.transform;
        hand = player.GetChild(0);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckLiftAndDrop();
    }

    private void CheckLiftAndDrop () 
    {
        GameObject latestTile = playerController.GetLatestTile();
        if (Input.GetKeyDown(KeyCode.Space) && isLifted && playerController.IsLifting(PV.ViewID) && latestTile && (latestTile.CompareTag("PlaceableTile") || latestTile.CompareTag("DropZone")) && !isPackaged)
        {
            Drop(latestTile);
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isLifted && canPickUp && playerController.IsLifting(-1) && !isPackaged)
        {
            Lift();
        }
    }

    public void Lift()
    {
        gameObject.transform.parent = player;
        gameObject.transform.localPosition = hand.transform.localPosition;
        float eulerY = ClosestAngle(gameObject.transform.localRotation.eulerAngles.y);
        gameObject.transform.localRotation = Quaternion.Euler(0, eulerY, 0);
        isLifted = true;
        playerController.SetLiftingID(PV.ViewID);
        PV.RPC("OnLift", RpcTarget.OthersBuffered, player.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnLift(int viewID, float eulerY)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        gameObject.transform.parent = player.transform;
        gameObject.transform.localPosition = hand.transform.localPosition;
        gameObject.transform.localRotation = Quaternion.Euler(0, eulerY, 0);
        isLifted = true;
    }

    public void Drop(GameObject latestTile)
    {
        isLifted = false;
        canPickUp = false;
        playerController.SetLiftingID(-1);

        if (latestTile.CompareTag("DropZone") && gameObject.CompareTag("PackageController"))
        {
            OrderDelivery(latestTile);
            return;
        }

        // Add parent and fix position & rotation
        gameObject.transform.parent = latestTile.transform;
        gameObject.transform.localPosition = tileOffset;
        float eulerY = ClosestAngle(gameObject.transform.rotation.eulerAngles.y);
        gameObject.transform.rotation = Quaternion.Euler(0, eulerY, 0);
        PV.RPC("OnDrop", RpcTarget.OthersBuffered, latestTile.name, eulerY);
    }

    public void OrderDelivery(GameObject latestTile)
    {
        string num = latestTile.name.Substring(latestTile.name.Length - 1);
        PackageController package = GetComponent<PackageController>();
        TaskController task = GameObject.FindGameObjectWithTag("Task"+num).GetComponent<TaskController>();
        Delivery(package, task);
        PV.RPC("OnDeliver", RpcTarget.OthersBuffered);
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

    [PunRPC]
    void OnDrop(string tileName, float eulerY)
    {
        GameObject tile = GameObject.Find(tileName);
        gameObject.transform.parent = tile.transform;
        gameObject.transform.localPosition = tileOffset;
        gameObject.transform.rotation = Quaternion.Euler(0, eulerY, 0);
        isLifted = false;
        canPickUp = false;
    }
    
    public void SetCanPickUp(bool _canPickUp)
    {
        canPickUp = _canPickUp;
    }

    public bool GetCanPickUp()
    {
        return canPickUp;
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

    public void SetIsLifted(bool _isLifted)
    {
        isLifted = _isLifted;
    }

    public bool GetIsLifted()
    {
        return isLifted;
    }

    public void SetIsPackaged(bool _isPackaged)
    {
        isPackaged = _isPackaged;
    }

    public bool GetIsPackaged()
    {
        return isPackaged;
    }

    public string GetProductType()
    {
        return type;
    }

    public void SetType(string _type)
    {
        type = _type;
    }
}
