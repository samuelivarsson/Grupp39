using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{

    private bool jumpKeyWasPressed;
    private bool isLifted;
    private float horizontalInput;
    private float verticalInput;
    private Rigidbody rigidbodyComponent;
    public GameObject box;
     

    // Start is called before the first frame update
    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
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
            isLifted = false;
        }
        if (isLifted) //Om den är lyft ska man sätta dess koordinater till samma som player
        {
            box.transform.position = new Vector3(gameObject.transform.position.x + 0.25f, gameObject.transform.position.y +1.5f, gameObject.transform.position.z);
              
        }
        else
        {
            //detacha barnen 
            gameObject.transform.DetachChildren();
            
        }
      

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {

        rigidbodyComponent.velocity = new Vector3(horizontalInput * 3, rigidbodyComponent.velocity.y, rigidbodyComponent.velocity.z);
        rigidbodyComponent.velocity = new Vector3(rigidbodyComponent.velocity.x, rigidbodyComponent.velocity.y, verticalInput * 3);
    }

    private void OnCollisionEnter(Collision collision)
    {
       
        if (collision.gameObject.tag == "Box" && jumpKeyWasPressed)
        {
            collision.gameObject.transform.parent = gameObject.transform;
            collision.gameObject.transform.position= gameObject.transform.position;
            isLifted = true;
        }
    }

}
