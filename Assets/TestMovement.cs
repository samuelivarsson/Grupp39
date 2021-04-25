using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;
    Quaternion rotation = Quaternion.identity;
    float smoothTime = 0.05f;
    float horizontalInput, verticalInput;
    float movementSpeed = 3;
    float rotateSpeed = 15;

    [SerializeField] GameObject head2;
    [SerializeField] Rigidbody player2;

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
        if (Input.GetKeyDown(KeyCode.Space)) Space();
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
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);

        // Rotate
        if (moveDir == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(moveDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
    }

    void Space()
    {
        player2.isKinematic = false;
        HingeJoint hingeJoint = head2.GetComponent<HingeJoint>();
        SetHingeJoint(hingeJoint, rb);
        head2.SetActive(true);
    }

    void SetHingeJoint(HingeJoint hingeJoint, Rigidbody conBody)
    {
        hingeJoint.anchor = new Vector3(0, 3, 0);
        hingeJoint.autoConfigureConnectedAnchor = false;
        hingeJoint.connectedAnchor = Vector3.zero;
        hingeJoint.connectedBody = conBody;
    }
}
