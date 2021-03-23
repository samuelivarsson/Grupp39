using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TaskController : MonoBehaviour
{
    GameObject canvasManager;
    [SerializeField] int taskNr;

    PhotonView PV;

    // The objects in the order
    private List<GameObject> orderedProducts;
    //[SerializeField] RenderTexture product;
    //[SerializeField] RawImage rawImage;

    void Awake()
    {
        canvasManager = CanvasManager.Instance.gameObject;
        PV = GetComponent<PhotonView>();
        gameObject.transform.SetParent(canvasManager.transform);
        GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-160, -110 - 250 * (taskNr-1), 0);
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        //rawImage.texture = product;
    }
}
