using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreController : MonoBehaviourPunCallbacks
{
    GameObject canvasManager;

    Vector3 startPos = new Vector3(50, -25, 0);

    [SerializeField] int score;
    [SerializeField] Text text;

    public static ScoreController Instance;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

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