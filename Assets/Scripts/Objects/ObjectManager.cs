using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ObjectManager : MonoBehaviour
{
    PhotonView PV;
    
    Vector3 productPos1 = new Vector3(13.2f, 0.95f, 11);
    Vector3 productPos2 = new Vector3(13.2f, 0.95f, 13);
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
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "ProductManager"), productPos2,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "ProductManager"), productPos1,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "PackageManager"), packagePos, Quaternion.identity);
        }
    }
}
