using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreController : MonoBehaviourPunCallbacks
{
    public static ScoreController Instance;

    [SerializeField] Text text;

    public int score {get; set;}
    
    Vector3 startPos = new Vector3(50, -25, 0);

    GameObject canvasManager;

    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        score = 0;
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = startPos;
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void IncrementScore(int change)
    {
        Hashtable hash = new Hashtable();
        score += change;
        hash.Add("score", score);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        text.text = score.ToString();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged["score"] != null)
        {
            score = (int)propertiesThatChanged["score"];
            text.text = score.ToString();
        }
    }
}