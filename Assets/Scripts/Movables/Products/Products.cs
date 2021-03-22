using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Products : MonoBehaviourPunCallbacks
{
    [SerializeField] int balance;
    string balanceKey;
    bool canPickUp;
    Transform latestPlayer;

    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        balanceKey = "balance" + PV.ViewID;
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
        PlayerController pc = latestPlayer.GetComponent<PlayerController>();
        if (pc.isLifting)
        {
            Debug.Log("You are already lifting something!");
        }
        if (balance == 0)
        {
            Debug.Log("Balance is 0!");
        }
        if (!pc.isLifting && balance > 0)
        {
            GameObject productControllerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ProductController"), Vector3.zero,  Quaternion.identity);
            ProductController productController = productControllerObj.GetComponent<ProductController>();
            productController.setLatestPlayer(latestPlayer);
            productController.Lift();

            Hashtable hash = new Hashtable();
            balance--;
            hash.Add(balanceKey, balance);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    private void CheckLiftAndDrop()
    {
        if (canPickUp && Input.GetKeyDown(KeyCode.Space))
        {
            CreateController();
        }
    }

    public void setCanPickUp(bool _canPickUp)
    {
        canPickUp = _canPickUp;
    }

    public void setLatestPlayer(Transform player)
    {
        latestPlayer = player;
    }
}
