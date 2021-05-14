using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PackageMultiLiftController : MonoBehaviour
{
    public List<int> lifters {get; set;} = new List<int>(Launcher.maxPlayers);
    public bool tooHeavy {get; set;} = false;

    // 4 sides of a package.
    public List<Vector3> takenAnchors {get; set;} = new List<Vector3>(4);

    PhotonView PV;
    Rigidbody rb;
    PackageController packageController;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        packageController = GetComponent<PackageController>();
    }

    public void AddLifter(PlayerLiftController addedPlayerLC)
    {
        if (lifters.Count != 0)
        {
            Debug.LogError("Tried to add lifter when Lifters.Count was not 0!");
            return;
        }
        PhotonView playerPV = addedPlayerLC.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Contains(viewID)) lifters.Add(viewID);
        SetMultiLiftBools(null);
    }

    public void RemoveLifter(PlayerLiftController removedPlayerLC)
    {
        if (lifters.Count != 1)
        {
            Debug.LogError("Tried to remove lifter when Lifters.Count was not 1!");
            return;
        }
        PhotonView playerPV = removedPlayerLC.GetComponent<PhotonView>();
        int viewID = playerPV.ViewID;
        if (!lifters.Remove(viewID)) print("Couldn't remove lifter");
        SetMultiLiftBools(removedPlayerLC);
    }

    public void AddHelper(PlayerLiftController addedPlayerLC, Vector3 anchor)
    {
        if (lifters.Count == 0)
        {
            Debug.LogError("Tried to add helper when Lifters.Count was 0!");
            return;
        }

        PhotonView addedPlayerPV = addedPlayerLC.GetComponent<PhotonView>();
        int addedViewID = addedPlayerPV.ViewID;
        if (!lifters.Contains(addedViewID)) lifters.Add(addedViewID);

        bool iAmLifting = lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID);

        if (iAmLifting)
        {
            // I am a lifter -> I was either added or was already in the lifters list.

            // Disable transform views on all lifters except myself
            SetLiftersTransformView(false, true);
            // Kinematic = false on all lifters for me
            SetLiftersKinematic(false);
        }

        // Add confjoints

        if (lifters.Count == 2)
        {
            // First helper was added -> Make original lifter a helper.

            // Add rb to package for everyone when someone starts to multilift this package.
            rb = gameObject.AddComponent<Rigidbody>();

            // Remove parent from package and setup joint for parent
            gameObject.transform.parent = null;
            PhotonView parentPV = PhotonView.Find(lifters[0]);
            Vector3 _anchor = CalculateLocalAnchor(parentPV.GetComponent<PlayerLiftController>());
            ConnectPlayer(parentPV.GetComponent<Rigidbody>(), _anchor);
        }
        // Setup joint for added player with anchor
        ConnectPlayer(addedPlayerLC.GetComponent<Rigidbody>(), anchor);

        SetMultiLiftBools(null);
    }
    
    public void RemoveHelper(PlayerLiftController removedPlayerLC)
    {
        if (lifters.Count < 2)
        {
            Debug.LogError("Tried to remove helper when Lifters.Count was less than 2!");
            return;
        }

        PhotonView removedPlayerPV = removedPlayerLC.GetComponent<PhotonView>();
        int removedViewID = removedPlayerPV.ViewID;
        if (!lifters.Remove(removedViewID)) print("Couldn't remove lifter");

        bool iAmLifting = (lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID));
        bool myPlayer = PhotonNetwork.LocalPlayer.ActorNumber == removedPlayerPV.CreatorActorNr;

        if (iAmLifting)
        {
            // I am a lifter -> I was not removed.

            // Start reading from this player's stream again.
            removedPlayerPV.GetComponent<PhotonTransformView>().enabled = true;
            // I was not removed -> Set kinematic true for the removed player.
            removedPlayerPV.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            // I am not a lifter -> I was either removed or only watching.
            if (myPlayer)
            {
                // It was my player that was removed

                // Enable transform views on all lifters again
                SetLiftersTransformView(true);
            }
            // Set all lifters to kinematic
            SetLiftersKinematic(true);
        }

        List<ConfigurableJoint> confJoints = new List<ConfigurableJoint>(GetComponents<ConfigurableJoint>());
        DestroyPlayerJoint(confJoints, removedViewID);
        removedPlayerLC.GetComponent<PlayerController>().rotateDir = removedPlayerLC.transform.rotation * Vector3.forward;
        removedPlayerLC.GetComponent<PlayerController>().rotation = removedPlayerLC.transform.rotation;

        if (lifters.Count < 2)
        {
            // Only 1 lifter remaining -> Package isn't being multilifted anymore.
            if (confJoints.Count > 1) Debug.LogError("MORE THAN ONE CONFJOINT!");

            // Destroy last joint.
            Destroy(confJoints[0]);

            // Destroy rb because the package isn't being multilifted anymore.
            Destroy(rb);
            rb = null;

            PlayerLiftController lastPlayerLC = PhotonView.Find(lifters[0]).GetComponent<PlayerLiftController>();
            PhotonView lastPlayerPV = lastPlayerLC.GetComponent<PhotonView>();

            // Lift the package normally with the last player.
            gameObject.transform.parent = lastPlayerLC.gameObject.transform;
            gameObject.transform.localPosition = lastPlayerLC.hand.transform.localPosition;
            lastPlayerLC.GetComponent<PlayerController>().rotateDir = lastPlayerLC.transform.rotation * Vector3.forward;
            lastPlayerLC.GetComponent<PlayerController>().rotation = lastPlayerLC.transform.rotation;
        }
        SetMultiLiftBools(removedPlayerLC);
    }

    // -------------------------------------------- Helper Methods --------------------------------------------

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
        tooHeavy = totalStrength < packageController.productCount;
        foreach (int viewID in lifters)
        {
            PlayerMultiLiftController playerMLC = PhotonView.Find(viewID).GetComponent<PlayerMultiLiftController>();
            playerMLC.tooHeavy = tooHeavy;
            playerMLC.iAmLifting = lifters.Contains(PlayerManager.myPlayerLiftController.GetComponent<PhotonView>().ViewID);
            playerMLC.isMultiLifting = lifters.Count > 1;
        }
    }

    void SetConfJoint(ConfigurableJoint confJoint, Rigidbody conBody, Vector3 anchor)
    {
        confJoint.connectedBody = conBody;
        confJoint.anchor = anchor;
        confJoint.axis = Vector3.zero;
        confJoint.autoConfigureConnectedAnchor = false;
        confJoint.connectedAnchor = new Vector3(0, 0.8f, 0.4f);
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

    public Vector3 CalculateLocalAnchor(PlayerLiftController player)
    {
        float offset1 = 0.4f;
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

    void SetLiftersTransformView(bool b, bool exceptMyself = false)
    {
        foreach (int vid in lifters)
        {
            PhotonView playerPV = PhotonView.Find(vid);
            if (exceptMyself && playerPV.CreatorActorNr == PhotonNetwork.LocalPlayer.ActorNumber) continue;
            playerPV.GetComponent<PhotonTransformView>().enabled = b;
        }
    }

    void SetLiftersKinematic(bool b)
    {
        foreach (int vid in lifters)
        {
            PhotonView playerPV = PhotonView.Find(vid);
            playerPV.GetComponent<Rigidbody>().isKinematic = b;
        }
    }

    void ConnectPlayer(Rigidbody playerRB, Vector3 anchor)
    {
        ConfigurableJoint confJoint = gameObject.AddComponent<ConfigurableJoint>();
        SetConfJoint(confJoint, playerRB, anchor);
    }

    void DestroyPlayerJoint(List<ConfigurableJoint> confJoints, int playerViewID)
    {
        for (int i = 0; i < confJoints.Count;)
        {
            if (confJoints[i].connectedBody.GetComponent<PhotonView>().ViewID == playerViewID) 
            {
                Destroy(confJoints[i]);
                confJoints.Remove(confJoints[i]);
            }
            else i++;
        }
    }
}
