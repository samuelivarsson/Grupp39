using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    PhotonView PV;
    Rigidbody rb;
    [SerializeField] float movementSpeed, smoothTime;
    private float horizontalInput, verticalInput;

    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;
    Quaternion rotation = Quaternion.identity;

    PlayerClimbController playerClimbController;

    public static KeyCode useButton = KeyCode.Space;
    public static KeyCode tapeButton = KeyCode.E;
    public static KeyCode packButton = KeyCode.LeftShift;
    public static KeyCode crouchButton = KeyCode.LeftControl;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerClimbController = gameObject.GetComponent<PlayerClimbController>();
    }

    void Start()
    {
        if (!PV.IsMine) Destroy(rb);

        rb.centerOfMass = Vector3.zero;
    }

    void Update()
    {
        if (!PV.IsMine) return;
        
        if (!gameObject.GetComponent<PlayerPackController>().isTaping)
        {
            MoveAndRotate();
        }
    }

    void FixedUpdate()
    {
        if (!PV.IsMine) return;
        
        rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        rb.MoveRotation(rotation);
    }

    void MoveAndRotate()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);    

        if (playerClimbController.isCrouching) return;

        // Rotate
        if (moveDir == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(moveDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 15);
    }
}
