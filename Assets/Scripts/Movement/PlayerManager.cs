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
        if(PhotonNetwork.OfflineMode)
        {
            SpawnPlayerController("Normal", 0);
            return;
        }

        Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        string character = (string) hash[PhotonNetwork.LocalPlayer.UserId+"Character"];
        int spIndex = (int) hash[PhotonNetwork.LocalPlayer.UserId+"SpawnPoint"];

        SpawnPlayerController(character, spIndex);
    }

    void SpawnPlayerController(string character, int spIndex)
    {
        Transform spawnPoint = SpawnManager.Instance.GetPlayerSpawnPoint(spIndex);
        GameObject playerObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", character + "PlayerController"), spawnPoint.position, spawnPoint.rotation);

        myPlayerLiftController = playerObj.GetComponent<PlayerLiftController>();
        myPlayerPackController = playerObj.GetComponent<PlayerPackController>();
        playerObj.GetComponent<Character>().characterType = character;
    }
}
