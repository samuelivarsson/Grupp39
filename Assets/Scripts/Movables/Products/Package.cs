using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class Package : MonoBehaviour
{
    //List<GameObject> droppedDeliveries = new List<GameObject>();
    //int gatheredPoints = 0;
    bool spaceKeyWasPressed;

    Rigidbody rb;
    PhotonView PV;
    public ScoreController score;
    [SerializeField] Transform player;
    GameObject[] players;
    Transform hand;
    Transform pic1;
    Transform pic2;
    Transform pic3;
    bool isLifted;
    

    bool canPickUp;
    Transform latestPlayer;
    private bool canPackage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        hand = player.GetChild(0);
        pic1 = gameObject.transform.GetChild(0);
        pic2 = gameObject.transform.GetChild(1);
        pic3 = gameObject.transform.GetChild(2);
    }

    // Update is called once per frame
    void Update()
    {
        CheckLiftAndDrop();
        
        if (canPackage)
        {
            
            if (Input.GetKeyDown(KeyCode.LeftControl) && latestPlayer.GetComponentInChildren<ProductController>() && gameObject.transform.childCount<6)
            {
               ProductController prodController = latestPlayer.GetComponentInChildren<ProductController>();
               prodController.setIsLifted(false);
               Transform prod = prodController.transform;
               latestPlayer.GetComponentInChildren<ProductController>().transform.parent = gameObject.transform;
               prod.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                if (gameObject.transform.childCount <= 3)
                {
                   prod.localPosition = pic1.transform.localPosition;
                }
                if (gameObject.transform.childCount == 4)
                {
                    prod.localPosition = pic2.transform.localPosition;
                }
                if (gameObject.transform.childCount == 5)
                {
                    prod.localPosition = pic3.transform.localPosition;
                }
            }
            
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && canPickUp)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }


    }

    private void CheckLiftAndDrop()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isLifted)
        {
            Drop();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isLifted && canPickUp)
        {
            Lift();
        }
    }

    public void Lift()
    {
        gameObject.transform.parent = latestPlayer;
        gameObject.transform.localPosition = hand.transform.localPosition;
        isLifted = true;
        PlayerController playerController = latestPlayer.GetComponent<PlayerController>();
        if(playerController.transform.parent.tag == "Package")
        {
            playerController.setIsLifting(true);
        }
        PV.RPC("OnLift", RpcTarget.OthersBuffered, latestPlayer.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnLift(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        gameObject.transform.parent = player.transform;
        gameObject.transform.localPosition = hand.transform.localPosition;
    }

    public void Drop()
    {
        gameObject.transform.parent = null;
        isLifted = false;
        canPickUp = false;
        PlayerController playerController = latestPlayer.GetComponent<PlayerController>();
        playerController.setIsLifting(false);
        PV.RPC("OnDrop", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnDrop()
    {
        gameObject.transform.parent = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DropDown"))
        {
            //droppedDeliveries.Add(gameObject);
            //gatheredPoints++;
            Destroy(gameObject);
            score.IncrementScore(1);
            //Debug.Log("Points:"+ gatheredPoints);
        }
    }

    public void setCanPickUp(bool _canPickUp)
    {
        canPickUp = _canPickUp;
    }

    public void setLatestPlayer(Transform player)
    {
        latestPlayer = player;
    }

    public void setLifted(bool _canPackage)
    {
        canPackage = _canPackage;
    }
}
