using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class TaskTimer : MonoBehaviour
{
    [SerializeField] Image timerBar;
    [SerializeField] GameObject timesUpText;
    [SerializeField] TMP_Text timeInt;

    public bool timerActive {get; set;} = false;
    public float maxTime {get; set;}
    public float timeLeft {get; set;}
    public float lastUpdate {get; set;}

    public bool hasDecreasedHealth {get; set;} = false;

    PhotonView PV;
    TaskController taskController;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        taskController = GetComponent<TaskController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        timesUpText.SetActive(false);
    }

    void FixedUpdate()
    {
        bool gameStarted = (bool) PhotonNetwork.CurrentRoom.CustomProperties["gameStarted"];
        if (!timerActive || !gameStarted) return;

        if (timeLeft > 0)
        {
            timeLeft -= Time.fixedDeltaTime;
            timerBar.fillAmount = timeLeft / maxTime;
            timeInt.text = ""+(int)timeLeft;
            if (lastUpdate - timeLeft > 2 && PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Sending");
                PV.RPC("OnUpdate", RpcTarget.Others, timeLeft, PhotonNetwork.ServerTimestamp);
                lastUpdate = timeLeft;
            }
        }
        else if (timeLeft > -1)
        {
            if (!hasDecreasedHealth)
            {
                HealthManager.Instance.DecreaseHealth();
                hasDecreasedHealth = true;
            }
            timesUpText.SetActive(true);
            timeLeft -= Time.fixedDeltaTime;
        }
        else
        {
            timerActive = false;
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
                TaskManager.Instance.GenerateNewTask(taskController.taskNr);
            }
        }
    }

    [PunRPC]
    void OnUpdate(float _timeLeft, int serverTimeStamp)
    {
        // Difference (lag) in milliseconds
        int diff = Mathf.Abs(PhotonNetwork.ServerTimestamp - serverTimeStamp);
        timeLeft = _timeLeft - diff/1000;
        Debug.LogError("Recieving-.");
        Debug.LogError("Active: "+timerActive);
        bool gameStarted = (bool) PhotonNetwork.CurrentRoom.CustomProperties["gameStarted"];
        Debug.LogError("gamestarted: "+gameStarted);
    }
}
