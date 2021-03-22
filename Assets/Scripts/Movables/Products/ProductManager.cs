using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ProductManager : MonoBehaviour
{
    PhotonView PV;
    
    Vector3 productPos1 = new Vector3(13.2f, 0.95f, 11);
    Vector3 productPos2 = new Vector3(13.2f, 0.95f, 13);
 
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
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Products"), productPos1,  Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Products"), productPos2,  Quaternion.identity);
        }
    }
}
