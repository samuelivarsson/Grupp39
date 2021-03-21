using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    public int score;
    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    public void Change(int change)
    {
        score += change;
        text.text = score.ToString();
    }
}