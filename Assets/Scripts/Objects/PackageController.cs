using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class PackageController : MonoBehaviour, LiftablePackage
{
    public List<int> lifters {get; set;} = new List<int>(4);
    public bool tooHeavy {get; set;} = false;

    public bool isLifted {get; set;} = false;
    public bool isTaped {get; set;} = false;

    Rigidbody rb;
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
        rb = GetComponent<Rigidbody>();
    }

    public void AddLifter(PlayerLiftController playerLiftController)
    {
        int viewID = playerLiftController.GetComponent<PhotonView>().ViewID;
        if (!lifters.Contains(viewID)) lifters.Add(viewID);
        SetTooHeavyAndIsMultiLifting(null);
    }

    public void AddHelper(PlayerLiftController newPLC)
    {
        if (lifters.Count == 0) return;

        PhotonView newPV = newPLC.GetComponent<PhotonView>();
        if (newPV.IsMine)
        {
            gameObject.transform.parent = newPLC.gameObject.transform;
            newPLC.GetComponent<PhotonRigidbodyView>().enabled = false;
            newPLC.GetComponent<PhotonTransformView>().enabled = false;
            foreach (int viewID in lifters)
            {
                GameObject p = PhotonView.Find(viewID).gameObject;
                p.transform.parent = newPLC.gameObject.transform;
                p.GetComponent<PhotonRigidbodyView>().enabled = false;
                p.GetComponent<PhotonTransformView>().enabled = false;
                newPV.GetComponent<PlayerController>().player2 = p;
            }
        }
        else
        {
            newPLC.gameObject.transform.parent = PlayerManager.myPlayerLiftController.gameObject.transform;
            PlayerManager.myPlayerLiftController.GetComponent<PlayerController>().player2 = newPLC.gameObject;
            foreach (int viewID in lifters)
            {
                GameObject p = PhotonView.Find(viewID).gameObject;
                p.GetComponent<PhotonRigidbodyView>().enabled = false;
                p.GetComponent<PhotonTransformView>().enabled = false;
            }
        }
        newPLC.GetComponent<PhotonRigidbodyView>().enabled = false;
        newPLC.GetComponent<PhotonTransformView>().enabled = false;

        int newViewID = newPV.ViewID;
        if (!lifters.Contains(newViewID)) lifters.Add(newViewID);

        SetTooHeavyAndIsMultiLifting(null);
    }

    public void RemoveLifter(PlayerLiftController playerLiftController)
    {
        int viewID = playerLiftController.GetComponent<PhotonView>().ViewID;
        if (!lifters.Remove(viewID)) print("wrong!");
        SetTooHeavyAndIsMultiLifting(playerLiftController);
    }

    public void RemoveHelper(PlayerLiftController playerLiftController)
    {
        PhotonView playerPV = playerLiftController.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Remove(viewID)) print("wrong!");
        foreach (ConfigurableJoint confJoint in GetComponents<ConfigurableJoint>())
        {
            if (confJoint.connectedBody.GetComponent<PhotonView>().ViewID == viewID) 
            {
                print("hi2");
                Destroy(confJoint);
            }
        }
        if (playerPV.IsMine && playerPV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) playerPV.TransferOwnership(playerPV.CreatorActorNr);
        if (lifters.Count < 2)
        {
            rb.isKinematic = true;
            Destroy(GetComponent<ConfigurableJoint>());
            GameObject lastPlayer = PhotonView.Find(lifters[0]).GetComponent<PlayerLiftController>().gameObject;
            gameObject.transform.parent = lastPlayer.transform.parent;
            PhotonView lastPlayerPV = lastPlayer.GetComponent<PhotonView>();
            if (lastPlayerPV.IsMine && lastPlayerPV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) lastPlayerPV.TransferOwnership(lastPlayerPV.CreatorActorNr);
        }
        SetTooHeavyAndIsMultiLifting(playerLiftController);
    }

    void SetTooHeavyAndIsMultiLifting(PlayerLiftController removedPlayer)
    {
        if (removedPlayer != null)
        {
            PlayerController removedPC = removedPlayer.GetComponent<PlayerController>();
            removedPC.tooHeavy = false;
            removedPC.isHelper = false;
            removedPC.isMultiLifting = false;
        }
        int totalStrength = 0;
        foreach (int viewID in lifters)
        {
            PlayerLiftController playerLiftController = PhotonView.Find(viewID).GetComponent<PlayerLiftController>();
            totalStrength += playerLiftController.GetComponent<Character>().strength;
        }
        tooHeavy = totalStrength < productCount;
        foreach (int viewID in lifters)
        {
            PlayerController playerController = PhotonView.Find(viewID).GetComponent<PlayerController>();
            playerController.tooHeavy = tooHeavy;
            playerController.isHelper = !PV.IsMine;
            playerController.isMultiLifting = lifters.Count > 1;
            print("News:");
            print("Too Heavy = " + playerController.tooHeavy);
            print("Is Helper = " + playerController.isHelper);
            print("Is Multi Lifting = " + playerController.isMultiLifting);
        }
    }

    float MinMoveSpeed(List<int> list)
    {
        float min = PhotonView.Find(list[0]).GetComponent<Character>().movementSpeed;
        for (int i = 1; i < list.Count; i++)
        {
            float current = PhotonView.Find(list[i]).GetComponent<Character>().movementSpeed;
            if (current < min) min = current;
        }
        return min;
    }

    // Delivery

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
