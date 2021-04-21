using UnityEngine;
using TMPro;

public class PopupInfo : MonoBehaviour
{
    public static PopupInfo Instance;

    [SerializeField] TMP_Text infoText;

    bool timerActive = false;
    float timer;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timerActive)
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0)
            {
                timerActive = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void Popup(string text, float seconds)
    {
        infoText.text = text;
        timerActive = true;
        timer = seconds;
        gameObject.SetActive(true);
    }
}
