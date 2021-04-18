using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    SpawnPoint[] playerSpawnPoints;
    SpawnPoint[] productSpawnPoints;
    SpawnPoint[] packageSpawnPoints;

    GameObject playerObj;
    GameObject productObj;
    GameObject packageObj;

    void Awake()
    {
        Instance = this;
        playerObj = transform.GetChild(0).gameObject;
        productObj = transform.GetChild(1).gameObject;
        packageObj = transform.GetChild(2).gameObject;
        playerSpawnPoints = playerObj.GetComponentsInChildren<SpawnPoint>();
        productSpawnPoints = productObj.GetComponentsInChildren<SpawnPoint>();
        packageSpawnPoints = packageObj.GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetPlayerSpawnPoint(int i)
    {
        return playerSpawnPoints[i].transform;
    }

    public Transform GetProductSpawnPoint(int i)
    {
        return productSpawnPoints[i].transform;
    }

    public Transform GetPackageSpawnPoint(int i)
    {
        return packageSpawnPoints[i].transform;
    }
}
