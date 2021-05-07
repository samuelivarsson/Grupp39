using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ObjectManager : MonoBehaviour
{
    PhotonView PV;

    // The different products
    public static List<string> possibleProducts = new List<string>() {"Boat", "Laptop", "Ball", "Book", "Car", "Bear"};

    // The amount of different products
    static int productCount = possibleProducts.Count;

    // The amount of package spawn points
    const int packageCount = 1;

    List<int> productSpawnPointList = new List<int>();
    List<int> packageSpawnPointList = new List<int>();
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        for (int i = 0; i < productCount; i++)
        {
            productSpawnPointList.Add(i);
        }
        for (int i = 0; i < packageCount; i++)
        {
            packageSpawnPointList.Add(i);
        }
    }
        
    void Start()
    {
       if (PV.IsMine) CreateProduct();
    }

    void CreateProduct()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!PhotonNetwork.OfflineMode)
            {
                for (int i = 0; i < productCount; i++)
                {
                    Transform spawnPoint = GetProductSpawnPoint();
                    PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManager" + possibleProducts[i]), spawnPoint.position, spawnPoint.rotation);
                }
            }
            else
            {
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerBlue"), new Vector3(6, (float)0.7, (float)13.2), new Quaternion(0, 0, 0, 0));
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManagerGreen"), new Vector3(8, (float)0.7, (float)13.2), new Quaternion(0, 0, 0, 0));
            }
            
            for (int i = 0; i < packageCount; i++)
            {
                Transform pkgSpawnPoint = GetPackageSpawnPoint();
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "PackageManager"), pkgSpawnPoint.position, pkgSpawnPoint.rotation);
            }
        }
    }

    Transform GetProductSpawnPoint()
    {
        int index = Random.Range(0, productSpawnPointList.Count);
        int result = productSpawnPointList[index];
        productSpawnPointList.RemoveAt(index);
        return SpawnManager.Instance.GetProductSpawnPoint(result);
    }

    Transform GetPackageSpawnPoint()
    {
        int index = Random.Range(0, packageSpawnPointList.Count);
        int result = packageSpawnPointList[index];
        packageSpawnPointList.RemoveAt(index);
        return SpawnManager.Instance.GetPackageSpawnPoint(result);
    }
}