using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ProductManager : MonoBehaviourPunCallbacks, ICreateController
{
    [SerializeField] int balance;
    public string type {get; set;}

    string balanceKey;
    PlayerLiftController playerLiftController;

    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        type = gameObject.name.Split('(')[0].Substring(gameObject.tag.Length);
        balanceKey = "balance" + PV.ViewID;
        playerLiftController = PlayerManager.myPlayerLiftController;
        SetMaterials();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged[balanceKey] != null)
        {
            balance = (int)propertiesThatChanged[balanceKey];
        }
    }

    public bool CreateController(Vector3 startPos)
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
        GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "Products", "Controllers", "ProductController"+type), startPos,  Quaternion.identity, 0, initData);
        playerLiftController.latestCollision = obj;
        playerLiftController.canLiftID = obj.GetComponent<PhotonView>().ViewID;

        Hashtable hash = new Hashtable();
        balance--;
        hash.Add(balanceKey, balance);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        return true;
    }

    void SetMaterials()
    {
        // switch (type)
        // {
        //     case "Ball":
        //         GetComponentInChildren<Renderer>().materials.CopyTo(PickUpCheck.standardBall, 0);
        //         break;

        //     case "Bear":
        //         GetComponentInChildren<Renderer>().materials.CopyTo(PickUpCheck.standardBear, 0);
        //         break;
            
        //     case "Boat":
        //         GetComponentInChildren<Renderer>().materials.CopyTo(PickUpCheck.standardBoat, 0);
        //         break;

        //     case "Book":
        //         GetComponentInChildren<Renderer>().materials.CopyTo(PickUpCheck.standardBook, 0);
        //         break;

        //     case "Car":
        //         GetComponentInChildren<Renderer>().materials.CopyTo(PickUpCheck.standardCar, 0);
        //         break;

        //     case "Laptop":
        //         GetComponentInChildren<Renderer>().materials.CopyTo(PickUpCheck.standardLaptop, 0);
        //         break;
        // }
    }
}
