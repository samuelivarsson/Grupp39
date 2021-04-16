﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    PhotonView PV;
    Rigidbody rb;
    [SerializeField] GameObject myPlayerIcon;
    [SerializeField] public float smoothTime;
    [SerializeField] public float rotateSpeed;
    float horizontalInput, verticalInput;

    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;
    Quaternion rotation;

    PlayerLiftController playerLC;
    PlayerPackController playerPC;
    PlayerClimbController playerCC;
    PlayerMultiLiftController playerMLC;
    Character character;

    public static KeyCode useButton = KeyCode.Space;
    public static KeyCode tapeButton = KeyCode.E;
    public static KeyCode packButton = KeyCode.LeftShift;
    public static KeyCode crouchButton = KeyCode.LeftControl;

    Vector3 networkMoveAmount = Vector3.zero;
    Quaternion networkRotation = Quaternion.identity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        myPlayerIcon.SetActive(PV.CreatorActorNr == PhotonNetwork.LocalPlayer.ActorNumber);
        playerLC = GetComponent<PlayerLiftController>();
        playerPC = GetComponent<PlayerPackController>();
        playerCC = GetComponent<PlayerClimbController>();
        playerMLC = GetComponent<PlayerMultiLiftController>();
        character = GetComponent<Character>();
    }

    void Start()
    {
        if (!PV.IsMine) rb.isKinematic = true;

        rb.centerOfMass = Vector3.zero;
    }

    void Update()
    {
        if (!PV.IsMine) return;
        
        if (playerPC.isTaping)
        {
            moveAmount = Vector3.zero;
            return;
        }
        Inputs();
        Move();
        Rotate();
    }

    void FixedUpdate()
    {
        if (!PV.IsMine) return;
        
        if (playerMLC.isMultiLifting) return;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f))
        {
            float dist = hit.distance - 0.5f;
            rb.MovePosition(new Vector3(transform.position.x, transform.position.y - dist, transform.position.z));
        }
        rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        rb.MoveRotation(rotation);
    }

    void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
    }

    void Move()
    {
        if (playerMLC.tooHeavy && playerLC.liftingID != -1)
        {
            moveAmount = Vector3.zero;
            return;
        }
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * character.movementSpeed, ref smoothMoveVelocity, smoothTime);
    }

    void Rotate()
    {
        if (playerCC.isCrouching || moveDir == Vector3.zero)
        {
            rotation = rb.rotation;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
    }
}
