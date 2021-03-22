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
    public ScoreController score;
    [SerializeField] Transform player;
    GameObject[] players;
    Transform hand;
    bool isLifted;

    bool canPickUp;
    Transform latestPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        hand = player.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckLiftAndDrop();

        if (PV.OwnerActorNr == 0)
        {
            if (gameObject.transform.parent != null && !isLifted)
            {
                gameObject.transform.parent = null;
            }
        }
        else
        {
            if (gameObject.transform.parent == null && !isLifted && PV.OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players)
                {
                    if (player.GetPhotonView().OwnerActorNr == PV.OwnerActorNr)
                    {
                        gameObject.transform.parent = player.transform;
                        gameObject.transform.localPosition = hand.transform.localPosition;
                    }
                }
            }
        }
    }

    private void CheckLiftAndDrop () 
    {
        if (Input.GetKeyDown(KeyCode.Space) && isLifted && PV.IsMine)
        {
            Drop();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isLifted && canPickUp)
        {
            if (PV.OwnerActorNr == 0)
            {
                Lift();
            }
        }
    }

    public void Lift()
    {
        PV.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        gameObject.transform.parent = latestPlayer;
        gameObject.transform.localPosition = hand.transform.localPosition;
        isLifted = true;
        PlayerController playerController = latestPlayer.GetComponent<PlayerController>();
        playerController.setIsLifting(true);
    }

    public void Drop()
    {
        gameObject.transform.parent = null;
        isLifted = false;
        canPickUp = false;
        PV.TransferOwnership(0);
        PlayerController playerController = latestPlayer.GetComponent<PlayerController>();
        playerController.setIsLifting(false);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DropDown"))
        {
            //droppedDeliveries.Add(gameObject);
            //gatheredPoints++;
            Destroy(gameObject);
            score.Change(1);
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
}
