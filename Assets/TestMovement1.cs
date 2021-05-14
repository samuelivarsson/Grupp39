using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement1 : MonoBehaviour
{
    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;
    Quaternion rotation = Quaternion.identity;
    float smoothTime = 0.05f;
    float horizontalInput, verticalInput;
    float movementSpeed = 4;
    float rotateSpeed = 15;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void FixedUpdate()
    {
        // RaycastHit hit;
        // // print("1: "+transform.position);
        // if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f))
        // {
        //     // print("2: "+hit.distance);
        //     Vector3 dir = Vector3.ProjectOnPlane(Vector3.down*(hit.distance - 1f), hit.normal);
        //     // print("3: "+dir);
        //     rb.MovePosition(new Vector3(transform.position.x, transform.position.y-(hit.distance - 1f), transform.position.z));
        // }
        rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        //rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        rb.MoveRotation(rotation);
    }

    void Move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal2");
        verticalInput = Input.GetAxisRaw("Vertical2");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);

        // Rotate
        if (moveDir == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(moveDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
    }
}
