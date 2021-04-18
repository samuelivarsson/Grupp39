using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    public const int maxHealth = 5;
    int healthLeft;

    GameObject[] healthObjects = new GameObject[maxHealth];

    GameObject canvasManager;
    PhotonView PV;
 
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
        
    void Start()
    {
        healthLeft = maxHealth;
        if (PhotonNetwork.IsMasterClient)
        {
            CreateHealth();
        }
    }

    void CreateHealth()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            object[] initData = {i};
            GameObject healthObj = PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "UI", "Health", "HealthController"), Vector3.zero,  Quaternion.identity, 0, initData);
            healthObjects[i] = healthObj;
        }
    }

    public void DecreaseHealth()
    {
        if (healthLeft == 0 || !PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.Destroy(healthObjects[healthLeft-1]);
        healthLeft--;
        if (healthLeft == 0) 
        {
            CanvasManager.Instance.GameOver();
        }    
    }
}
