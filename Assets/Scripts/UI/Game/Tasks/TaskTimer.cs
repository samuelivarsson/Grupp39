using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TaskTimer : MonoBehaviour
{
    [SerializeField] Image timerBar;
    [SerializeField] GameObject timesUpText;

    public bool timerActive {get; set;} = false;
    public float maxTime {get; set;}
    public float timeLeft {get; set;}

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
        if (!timerActive || !TaskManager.gameStarted) return;

        if (timeLeft > 0)
        {
            timeLeft -= Time.fixedDeltaTime;
            timerBar.fillAmount = timeLeft / maxTime;
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
}
