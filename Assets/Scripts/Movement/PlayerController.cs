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
    PlayerPackController playerPackController;

    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;

    public static KeyCode useButton = KeyCode.Space;
    public static KeyCode tapeButton = KeyCode.E;
    public static KeyCode packButton = KeyCode.LeftShift;
    public static KeyCode crouchButton = KeyCode.LeftControl;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerPackController = PlayerManager.myPlayerPackController;
    }

    void Start()
    {
        if (!PV.IsMine) Destroy(rb);
    }

    void Update()
    {
        if (!PV.IsMine) return;

        if (!gameObject.GetComponent<PlayerPackController>().isTaping)
        {
            Move();
        }
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;

        if (moveAmount != Vector3.zero) rb.MovePosition(rb.position + moveAmount * Time.fixedDeltaTime);
        if (moveDir != Vector3.zero) rb.MoveRotation(Quaternion.LookRotation(moveDir));
    }

    private void Move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * movementSpeed, ref smoothMoveVelocity, smoothTime);
    }
}
