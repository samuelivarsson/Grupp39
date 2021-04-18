using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    public static PlayerLiftController myPlayerLiftController;
    public static PlayerPackController myPlayerPackController;

    PhotonView PV;
 
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
        Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        string character = (string) hash[PhotonNetwork.LocalPlayer.NickName+"Character"];
        int spIndex = (int) hash[PhotonNetwork.LocalPlayer.NickName+"SpawnPoint"];
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(spIndex);
        GameObject playerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", character+"PlayerController"), spawnPoint.position, spawnPoint.rotation);
        myPlayerLiftController = playerObj.GetComponent<PlayerLiftController>();
        myPlayerPackController = playerObj.GetComponent<PlayerPackController>();
        playerObj.GetComponent<Character>().characterType = character;
    }
}
