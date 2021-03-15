using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{

    //List<GameObject> droppedDeliveries = new List<GameObject>();
    int gatheredPoints = 0;
    bool jumpKeyWasPressed;

    Rigidbody rb;
    PhotonView PV;
    [SerializeField] GameObject hand = GameObject.FindWithTag("Hand");

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(rb);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpKeyWasPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpKeyWasPressed = false;
            gameObject.transform.parent = null;
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DropDown"))
        {
            //droppedDeliveries.Add(gameObject);
            gatheredPoints++;
            Destroy(gameObject);
            Debug.Log("Points:"+ gatheredPoints);
        }

        if(collision.gameObject.tag == "Player" && jumpKeyWasPressed)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.transform.position = hand.transform.position;
            gameObject.transform.parent = collision.gameObject.transform;
        }
    }

}
