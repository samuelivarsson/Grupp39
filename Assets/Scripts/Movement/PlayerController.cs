using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float movementSpeed, smoothTime;
    private float horizontalInput, verticalInput;
    public bool isLifting;

    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;

    PhotonView PV;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        isLifting = false;
    }

    void Start()
    {
        if (!PV.IsMine) Destroy(rb);
    }

    void Update()
    {
        if (!PV.IsMine) return;

        move();
    }

    private void move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;

        if (moveAmount != Vector3.zero) rb.MovePosition(rb.position + moveAmount * Time.fixedDeltaTime);
        if (moveDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDir);
    }

    public void setIsLifting(bool _isLifting)
    {
        isLifting = _isLifting;
    }

    //public Transform playerIsLifting(bool _isLifting) 
    //{
    //    isLifting = _isLifting;
    //    return this.transform;
    //}
}
