using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ProductManager : MonoBehaviourPunCallbacks, ICreateController
{
    // [SerializeField] int balance;
    public string type {get; set;}

    // string balanceKey;

    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        type = gameObject.name.Split('(')[0].Substring(gameObject.tag.Length);
        // balanceKey = "balance" + PV.ViewID;
    }

    // public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    // {
    //     if (propertiesThatChanged[balanceKey] != null)
    //     {
    //         balance = (int)propertiesThatChanged[balanceKey];
    //     }
    // }

    public void CreateController(int playerViewID, Vector3 startPos)
    {
        // if (balance == 0)
        // {
        //     print("Balance is 0!");
        //     return false;
        // }
        PV.RPC("OnCreateController", RpcTarget.MasterClient, playerViewID, startPos);
        // Hashtable hash = new Hashtable();
        // balance--;
        // hash.Add(balanceKey, balance);
        // PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    [PunRPC]
    void OnCreateController(int playerViewID, Vector3 startPos)
    {
        PlayerLiftController playerLiftController = PhotonView.Find(playerViewID).GetComponent<PlayerLiftController>();
        object[] initData = {type};
        GameObject obj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Controllers", "ProductController"+type), startPos,  Quaternion.identity, 0, initData);
        playerLiftController.latestCollision = obj;
        int objViewID = obj.GetComponent<PhotonView>().ViewID;
        playerLiftController.canLiftID = objViewID;
        playerLiftController.GetComponent<PhotonView>().RPC("OnLift", RpcTarget.AllBufferedViaServer,objViewID, 0f);
    }
}
