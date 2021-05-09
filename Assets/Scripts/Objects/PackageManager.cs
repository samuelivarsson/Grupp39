using UnityEngine;
using Photon.Pun;
using System.IO;

public class PackageManager : MonoBehaviour, ICreateController
{
    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void CreateController(int playerViewID, Vector3 startPos)
    {
        PV.RPC("OnCreateController", RpcTarget.MasterClient, playerViewID, startPos);
    }

    [PunRPC]
    void OnCreateController(int playerViewID, Vector3 startPos)
    {
        PlayerLiftController playerLiftController = PhotonView.Find(playerViewID).GetComponent<PlayerLiftController>();
        GameObject obj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "PackageController"), startPos,  Quaternion.identity);
        playerLiftController.latestCollision = obj;
        int objViewID = obj.GetComponent<PhotonView>().ViewID;
        playerLiftController.canLiftID = objViewID;
        playerLiftController.GetComponent<PhotonView>().RPC("OnLift", RpcTarget.AllBufferedViaServer,objViewID, 0f);
    }
}
