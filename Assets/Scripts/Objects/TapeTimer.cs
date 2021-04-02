using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapeTimer : MonoBehaviour
{

    Image timerBar;
    [SerializeField] float maxTime = 5f;
    [SerializeField] float timeLeft;
    PackageController packageController;
    PlayerPackController playerPackController;
    bool doneTaping = false;
    
   
    void Awake()
    {
        packageController = GetComponentInParent<PackageController>();
        playerPackController = PlayerManager.myPlayerPackController;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        timerBar = GetComponent<Image>();
        timeLeft = maxTime;
    }

    // Update is called once per frame
     public void Update()
    {
        if (packageController.isTaped && !doneTaping)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                timerBar.fillAmount = timeLeft / maxTime;
            }
            else
            {
                timeLeft -= (float)0.01;
                packageController.transform.GetChild(5).transform.GetChild(0).gameObject.SetActive(false);
                packageController.transform.GetChild(5).transform.GetChild(1).gameObject.SetActive(true);
                playerPackController.isTaping = false;
                doneTaping = true;
            }
        }
    }
}
