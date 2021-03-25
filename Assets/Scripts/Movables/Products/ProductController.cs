using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class ProductController : MonoBehaviour
{
    //List<GameObject> droppedDeliveries = new List<GameObject>();
    //int gatheredPoints = 0;
    bool spaceKeyWasPressed;

    Rigidbody rb;
    PhotonView PV;
    Transform player;
    PlayerController playerController;
    Transform hand;
    bool isLifted;

    bool canPickUp;
    Vector3 tileOffset = new Vector3(1.5f, 0.25f, 1.5f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        player = PlayerManager.myPlayerController.transform;
        playerController = player.GetComponent<PlayerController>();
        hand = player.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckLiftAndDrop();
    }

    private void CheckLiftAndDrop () 
    {
        GameObject latestTile = playerController.GetLatestTile();
        if (Input.GetKeyDown(KeyCode.Space) && isLifted && playerController.GetIsLifting() && latestTile && (latestTile.tag == "PlaceableTile" || latestTile.tag == "DropZone"))
        {
            Drop(latestTile);
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isLifted && canPickUp && !playerController.GetIsLifting())
        {
            Lift();
        }
    }

    public void Lift()
    {
        gameObject.transform.parent = player;
        gameObject.transform.localPosition = hand.transform.localPosition;
        float eulerY = closestAngle(gameObject.transform.localRotation.eulerAngles.y);
        gameObject.transform.localRotation = Quaternion.Euler(0, eulerY, 0);
        isLifted = true;
        playerController.SetIsLifting(true);
        PV.RPC("OnLift", RpcTarget.OthersBuffered, player.GetComponent<PhotonView>().ViewID, eulerY);
    }

    [PunRPC]
    void OnLift(int viewID, float eulerY)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        gameObject.transform.parent = player.transform;
        gameObject.transform.localPosition = hand.transform.localPosition;
        gameObject.transform.localRotation = Quaternion.Euler(0, eulerY, 0);
        isLifted = true;
    }

    public void Drop(GameObject latestTile)
    {
        isLifted = false;
        canPickUp = false;
        playerController.SetIsLifting(false);

        if (latestTile.CompareTag("DropZone"))
        {
            Deliver();
            PV.RPC("OnDeliver", RpcTarget.OthersBuffered);
            return;
        }

        // Add parent and fix position & rotation
        gameObject.transform.parent = latestTile.transform;
        gameObject.transform.localPosition = tileOffset;
        float eulerY = closestAngle(gameObject.transform.rotation.eulerAngles.y);
        gameObject.transform.rotation = Quaternion.Euler(0, eulerY, 0);
        PV.RPC("OnDrop", RpcTarget.OthersBuffered, latestTile.name, eulerY);
    }

    [PunRPC]
    void OnDrop(string tileName, float eulerY)
    {
        GameObject tile = GameObject.Find(tileName);
        gameObject.transform.parent = tile.transform;
        gameObject.transform.localPosition = tileOffset;
        gameObject.transform.rotation = Quaternion.Euler(0, eulerY, 0);
        isLifted = false;
        canPickUp = false;
    }

    void Deliver()
    {
        //droppedDeliveries.Add(gameObject);
        Destroy(gameObject);
        ScoreController.Instance.IncrementScore(1);
    }

    [PunRPC]
    void OnDeliver()
    {
        Destroy(gameObject);
    }
    
    public void setCanPickUp(bool _canPickUp)
    {
        canPickUp = _canPickUp;
    }

    private float closestAngle(float a)
    {
        float[] w = {0, 90, 180, 270};
        float currentNearest = w[0];
        float currentDifference = Mathf.Abs(currentNearest - a);

        for (int i = 1; i < w.Length; i++)
        {
            float diff = Mathf.Abs(w[i] - a);
            if (diff < currentDifference)
            {
                currentDifference = diff;
                currentNearest = w[i];
            }
        }

        return currentNearest;
    }
}
