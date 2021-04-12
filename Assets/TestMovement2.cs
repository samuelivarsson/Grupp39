using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement2 : MonoBehaviour
{
    Vector3 smoothMoveVelocity, smoothMoveVelocity2;
    Vector3 moveDir, moveDir2;
    Vector3 moveAmount, moveAmount2;
    Quaternion rotation = Quaternion.identity, rotation2 = Quaternion.identity;
    float smoothTime = 0.05f;
    float horizontalInput, verticalInput, horizontalInput2, verticalInput2;
    float movementSpeed = 4;
    float rotateSpeed = 15;

    Rigidbody rb;
    GameObject player1, player2;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player1 = transform.GetChild(1).gameObject;
        player2 = transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void FixedUpdate()
    {
        //transform.RotateAround(player1.transform.position, Vector3.up, 20 * Time.fixedDeltaTime);
        //transform.Translate(Vector3.forward * Time.fixedDeltaTime, player1.transform);
        Vector3 point = player1.transform.position;
        Quaternion rotation = Quaternion.LookRotation(
        gameObject.transform.position - point,
        gameObject.transform.rotation * Vector3.up);
    }

    void Move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput2 = Input.GetAxisRaw("Horizontal2");
        verticalInput2 = Input.GetAxisRaw("Vertical2");

        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
        moveDir2 = new Vector3(horizontalInput2, 0, verticalInput2).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);
        moveAmount2 = Vector3.SmoothDamp(moveAmount2, moveDir2 * movementSpeed, ref smoothMoveVelocity2, smoothTime);

        // Rotate
        // if (moveDir != Vector3.zero)
        // {
        //     var targetRotation = Quaternion.LookRotation(moveDir);
        //     rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
        // }
        // if (moveDir2 != Vector3.zero)
        // {
        //     var targetRotation = Quaternion.LookRotation(moveDir);
        //     rotation2 = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
        // }
    }
}
