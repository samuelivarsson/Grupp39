using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PopupInfo : MonoBehaviour
{
    public static PopupInfo Instance;

    [SerializeField] TMP_Text infoText;

    CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Popup(string text, float seconds)
    {
        gameObject.SetActive(true);
        StartCoroutine(StartPopupTimer(text, seconds));
    }

    IEnumerator StartPopupTimer(string text, float sec)
    {
        infoText.text = text;
        print("Setting: "+text);
        for (float i = sec; i >= 0; i -= Time.deltaTime)
        {
            // Waiting
            yield return null;
        }
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        for (float i = canvasGroup.alpha; i >= 0; i -= Time.deltaTime)
        {
            canvasGroup.alpha = i;
            yield return null;
        }
        gameObject.SetActive(false);
        canvasGroup.alpha = 1;
    }
}
