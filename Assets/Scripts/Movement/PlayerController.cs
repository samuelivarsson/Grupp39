using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    PhotonView PV;
    Rigidbody rb;
    [SerializeField] public float smoothTime;
    [SerializeField] public float rotateSpeed;
    float horizontalInput, verticalInput;

    Vector3 smoothMoveVelocity;
    Vector3 moveDir;
    public Vector3 rotateDir {get; set;} = Vector3.zero;
    Vector3 moveAmount;
    public Quaternion rotation {get; set;} 

    PlayerLiftController playerLC;
    PlayerPackController playerPC;
    PlayerClimbController playerCC;
    PlayerMultiLiftController playerMLC;
    Character character;
    Animator anim;

    public static KeyCode useButton = KeyCode.Space;
    public static KeyCode crouchButton = KeyCode.Z;
    public static KeyCode packButton = KeyCode.X;
    public static KeyCode tapeButton = KeyCode.C;

    Vector3 networkMoveAmount = Vector3.zero;
    Quaternion networkRotation = Quaternion.identity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerLC = GetComponent<PlayerLiftController>();
        playerPC = GetComponent<PlayerPackController>();
        playerCC = GetComponent<PlayerClimbController>();
        playerMLC = GetComponent<PlayerMultiLiftController>();
        character = GetComponent<Character>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (!PV.IsMine) rb.isKinematic = true;

        rb.centerOfMass = Vector3.zero;
        rotation = rb.rotation;
    }

    void Update()
    {
        bool gameStarted = (bool) PhotonNetwork.CurrentRoom.CustomProperties["gameStarted"];
        if (!PV.IsMine || TaskManager.Instance == null || !gameStarted) return;
        SetCondition();
        if (playerPC.isTaping || playerCC.isCrouching)
        {
            moveAmount = Vector3.zero;
            moveDir = Vector3.zero;
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
        
        rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        rb.MoveRotation(rotation);
    }

    void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
        rotateDir = (moveDir != Vector3.zero) ? moveDir : rotateDir;
    }

    void Move()
    {
        if (playerMLC.tooHeavy && !playerLC.IsLifting(-1))
        {
            moveAmount = Vector3.zero;
            moveDir = Vector3.zero;
            PopupInfo.Instance.Popup("Lådan är för tung att lyfta själv", 7);
            return;
        }
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * character.movementSpeed, ref smoothMoveVelocity, smoothTime);
    }

    void Rotate()
    {
        if (playerCC.isCrouching || rotateDir == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(rotateDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
    }

    void SetCondition()
    {
        int condition;
        if (playerLC.IsLifting(-1)) condition = (moveDir == Vector3.zero || playerCC.isClimbing) ? 0 : 1;
        else condition = (moveDir == Vector3.zero || playerCC.isClimbing) ? 2 : 3;
        anim.SetInteger("condition", condition);
    }
}
