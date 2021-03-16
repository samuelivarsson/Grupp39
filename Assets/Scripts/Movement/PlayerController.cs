using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private float horizontalInputRot;
    private float verticalInputRot;
    [SerializeField] int movementSpeed;

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
        if (!PV.IsMine) return;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        horizontalInputRot = Input.GetAxisRaw("Horizontal");
        verticalInputRot = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;
        move();
        rotate();
    }

    private void move()
    {
        Vector3 movement = new Vector3(horizontalInput * movementSpeed, rb.velocity.y, verticalInput * movementSpeed);
        rb.velocity = movement;
    }

    private void rotate()
    {
        Vector3 dir = new Vector3(horizontalInputRot, 0.0f, verticalInputRot);
        if (dir != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
