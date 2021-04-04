using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    public static PlayerLiftController myPlayerLiftController;
    public static PlayerPackController myPlayerPackController;

    PhotonView PV;
    Vector3 startPos = new Vector3(10.5f, 1.5f, 12.5f);
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        if(PV.IsMine)
        {
            CreateController();
        } 
    }

    void CreateController()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "NormalPlayerController"), startPos, Quaternion.identity);
        myPlayerLiftController = playerObj.GetComponent<PlayerLiftController>();
        myPlayerPackController = playerObj.GetComponent<PlayerPackController>();
    }
}
