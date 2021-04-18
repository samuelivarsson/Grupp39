using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public bool gameOver {get; set;} = false;
    public int healthNr {get; set;}

    GameObject canvasManager;
    RectTransform rectTransform;

    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
    }

    void Start() 
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = new Vector3(GetX(), -50, 0);
        rectTransform.localScale = new Vector3(1, 1, 1);
    }

    int GetX()
    {
        int maxHealth = HealthManager.maxHealth;
        if (maxHealth % 2 == 0)
        {
            int offset = maxHealth/2 - 1;
            return -25-(offset*50) + 50*healthNr;
        }
        else
        {
            int offset = (maxHealth-1)/2;
            return -(offset*50) + 50*healthNr;
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        healthNr = (int) initData[0];
    }
}
