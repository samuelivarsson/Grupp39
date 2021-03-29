using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ObjectManager : MonoBehaviour
{
    PhotonView PV;
    
    Vector3 productPos1 = new Vector3(13.2f, 0.95f, 11f);
    Vector3 productPos2 = new Vector3(13.2f, 0.95f, 13f);
    Vector3 productPos3 = new Vector3(1.8f, 0.95f, 7f);
    Vector3 productPos4 = new Vector3(1.8f, 0.95f, 9f);
    Vector3 productPos5 = new Vector3(2f, 2.45f, 13.2f);
    Vector3 productPos6 = new Vector3(3.5f, 2.2f, 7f);

    Vector3 packagePos = new Vector3(14.5f, 0.91f, 5.5f);
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
        
    void Start()
    {
       if (PV.IsMine) CreateProduct();
    }

    void CreateProduct()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerRed"), productPos1,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerBlue"), productPos2,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerGreen"), productPos3,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerPink"), productPos4,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerYellow"), productPos5,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerCyan"), productPos6,  Quaternion.identity);

            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "PackageManager"), packagePos, Quaternion.identity);
        }
    }
}
