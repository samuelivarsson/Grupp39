using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    PhotonView PV;
    Rigidbody rb;
    [SerializeField] public float smoothTime {get; set;}
    [SerializeField] float rotateSpeed;
    float horizontalInput, verticalInput;

    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    Vector3 moveAmount;
    Quaternion rotation = Quaternion.identity;

    public bool isMultiLifting {get; set;} = false;
    public bool tooHeavy {get; set;} = false;
    public bool isHelper {get; set;} = false;
    public bool isGrounded {get; set;} = false;

    PlayerLiftController playerLiftController;
    PlayerPackController playerPackController;
    PlayerClimbController playerClimbController;
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
        playerLiftController = GetComponent<PlayerLiftController>();
        playerPackController = GetComponent<PlayerPackController>();
        playerClimbController = GetComponent<PlayerClimbController>();
        character = GetComponent<Character>();
    }

    void Start()
    {
        if (!PV.IsMine) rb.isKinematic = true;

        rb.centerOfMass = Vector3.zero;
    }

    void Update()
    {
        if (PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) return;
        
        if (playerPackController.isTaping || (tooHeavy && playerLiftController.liftingID != -1))
        {
            moveAmount = Vector3.zero;
            return;
        }
        MoveAndRotate();
    }

    void FixedUpdate()
    {
        if (!PV.IsMine) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f))
        {
            float dist = hit.distance - 0.5f;
            rb.MovePosition(new Vector3(transform.position.x, transform.position.y - dist, transform.position.z));
        }

        if (PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber && isMultiLifting)
        {
            rb.velocity = new Vector3(networkMoveAmount.x, rb.velocity.y, networkMoveAmount.z);
            RotateToPackage();
            return;
        }
        
        rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        
        if (isMultiLifting)
        {
            RotateToPackage();
            return;
        }
        
        rb.MoveRotation(rotation);
    }

    void MoveAndRotate()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * character.movementSpeed, ref smoothMoveVelocity, smoothTime);

        if (!PV.IsMine && PV.CreatorActorNr == PhotonNetwork.LocalPlayer.ActorNumber && moveAmount != networkMoveAmount)
        {
            networkMoveAmount = moveAmount;
            PV.RPC("OnMove", RpcTarget.OthersBuffered, moveAmount);
        }

        if (playerClimbController.isCrouching) return;

        // Rotate
        if (moveDir == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
    }

    void RotateToPackage()
    {
        Vector3 packagePos = playerLiftController.latestObject.transform.position;
        Vector3 target = new Vector3(packagePos.x, 0, packagePos.z);
        Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
        Quaternion qTo = Quaternion.LookRotation(target - current);
        rb.MoveRotation(qTo);
    }

    [PunRPC] 
    void OnMove(Vector3 moveAmount)
    {
        networkMoveAmount = moveAmount;
    }
}
