using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{

    //List<GameObject> droppedDeliveries = new List<GameObject>();


    Rigidbody rb;
    PhotonView PV;
    [SerializeField] Transform player;
    Transform hand;
    bool isLifted;

    bool canPickUp;
    Collision latestCollision;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        hand = player.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        /*if (!PV.IsMine)
        {
            Destroy(rb);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space) && isLifted)
        {
            gameObject.transform.parent = null;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            rb.WakeUp();
            isLifted = false;
            canPickUp = false;
        }
        if (canPickUp)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                rb.Sleep();
                gameObject.transform.localPosition = hand.transform.localPosition;
                gameObject.transform.parent = latestCollision.gameObject.transform;
                isLifted = true;
            }
        }
        
        if (isLifted)
        {
            gameObject.transform.localPosition = hand.transform.localPosition;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Box"))
        {
            //droppedDeliveries.Add(gameObject);
            //gatheredPoints++;
            Destroy(gameObject);
            Debug.Log("i lådan");
        }

        if (collision.gameObject.tag == "Player")
        {
            canPickUp = true;
            latestCollision = collision;
           
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            canPickUp = false;
            
        }
    }
}
