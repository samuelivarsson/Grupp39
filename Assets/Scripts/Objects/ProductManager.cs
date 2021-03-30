using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ProductManager : MonoBehaviourPunCallbacks
{
    [SerializeField] int balance;
    [SerializeField] string type;

    string balanceKey;
    PlayerLiftController playerLiftController;

    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
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
        GameObject obj;
        ProductController productController;

        if (playerLiftController.liftingID != -1)
        {
            Debug.Log("You are already lifting something!");
            return false;
        }
        if (balance == 0)
        {
            Debug.Log("Balance is 0!");
            return false;
        }
        if (balance == -1)
        {
            obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "PackageController"), Vector3.zero,  Quaternion.identity);
            playerLiftController.latestCollision = obj;
            playerLiftController.canLiftID = obj.GetComponent<PhotonView>().ViewID;
            return true;
        }

        obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "Products", "Controllers", "ProductController"+type), Vector3.zero,  Quaternion.identity);
        playerLiftController.latestCollision = obj;
        playerLiftController.canLiftID = obj.GetComponent<PhotonView>().ViewID;
        productController = obj.GetComponent<ProductController>();
        productController.type = type;

        Hashtable hash = new Hashtable();
        balance--;
        hash.Add(balanceKey, balance);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        return true;
    }
}
