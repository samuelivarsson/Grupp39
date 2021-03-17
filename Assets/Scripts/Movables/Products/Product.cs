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
    [SerializeField] Transform productController;
    Transform hand;
    bool isLifted;

    bool canPickUp;
    Collision latestCollision;
    Collision boxCol;

    bool canDrop;
    bool isDroped;
    Transform pic;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        hand = player.GetChild(0);
        pic = productController.GetChild(0);
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

       /* if (Input.GetKeyDown(KeyCode.Space) && isDroped)
        {
            gameObject.transform.parent = null;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            rb.WakeUp();
            isDroped = false;
            canDrop = false;
        }*/

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

        if (canDrop)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                rb.Sleep();
                gameObject.transform.localPosition = pic.transform.localPosition;
                gameObject.transform.parent = boxCol.gameObject.transform;
                gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                isDroped = true;
            }
            
        }

        if (isDroped)
        {
            gameObject.transform.localPosition = pic.transform.localPosition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Box")
        {
            canDrop = true;
            boxCol = collision;
            //Destroy(gameObject);

            // gameObject.transform.parent = null;

            //rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            //rb.Sleep();

            //gameObject.transform.parent = collision.gameObject.transform;
            //gameObject.transform.localPosition = new Vector3(0, 1, 0);
            //gameObject.transform.SetParent(collision.gameObject.transform);

            //gameObject.transform.localScale = new Vector3(0.2f,0.2f,0.2f);
            //gameObject.transform.position = new Vector3(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y+0.5f, collision.gameObject.transform.position.z);
            //gameObject.transform.localPosition = new Vector3(0, 1, 0);

            //gameObject.transform.localPosition = collision.transform.localPosition;
            //gameObject.transform.position.Set()
            //Debug.Log("i lådan");



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

        /*if (collision.gameObject.tag == "Box")
        {
            canDrop = false;

        }*/
    }
}
