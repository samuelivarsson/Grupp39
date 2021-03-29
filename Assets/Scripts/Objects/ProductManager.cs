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
    bool canPickUp;
    Transform player;

    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        balanceKey = "balance" + PV.ViewID;
        player = PlayerManager.myPlayerController.transform;
    }

    void Update()
    {
        CheckLiftAndDrop();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged[balanceKey] != null)
        {
            balance = (int)propertiesThatChanged[balanceKey];
        }
    }

    void CreateController()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        ProductController productController;
        GameObject packageControllerObj;

        if (pc.GetLiftingID() != -1)
        {
            Debug.Log("You are already lifting something!");
            return;
        }
        if (balance == 0)
        {
            Debug.Log("Balance is 0!");
            return;
        }
        if (balance == -1)
        {
            packageControllerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "PackageController"), Vector3.zero,  Quaternion.identity);
            productController = packageControllerObj.GetComponent<ProductController>();
            productController.Lift();
            return;
        }

        packageControllerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "Products", "Controllers", "ProductController"+type), Vector3.zero,  Quaternion.identity);
        productController = packageControllerObj.GetComponent<ProductController>();
        productController.SetType(type);
        productController.Lift();

        Hashtable hash = new Hashtable();
        balance--;
        hash.Add(balanceKey, balance);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    private void CheckLiftAndDrop()
    {
        if (canPickUp && Input.GetKeyDown(KeyCode.Space))
        {
            CreateController();
        }
    }

    public void SetCanPickUp(bool _canPickUp)
    {
        canPickUp = _canPickUp;
    }
}
