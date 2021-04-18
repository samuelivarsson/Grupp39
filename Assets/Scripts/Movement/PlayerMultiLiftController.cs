using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMultiLiftController : MonoBehaviour
{
    Rigidbody rb;
    PhotonView PV;

    PlayerController playerC;
    PlayerLiftController playerLC;
    Character character;

    public bool isMultiLifting {get; set;} = false;
    public bool tooHeavy {get; set;} = false;
    public bool iAmLifting {get; set;} = false;

    float horizontalInput, verticalInput;
    Vector3 moveDir = Vector3.zero;
    Vector3 moveAmount = Vector3.zero;
    Vector3 smoothMoveVelocity;
    Quaternion rotation = Quaternion.identity;

    Vector3 networkVelocity = Vector3.zero;
    Vector3 networkPosition;

    const float farAway = 1f; // Fastest movement speed = 6u/s, Maximum lag = 400ms = 0.4s --> far away = 6/0.4 = 2.4
    const float veryClose = 0.001f;
    const int interpolationTime = 200; // How much time the client has to interpolate the position (milliseconds) OLD
    const float interpolationSpeed = 20f;
    Vector3 interpolationStartPosition;
    int interpolationStartTime; // OLD
    bool interpolating = false;

    int updateFreq = 20; // Interval in milliseconds of how often RPCs are executed
    int latestServerSend;
    int latestClientSend;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerC = GetComponent<PlayerController>();
        playerLC = GetComponent<PlayerLiftController>();
        character = GetComponent<Character>();
        
        latestServerSend = latestClientSend = PhotonNetwork.ServerTimestamp;
    }
    
    void Update()
    {
        if (!isMultiLifting /*|| (!PV.IsMine && PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber)*/) return;

        // We are multi lifting and we are the owner or the creator (or both)
        Inputs();
        Move();
        RotateToPackage();
    }

    void FixedUpdate()
    {
        if (!isMultiLifting || !iAmLifting) return;

        // We are multi lifting and we are the owner or the creator (or both)
        if (PV.IsMine)
        {
            // Owner
            if (PV.CreatorActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // Owner and creator
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f))
                {
                    float dist = hit.distance - 0.5f;
                    rb.MovePosition(new Vector3(transform.position.x, transform.position.y - dist, transform.position.z));
                }

                rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
                rb.MoveRotation(rotation);
                if (PhotonNetwork.ServerTimestamp - latestServerSend > updateFreq)
                {
                    PV.RPC("OnSend", RpcTarget.OthersBuffered, rb.velocity, rb.position);
                    latestServerSend = PhotonNetwork.ServerTimestamp;
                }
            }
            else
            {
                // Owner but not creator
                if (interpolating)
                {
                    float alpha = Time.fixedDeltaTime * interpolationSpeed;
                    rb.position = Vector3.Lerp(interpolationStartPosition, networkPosition, alpha);
                    if (Vector3.Distance(rb.position, networkPosition) < veryClose)
                    {
                        // Interpolation is finished
                        rb.position = networkPosition;
                        interpolating = false;
                        print("6. Done interpolating");
                    }
                }
                rb.velocity = networkVelocity;
                rb.MoveRotation(rotation);
            }
        }
        else
        {
            // Not owner
            if (PV.CreatorActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // Not owner but creator
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f))
                {
                    float dist = hit.distance - 0.5f;
                    rb.MovePosition(new Vector3(transform.position.x, transform.position.y - dist, transform.position.z));
                }

                rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
                rb.MoveRotation(rotation);
                if (PhotonNetwork.ServerTimestamp - latestClientSend > updateFreq)
                {
                    PV.RPC("OnSend", RpcTarget.OthersBuffered, rb.velocity, rb.position);
                    latestClientSend = PhotonNetwork.ServerTimestamp;
                }
            }
            else
            {
                // Not owner and not creator
                if (interpolating)
                {
                    float alpha = Time.fixedDeltaTime * interpolationSpeed;
                    rb.position = Vector3.Lerp(interpolationStartPosition, networkPosition, alpha);
                    if (Vector3.Distance(rb.position, networkPosition) < veryClose)
                    {
                        // Interpolation is finished
                        rb.position = networkPosition;
                        interpolating = false;
                    }
                }
                rb.velocity = networkVelocity;
                rb.MoveRotation(rotation);
            }
        }
    }

    void Inputs()
    {
        if (PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
    }

    void Move()
    {
        if (tooHeavy && playerLC.liftingID != -1)
        {
            moveAmount = Vector3.zero;
            return;
        }
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * character.movementSpeed, ref smoothMoveVelocity, playerC.smoothTime);
    }

    void RotateToPackage()
    {
        if (PlayerManager.myPlayerLiftController.latestObject == null) return;

        Vector3 packagePos = PlayerManager.myPlayerLiftController.latestObject.transform.position;
        Vector3 target = new Vector3(packagePos.x, 0, packagePos.z);
        Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
        rotation = Quaternion.LookRotation(target - current);
    }

    [PunRPC]
    void OnSend(Vector3 _velocity, Vector3 _pos)
    {
        if (!iAmLifting) return;

        networkVelocity = _velocity;
        networkPosition = _pos;
        float distance = Vector3.Distance(_pos, rb.position);
        if (distance > farAway)
        {
            // The difference is too big -> teleport to correct
            rb.MovePosition(networkPosition);
        }
        else if (distance > veryClose)
        {
            // It is close to the correct clue but not close enough -> interpolate
            interpolationStartPosition = rb.position;
            interpolating = true;
        }
        else interpolating = false; // It is VERY close -> don't move it
    }
}
