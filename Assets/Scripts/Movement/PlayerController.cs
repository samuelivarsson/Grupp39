using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool jumpKeyWasPressed;
    [SerializeField] bool isLifted;
    [SerializeField] GameObject box;
    [SerializeField] Transform hand;

    PhotonView PV;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(rb);
        }
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpKeyWasPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpKeyWasPressed = false;
            isLifted = false;
            box.transform.parent = null;
            box.GetComponent<Rigidbody>().useGravity = true;
        }*/

        if (!PV.IsMine) return;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        /*if (isLifted) //Om den är lyft ska man sätta dess koordinater till samma som player
        {
            //box.GetComponent<Rigidbody>().MovePosition(new Vector3(4f, 4f, 4f));
            box.transform.localPosition = new Vector3(4f, 4f, 4f);
        }
        else
        {
            //detacha barnen 
            gameObject.transform.DetachChildren();
        }*/

        if (!PV.IsMine) return;
        rb.velocity = new Vector3(horizontalInput * 4, rb.velocity.y, verticalInput * 4);
        //rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, verticalInput * 4);

       
    }
    /*private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Box" && jumpKeyWasPressed)
        {
            /*collision.gameObject.transform.parent = gameObject.transform;
            collision.gameObject.transform.position = gameObject.transform.position;
            collision.rigidbody.useGravity = false;
            isLifted = true;*/

            /*collision.rigidbody.useGravity = false;
            collision.gameObject.transform.position = hand.position;
            collision.gameObject.transform.parent = gameObject.transform;
        }
    }*/
}
