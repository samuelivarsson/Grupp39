using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TaskTimer : MonoBehaviour
{

    public float maxTime {get; set;}
    public float timeLeft {get; set;}
    [SerializeField] Image timerBar;
    [SerializeField] GameObject timesUpText;
    [SerializeField] GameObject taskWindow;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        timesUpText.SetActive(false);
    }

    void FixedUpdate()
    {
        if(timeLeft > 0)
        {
            timeLeft -= Time.fixedDeltaTime;
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
            HealthBarController.Instance.DecreaseHealth();
            Destroy(taskWindow);
            PV.RPC("OnTimesUp", RpcTarget.OthersBuffered);
        }
    }

    [PunRPC]
    void OnTimesUp()
    {
        Destroy(taskWindow);
    }
}
