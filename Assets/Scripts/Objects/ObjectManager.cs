using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ObjectManager : MonoBehaviour
{
    PhotonView PV;

    // The different products
    static string[] possibleProducts = {"Blue", "Red", "Cyan", "Green", "Yellow", "Pink"};

    // The amount of different products
    static int productCount = possibleProducts.Length;

    // The amount of package spawn points
    const int packageCount = 2;

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
            for (int i = 0; i < productCount; i++)
            {
                Transform spawnPoint = GetProductSpawnPoint();
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Objects", "Products", "Managers", "ProductManager"+possibleProducts[i]), spawnPoint.position,  spawnPoint.rotation);
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