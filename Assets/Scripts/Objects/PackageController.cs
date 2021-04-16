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
    }

    public void AddLifter(PlayerLiftController playerLiftController)
    {
        PhotonView playerPV = playerLiftController.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Contains(viewID)) lifters.Add(viewID);
        SetMultiLiftBools(null);
    }

    public void AddHelper(PlayerLiftController newPlayerLC)
    {
        if (lifters.Count == 0) return;

        PhotonView newPlayerPV = newPlayerLC.GetComponent<PhotonView>();
        int viewID = newPlayerPV.ViewID;
        if (!lifters.Contains(viewID)) lifters.Add(viewID);

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        if (lifters.Count == 2)
        {
            GameObject parentPlayer = GetComponentInParent<PlayerLiftController>().gameObject;
            PhotonView parentPV = parentPlayer.GetComponent<PhotonView>();
            Rigidbody parentRB = parentPlayer.GetComponent<Rigidbody>();
            if (parentPV.IsMine && !PV.IsMine) parentPV.TransferOwnership(PV.Owner);
            gameObject.transform.parent = null;
            ConfigurableJoint confJoint = gameObject.AddComponent<ConfigurableJoint>();
            SetConfJoint(confJoint, parentRB, parentPlayer.GetComponent<PlayerLiftController>());
            if (lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID))
            {
                parentRB.isKinematic = false;
            }
        }

        if (lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID) && !PV.IsMine)
        {
            foreach (int vid in lifters)
            {
                PhotonView playerPV = PhotonView.Find(vid);
                playerPV.GetComponent<PhotonTransformViewClassic>().enabled = false;
                playerPV.GetComponent<PhotonRigidbodyView>().enabled = false;
            }
        }

        Rigidbody newPlayerRB = newPlayerLC.GetComponent<Rigidbody>();
        if (newPlayerPV.IsMine && !PV.IsMine) newPlayerPV.TransferOwnership(PV.Owner);
        if (lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID))
        {
            newPlayerRB.isKinematic = false;
        }
        ConfigurableJoint configJoint = gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint(configJoint, newPlayerRB, newPlayerLC);

        SetMultiLiftBools(null);
    }

    public void RemoveLifter(PlayerLiftController playerLiftController)
    {
        PhotonView playerPV = playerLiftController.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Remove(viewID)) print("Couldn't remove lifter");
        SetMultiLiftBools(playerLiftController);
    }

    public void RemoveHelper(PlayerLiftController playerLiftController)
    {
        PhotonView playerPV = playerLiftController.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Remove(viewID)) print("Couldn't remove lifter");
        foreach (ConfigurableJoint confJoint in GetComponents<ConfigurableJoint>())
        {
            if (confJoint.connectedBody.GetComponent<PhotonView>().ViewID == viewID) 
            {
                Destroy(confJoint);
            }
        }
        if (playerPV.IsMine && playerPV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) playerPV.TransferOwnership(playerPV.CreatorActorNr);
        if (!lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID) && !PV.IsMine)
        {
            foreach (int vid in lifters)
            {
                PhotonView _pv = PhotonView.Find(vid);
                _pv.GetComponent<PhotonTransformViewClassic>().enabled = true;
                _pv.GetComponent<PhotonRigidbodyView>().enabled = true;
            }
        }
        if (lifters.Count < 2)
        {
            Destroy(GetComponent<ConfigurableJoint>());
            Destroy(rb);
            rb = null;
            GameObject lastPlayer = PhotonView.Find(lifters[0]).GetComponent<PlayerLiftController>().gameObject;
            gameObject.transform.parent = lastPlayer.transform;
            gameObject.transform.position = lastPlayer.GetComponent<PlayerLiftController>().hand.position;
            PhotonView lastPlayerPV = lastPlayer.GetComponent<PhotonView>();
            if (lastPlayerPV.IsMine && lastPlayerPV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) lastPlayerPV.TransferOwnership(lastPlayerPV.CreatorActorNr);
        }
        SetMultiLiftBools(playerLiftController);
    }

    void SetMultiLiftBools(PlayerLiftController removedPlayer)
    {
        if (removedPlayer != null)
        {
            PlayerMultiLiftController removedPlayerMLC = removedPlayer.GetComponent<PlayerMultiLiftController>();
            removedPlayerMLC.tooHeavy = false;
            removedPlayerMLC.iAmLifting = false;
            removedPlayerMLC.isMultiLifting = false;
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
            PlayerMultiLiftController playerMLC = PhotonView.Find(viewID).GetComponent<PlayerMultiLiftController>();
            playerMLC.tooHeavy = tooHeavy;
            playerMLC.iAmLifting = lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID);
            playerMLC.isMultiLifting = lifters.Count > 1;
            print("News:");
            print("Too Heavy = " + playerMLC.tooHeavy);
            print("I Am Lifting = " + playerMLC.iAmLifting);
            print("Is Multi Lifting = " + playerMLC.isMultiLifting);
        }
    }

    void SetConfJoint(ConfigurableJoint confJoint, Rigidbody conBody, PlayerLiftController player)
    {
        confJoint.connectedBody = conBody;
        confJoint.anchor = CalculateLocalAnchors(player);
        confJoint.autoConfigureConnectedAnchor = false;
        confJoint.connectedAnchor = Vector3.zero;
        confJoint.xMotion = ConfigurableJointMotion.Locked;
        confJoint.yMotion = ConfigurableJointMotion.Locked;
        confJoint.zMotion = ConfigurableJointMotion.Locked;
        confJoint.angularXMotion = ConfigurableJointMotion.Locked;
        confJoint.angularYMotion = ConfigurableJointMotion.Free;
        confJoint.angularZMotion = ConfigurableJointMotion.Limited;
        SoftJointLimit sjl = new SoftJointLimit();
        sjl.limit = 20;
        // confJoint.lowAngularXLimit = sjl;
        // confJoint.highAngularXLimit = sjl;
        confJoint.angularZLimit = sjl;
    }

    Vector3 CalculateLocalAnchors(PlayerLiftController player)
    {
        float offset1 = 0.7f;
        Vector3[] list = {new Vector3(offset1, 0, 0), new Vector3(-offset1, 0, 0), new Vector3(0, 0, offset1), new Vector3(0, 0, -offset1)};

        Vector3 anchor = list[0];
        float min = Vector3.Distance(gameObject.transform.position + list[0], player.transform.position);
        for (int i = 1; i < list.Length; i++)
        {
            Vector3 pos = gameObject.transform.position + list[i];
            float current = Vector3.Distance(pos, player.transform.position);
            if (current < min) 
            {
                min = current;
                anchor = list[i];
            }
        }
        return anchor;
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
