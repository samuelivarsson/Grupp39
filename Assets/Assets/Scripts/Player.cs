using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private bool jumpKeyWasPressed;
    private float horizontalInput;
    private float verticalInput;
    private Rigidbody rigidbodyComponent;

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

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (jumpKeyWasPressed)
        {
            rigidbodyComponent.AddForce(Vector3.up * 5, ForceMode.VelocityChange);
            jumpKeyWasPressed = false;
        }

        rigidbodyComponent.velocity = new Vector3(horizontalInput * 3, rigidbodyComponent.velocity.y, rigidbodyComponent.velocity.z);
        rigidbodyComponent.velocity = new Vector3(rigidbodyComponent.velocity.x, rigidbodyComponent.velocity.y, verticalInput * 3);
    }
}
