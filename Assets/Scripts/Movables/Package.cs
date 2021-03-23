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
    [SerializeField] Transform package;
    Transform pic1;
    bool isLifted;
    

    bool canPickUp;
    Transform latestPlayer;
    private bool canPackage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        hand = player.GetChild(0);
        pic1 = package.GetChild(0);
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
        
        if (canPackage)
        {
            
            if (Input.GetKeyDown(KeyCode.B) && latestPlayer.GetChild(3))
            {
                Transform prod = latestPlayer.GetChild(3).transform;
                latestPlayer.GetChild(3).transform.parent = null;
                prod.parent = gameObject.transform;
                prod.localPosition = pic1.transform.localPosition;
                prod.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            }
            
        }
        if (Input.GetKeyDown(KeyCode.T) && canPickUp)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }


    }

    private void CheckLiftAndDrop()
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
        gameObject.transform.GetChild(1).localPosition= pic1.transform.localPosition;
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

    public void setLifted(bool _canPackage)
    {
        canPackage = _canPackage;
    }
}
