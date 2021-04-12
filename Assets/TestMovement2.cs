using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement2 : MonoBehaviour
{
    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;
    Quaternion rotation = Quaternion.identity, rotation2 = Quaternion.identity;
    float smoothTime = 0.05f;
    float horizontalAngle, verticalAngle, horizontalAngle2, verticalAngle2;
    float horizontalInput, verticalInput, horizontalInput2, verticalInput2;
    float movementSpeed = 4;
    float rotateSpeed = 50;

    Rigidbody rb;
    GameObject player1, player2, package;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player1 = gameObject;
        player2 = transform.GetChild(1).gameObject;
        package = transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        Move();
        Rotate();
    }

    void FixedUpdate()
    {
        // Move
        transform.Translate(moveAmount * Time.fixedDeltaTime, Space.World);
        player2.transform.localPosition = new Vector3(package.transform.localPosition.x, player2.transform.localPosition.y, package.transform.localPosition.z + 1.5f);
        // rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);

        // Rotate
        if (horizontalInput != horizontalInput2)
        {
            transform.RotateAround(package.transform.position, -Vector3.up, horizontalAngle * rotateSpeed * Time.fixedDeltaTime);
            transform.RotateAround(package.transform.position, -Vector3.up, horizontalAngle2 * rotateSpeed * Time.fixedDeltaTime);
        }
        if (verticalInput != verticalInput2)
        {
            transform.RotateAround(package.transform.position, Vector3.up, verticalAngle * rotateSpeed * Time.fixedDeltaTime);
            transform.RotateAround(package.transform.position, Vector3.up, verticalAngle2 * rotateSpeed * Time.fixedDeltaTime);
        }
    }

    void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput2 = Input.GetAxisRaw("Horizontal2");
        verticalInput2 = Input.GetAxisRaw("Vertical2");
    }

    void Move()
    {
        moveDir = new Vector3(horizontalInput+horizontalInput2, 0, verticalInput+verticalInput2).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);
    }

    void Rotate()
    {
        horizontalAngle = CalcRotAngle(player1, horizontalInput, InputType.horizontal);
        verticalAngle = CalcRotAngle(player1, verticalInput, InputType.vertical);
        horizontalAngle2 = CalcRotAngle(player2, horizontalInput2, InputType.horizontal);
        verticalAngle2 = CalcRotAngle(player2, verticalInput2, InputType.vertical);
        print(player1.transform.forward);
    }

    float CalcRotAngle(GameObject player, float input, InputType inputType)
    {
        Vector3 fwd = player.transform.forward;
        float dir;

        switch (inputType)
        {
            case InputType.horizontal:
            {
                dir = fwd.z;
                break;
            }
            case InputType.vertical:
            {
                dir = fwd.x;
                break;
            }
            default:
            {
                dir = 0;
                break;
            }
        }
        
        return input * dir;
    }

    enum InputType
    {
        horizontal,
        vertical
    }
}
