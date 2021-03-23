using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    GameObject canvasManager;

    Vector3 startPos = new Vector3(50, -25, 0);

    [SerializeField] int score;
    [SerializeField] Text text;

    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        score = 0;
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = startPos;
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    public void IncrementScore(int change)
    {
        score += change;
        text.text = score.ToString();
    }
}