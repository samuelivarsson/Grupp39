using UnityEngine;
using Photon.Pun;
using System.IO;

public class PackageManager : MonoBehaviour, ICreateController
{
    PlayerLiftController playerLiftController;
 
    void Awake()
    {
        playerLiftController = PlayerManager.myPlayerLiftController;
    }

    public bool CreateController()
    {
        if (playerLiftController.liftingID != -1)
        {
            Debug.Log("You are already lifting something!");
            return false;
        }
        GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Objects", "PackageController"), Vector3.zero,  Quaternion.identity);
        playerLiftController.latestCollision = obj;
        playerLiftController.canLiftID = obj.GetComponent<PhotonView>().ViewID;

        return true;
    }
}
