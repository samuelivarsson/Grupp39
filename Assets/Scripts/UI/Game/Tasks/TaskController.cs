using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.UI;

public class TaskController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public int taskNr {get; set;}
    public int productAmount {get; set;}

    [SerializeField] Material[] materials;
    [SerializeField] Image bg;
    [SerializeField] Texture[] pictures;

    [SerializeField] GameObject[] slots;
    [SerializeField] GameObject[] pluss;

    Dictionary<string, Texture> pics = new Dictionary<string, Texture>();

    GameObject canvasManager;
    TaskTimer taskTimer;
    PhotonView PV;

    // The objects in the order
    public string[] requiredProducts {get; set;}

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        taskTimer = GetComponentInChildren<TaskTimer>();
        canvasManager = CanvasManager.Instance.gameObject;
        gameObject.transform.SetParent(canvasManager.transform);
    }

    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = new Vector3(-50, -50 - 250 * taskNr, 0);
        rectTransform.localScale = new Vector3(1, 1, 1);
        bg.color = materials[taskNr].color;
    }

    [PunRPC]
    void DestroyTask()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.Destroy(gameObject);
        TaskManager.Instance.GenerateNewTask(taskNr);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        string tag = (string) initData[0];
        gameObject.tag = tag;

        taskNr = (int) initData[1];
        productAmount = (int) initData[2];

        requiredProducts = (string[]) initData[3];

        SetDictionary();
        SetPictures(requiredProducts);

        int time = (int) initData[4];
        taskTimer.maxTime = time;
        taskTimer.timeLeft = time;
        taskTimer.lastUpdate = time;
        taskTimer.timerActive = true;
    }

    void SetDictionary()
    {
        for (int i = 0; i < pictures.Length; i++)
        {
            pics.Add(ObjectManager.possibleProducts[i], pictures[i]);
        }
    }

    public void SetPictures(string[] requiredProducts)
    {
        
        for (int i = 0; i < requiredProducts.Length; i++)
        {
            // Activate slots and plus signs in between
            slots[i].SetActive(true);
            if (i > 0) pluss[i-1].SetActive(true);

            // Set pictures
            RawImage rawImage = slots[i].GetComponentInChildren<RawImage>();
            rawImage.texture = pics[requiredProducts[i]];
            rawImage.SizeToParent();
        }
    }
}

static class CanvasExtensions
{
    public static Vector2 SizeToParent(this RawImage image, float padding = 0) {
        float w = 0, h = 0;
        var parent = image.GetComponentInParent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();

        // check if there is something to do
        if (image.texture != null) {
            if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90) {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding) { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
        }
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }
}