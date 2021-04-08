using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthBarController : MonoBehaviour
{
    GameObject canvasManager;

    Vector3 startPos = new Vector3(960, 540, 0);

    public static HealthBarController Instance;

    int heartsLeft = 3;

    void Awake() 
    {
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = startPos;
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void DecreaseHealth()
    {
        GameObject health = GameObject.FindGameObjectWithTag("Health" + heartsLeft);
        Destroy(health);
        heartsLeft--;
    }
}
