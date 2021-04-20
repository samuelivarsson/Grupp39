using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    public static Vector3 tileOffset = new Vector3(1.5f, 0f, 1.5f);
    public static Vector3 tapeOffset = new Vector3(1.5f, 0.28f, 1.5f);
    public static Vector3 tableOffset = new Vector3(1.5f, 0.3f, 1.5f);

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
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

    // AFTER ADDING TO LIST:
    // I am a lifter:
        // Me who was added:
            // I'm owner: both kinematic = false; add confjoints; package kinematic = false;
            // I'm not owner: both kinematic = false; add confjoints; disable transform view for all lifters; transfer ownership; package kinematic = false;
        // Not me who was added:
            // I'm owner: both kinematic = false; add confjoints; package kinematic = false;
            // I'm not owner: both kinematic = false; add confjoints; disable transform view; package kinematic = false;
    // I am not a lifter:
        // Not me who was added:
            // I'm owner: both kinematic = true; add confjoints;
            // I'm not owner: both kinematic = true; add confjoints; disable transform view;   
    public void AddHelper(PlayerLiftController newPlayerLC)
    {
        if (lifters.Count == 0) return;

        PhotonView newPlayerPV = newPlayerLC.GetComponent<PhotonView>();
        int viewID = newPlayerPV.ViewID;
        if (!lifters.Contains(viewID)) lifters.Add(viewID);

        if (lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID))
        {
            // I am a lifter -> I was either added or was already in the lifters list.
            if (PhotonNetwork.LocalPlayer.ActorNumber == newPlayerPV.CreatorActorNr)
            {
                // My player was added
                if (!PV.IsMine)
                {
                    // My player was added and I am not the owner of the package.
                    newPlayerPV.TransferOwnership(PV.Owner);
                    // Disable transform views for all lifters (locally)
                    foreach (int vid in lifters)
                    {
                        PhotonView playerPV = PhotonView.Find(vid);
                        playerPV.GetComponent<PhotonTransformViewClassic>().enabled = false;
                    }
                }
            }
            foreach (int vid in lifters)
            {
                PhotonView playerPV = PhotonView.Find(vid);
                playerPV.GetComponent<Rigidbody>().isKinematic = false;
            }
            rb.isKinematic = false;
        }
        else
        {
            // I am not a lifter -> the player that was added was not me.
            foreach (int vid in lifters)
            {
                // Set all players to kinematic (except myself)
                PhotonView playerPV = PhotonView.Find(vid);
                if (playerPV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) playerPV.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        // Add confjoints
        if (lifters.Count == 2)
        {
            // First helper was added -> Make original lifter a helper.
            PhotonView parentPV = PhotonView.Find(lifters[0]);
            PlayerLiftController parentPLC = parentPV.GetComponent<PlayerLiftController>();
            Rigidbody parentRB = parentPV.GetComponent<Rigidbody>();
            if (parentPV.IsMine && !PV.IsMine) parentPV.TransferOwnership(PV.Owner);
            gameObject.transform.parent = null;
            ConfigurableJoint confJoint = gameObject.AddComponent<ConfigurableJoint>();
            SetConfJoint(confJoint, parentRB, parentPLC);
        }

        Rigidbody newPlayerRB = newPlayerLC.GetComponent<Rigidbody>();
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

    // AFTER REMOVING FROM LIST:
    // I am a lifter:
        // Not me who was removed:
            // I'm owner: removed player kinematic = true; remove confjoint; transfer ownership back;
            // I'm not owner: removed player kinematic = true; remove confjoint; enable transform view for all non-lifters;
    // I am not a lifter:
        // Me who was removed:
            // I'm owner: all except myself kinematic = true; remove confjoint; package kinematic = true;
            // I'm not owner: all except myself kinematic = true; remove confjoint; enable transform view; package kinematic = true;
        // Not me who was removed:
            // I'm owner: all except myself kinematic = true; remove confjoint; transfer ownership back;
            // I'm not owner: all except myself kinematic = true; remove confjoint;        
    public void RemoveHelper(PlayerLiftController playerLiftController)
    {
        PhotonView playerPV = playerLiftController.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Remove(viewID)) print("Couldn't remove lifter");

        if (lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID))
        {
            // I am a lifter -> I was not removed.
            if (PV.IsMine)
            {
                // I was not removed but I am owner of the package.
                playerPV.TransferOwnership(playerPV.CreatorActorNr);
            }
            else
            {
                // I'm not owner of package -> I have disabled the removed player's transform view -> enable it again.
                playerPV.GetComponent<PhotonTransformViewClassic>().enabled = true;
            }
            // Set the removed player to kinematic = true
            playerPV.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            // I am not a lifter -> I was either removed or only watching.
            if (PhotonNetwork.LocalPlayer.ActorNumber == playerPV.CreatorActorNr)
            {
                // I was removed
                if (!PV.IsMine)
                {
                    // My player was removed and I am not the owner of the package.
                    // Enable transform views for all lifters (locally)
                    foreach (int vid in lifters)
                    {
                        PhotonView _playerPV = PhotonView.Find(vid);
                        _playerPV.GetComponent<PhotonTransformViewClassic>().enabled = true;
                    }
                }
                rb.isKinematic = true;
            }
            else
            {
                if (PV.IsMine)
                {
                    // Not my player was removed but I am the owner of the package.
                    playerPV.TransferOwnership(playerPV.CreatorActorNr);
                }
            }
            foreach (int vid in lifters)
            {
                // Set all players to kinematic (except myself)
                PhotonView _playerPV = PhotonView.Find(vid);
                if (playerPV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) _playerPV.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
        List<ConfigurableJoint> confJoints = new List<ConfigurableJoint>(GetComponents<ConfigurableJoint>());
        for (int i = 0; i < confJoints.Count;)
        {
            if (confJoints[i].connectedBody.GetComponent<PhotonView>().ViewID == viewID) 
            {
                Debug.LogError("DESTROYING");
                Destroy(confJoints[i]);
                confJoints.Remove(confJoints[i]);
            }
            else i++;
        }
        if (lifters.Count < 2)
        {
            rb.isKinematic = true;
            if (confJoints.Count > 1)
            {
                Debug.LogError("MORE THAN ONE CONFJOINT!");
                print("Length: "+confJoints.Count);
                print("First: "+confJoints[0].anchor);
                if (confJoints[1] != null) print("Second: "+confJoints[1].anchor);
            }
            Destroy(confJoints[0]);
            PlayerLiftController lastPlayerLC = PhotonView.Find(lifters[0]).GetComponent<PlayerLiftController>();
            gameObject.transform.parent = lastPlayerLC.transform;
            gameObject.transform.position = lastPlayerLC.hand.position;
            PhotonView lastPlayerPV = lastPlayerLC.GetComponent<PhotonView>();
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
        }
    }

    void SetConfJoint(ConfigurableJoint confJoint, Rigidbody conBody, PlayerLiftController player)
    {
        confJoint.connectedBody = conBody;
        Vector3 anchor = CalculateLocalAnchors(player);
        confJoint.anchor = anchor;
        confJoint.axis = Vector3.zero;
        confJoint.autoConfigureConnectedAnchor = false;
        confJoint.connectedAnchor = new Vector3(0, 0.5f, 0.7f);
        confJoint.xMotion = ConfigurableJointMotion.Locked;
        confJoint.yMotion = ConfigurableJointMotion.Limited;
        confJoint.zMotion = ConfigurableJointMotion.Locked;
        confJoint.angularXMotion = ConfigurableJointMotion.Locked;
        confJoint.angularYMotion = ConfigurableJointMotion.Free;
        confJoint.angularZMotion = ConfigurableJointMotion.Locked;
        float limit = 0.5f;
        SoftJointLimit sjl = new SoftJointLimit();
        sjl.limit = limit;
        confJoint.linearLimit = sjl;
    }

    Vector3 CalculateLocalAnchors(PlayerLiftController player)
    {
        float offset1 = 0.5f;
        Vector3[] list = {new Vector3(offset1, 0, 0), new Vector3(-offset1, 0, 0), new Vector3(0, 0, offset1), new Vector3(0, 0, -offset1)};

        Vector3 anchor = list[0];
        float min = Vector3.Distance(gameObject.transform.TransformPoint(list[0]), player.transform.position);
        for (int i = 1; i < list.Length; i++)
        {
            Vector3 pos = gameObject.transform.TransformPoint(list[i]);
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

    public bool OrderDelivery(GameObject latestTile)
    {
        if (!isTaped)
        {
            print("Package isn't taped!");
            PopupInfo.Instance.Popup("Package isn't taped!", 5);
            return false;
        }
        if (!latestTile.CompareTag("DropZone"))
        {
            print("This is not a drop zone!");
            return false;
        }
        string num = latestTile.name.Substring(latestTile.name.Length - 1);
        GameObject taskObj = GameObject.FindGameObjectWithTag("Task"+num);
        if (taskObj == null)
        {
            // Task was already destroyed and no new task has spawned yet
            print("No task was found with name Task"+num);
            return false;
        }
        TaskTimer taskTimer = taskObj.GetComponent<TaskTimer>();
        if (taskTimer.timeLeft <= 0)
        {
            // Task hasn't been destroyed yet, but the time was up
            print("Time is up!");
            return false;
        }

        TaskController taskController = taskObj.GetComponent<TaskController>();
        PhotonView taskPV = taskObj.GetComponent<PhotonView>();
        if (HasRequiredProducts(taskController))
        {
            ScoreController.Instance.IncrementScore(taskController.productAmount * 100);
            foreach (ProductController productController in GetComponentsInChildren<ProductController>())
            {
                PhotonView productPV = productController.GetComponent<PhotonView>();
                if (productPV.IsMine)
                {
                    // Destroy product
                    PhotonNetwork.Destroy(productController.gameObject);
                }
                else productPV.RPC("DestroyProduct", RpcTarget.OthersBuffered);
            }
            if (PV.IsMine)
            {
                // Destroy package
                PhotonNetwork.Destroy(gameObject);
            }
            else PV.RPC("DestroyPackage", RpcTarget.OthersBuffered);
            if (PhotonNetwork.IsMasterClient)
            {
                // Destroy task
                PhotonNetwork.Destroy(taskObj);
                TaskManager.Instance.GenerateNewTask(taskController.taskNr);
            }
            else taskPV.RPC("DestroyTask", RpcTarget.OthersBuffered);
            print("The package contained the required products!");
            return true;
        }
        else
        {
            print("The package did not contain the required products!");
            return false;
        }
    }

    [PunRPC]
    void DestroyPackage()
    {
        if (PV.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    bool HasRequiredProducts(TaskController task)
    {
        string[] packageProducts = GetProductTypes();
        string[] requiredProducts = task.requiredProducts;

        string pkgProducts = "{" + System.String.Join(", ", packageProducts) + "}";
        string rqrdProducts = "{" + System.String.Join(", ", requiredProducts) + "}";
        print("Package products: "+pkgProducts);
        print("Required products: "+rqrdProducts);

        if (packageProducts.Length <= 0) return false;

        return packageProducts.OrderBy(x=>x).SequenceEqual(requiredProducts.OrderBy(x=>x));
    }

    string[] GetProductTypes()
    {
        ProductController[] productControllers = GetComponentsInChildren<ProductController>();
        string[] packageProducts = new string[productControllers.Length];
        
        for (int i = 0; i < productControllers.Length; i++)
        {
            packageProducts[i] = productControllers[i].type;
        }

        return packageProducts;
    }
}
