using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;

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
        if(!PV.IsMine) return;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if(!PV.IsMine) return;
        rb.velocity = new Vector3(horizontalInput * 4, rb.velocity.y, rb.velocity.z);
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, verticalInput * 4);
    }
}
