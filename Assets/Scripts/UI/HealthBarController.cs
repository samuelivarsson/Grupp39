using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthBarController : MonoBehaviour
{
    GameObject canvasManager;

    Vector3 startPos = new Vector3(0, 600, 0);

    public static HealthBarController Instance;
    RectTransform rectTransform;
    int heartsLeft = 3;
    public bool gameOver = false;

    void Awake() 
    {
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(1, 1, 1);
        rectTransform.offsetMin = new Vector2(0,0);
        rectTransform.offsetMax = new Vector2(0,0);
        
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void DecreaseHealth()
    {
        if(heartsLeft == 0) 
        {
            return;
        }
        GameObject health = GameObject.FindGameObjectWithTag("Health" + heartsLeft);
        Destroy(health);
        heartsLeft--;
        if(heartsLeft == 0) 
        {
            CanvasManager.Instance.OpenGameOverMenu();
        }    
    }  
}
