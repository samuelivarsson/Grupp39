using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ProductManager : MonoBehaviourPunCallbacks, ICreateController
{
    [SerializeField] int balance;
    string type;

    string balanceKey;
    PlayerLiftController playerLiftController;

    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        type = gameObject.name.Split('(')[0].Substring(gameObject.tag.Length);
        balanceKey = "balance" + PV.ViewID;
        playerLiftController = PlayerManager.myPlayerLiftController;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged[balanceKey] != null)
        {
            balance = (int)propertiesThatChanged[balanceKey];
        }
    }

    public bool CreateController()
    {
        if (playerLiftController.liftingID != -1)
        {
            print("You are already lifting something!");
            return false;
        }
        if (balance == 0)
        {
            print("Balance is 0!");
            return false;
        }
        object[] initData = {type};
        GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "Products", "Controllers", "ProductController"+type), Vector3.zero,  Quaternion.identity, 0, initData);
        playerLiftController.latestCollision = obj;
        playerLiftController.canLiftID = obj.GetComponent<PhotonView>().ViewID;

        Hashtable hash = new Hashtable();
        balance--;
        hash.Add(balanceKey, balance);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        return true;
    }
}
