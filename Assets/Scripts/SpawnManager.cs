using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    SpawnPoint[] playerSpawnPoints;
    SpawnPoint[] productSpawnPoints;
    SpawnPoint[] packageSpawnPoints;

    [SerializeField] GameObject playerObj;
    [SerializeField] GameObject productObj;
    [SerializeField] GameObject packageObj;

    void Awake()
    {
        Instance = this;
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
