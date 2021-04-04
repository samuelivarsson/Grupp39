using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskTimer : MonoBehaviour
{

    Image timerBar;
    public float maxTime = 5f;
    [SerializeField] float timeLeft;
    [SerializeField] GameObject timesUpText;
    [SerializeField] GameObject taskWindow;


    // Start is called before the first frame update
    void Start()
    {
        timesUpText.SetActive(false);
        timerBar = GetComponent<Image>();
        timeLeft = maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerBar.fillAmount = timeLeft / maxTime;
        }
        else
        {
            timesUpText.SetActive(true);
            timeLeft -= (float) 0.01;
        }

        if(timeLeft < -1)
        {
            //taskWindow.SetActive(false);
            Destroy(taskWindow);
        }
    }
}
