using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapeTimer : MonoBehaviour
{

    Image timerBar;
    [SerializeField] float maxTime = 5f;
    [SerializeField] float timeLeft;
 


    // Start is called before the first frame update
    void Start()
    {
        timerBar = GetComponent<Image>();
        timeLeft = maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerBar.fillAmount = timeLeft / maxTime;
        }
        else
        { 
            timeLeft -= (float)0.01;
        }

    }
}
