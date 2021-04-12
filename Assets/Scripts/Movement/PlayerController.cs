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

    [SerializeField] float multiLiftRotateSpeed;
    public float networkHorizontalInput {get; set;} = 0;
    public float networkVerticalInput {get; set;} = 0;
    float latestNWHInput = 0, latestNWVInput = 0;
    float horizontalAngle = 0, verticalAngle = 0;
    float networkHorizontalAngle = 0, networkVerticalAngle = 0;
    public GameObject player2 {get; set;}
    public bool isMultiLifting {get; set;} = false;
    public bool tooHeavy {get; set;} = false;
    public bool isHelper {get; set;} = false;

    PlayerLiftController playerLiftController;
    PlayerPackController playerPackController;
    PlayerClimbController playerClimbController;
    Character character;

    public static KeyCode useButton = KeyCode.Space;
    public static KeyCode tapeButton = KeyCode.E;
    public static KeyCode packButton = KeyCode.LeftShift;
    public static KeyCode crouchButton = KeyCode.LeftControl;

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
        if (!PV.IsMine) return;
        
        if (playerPackController.isTaping || (tooHeavy && playerLiftController.liftingID != -1))
        {
            moveAmount = Vector3.zero;
            return;
        }
        Inputs();
        if (isMultiLifting && player2 != null)
        {
            PlayerController pc2 = player2.GetComponent<PlayerController>();
            MultiLiftMove(horizontalInput, verticalInput, pc2.networkHorizontalInput, pc2.networkVerticalInput);
            MultiLiftRotate(horizontalInput, verticalInput, pc2.networkHorizontalInput, pc2.networkVerticalInput);
            return;
        }
        MoveAndRotate(horizontalInput, verticalInput, 0, 0);
    }

    void FixedUpdate()
    {
        if (!PV.IsMine) return;

        // RaycastHit hit;
        // if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f))
        // {
        //     float dist = hit.distance - 0.5f;
        //     rb.MovePosition(new Vector3(transform.position.x, transform.position.y - dist, transform.position.z));
        // }
        
        if (isMultiLifting && player2 != null)
        {
            GameObject package = playerLiftController.latestObject;
            transform.Translate(moveAmount * Time.fixedDeltaTime, Space.World);
            player2.transform.localPosition = new Vector3(package.transform.localPosition.x, player2.transform.localPosition.y, package.transform.localPosition.z + 1.5f);
            PlayerController pc2 = player2.GetComponent<PlayerController>();
            // rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);

            // Rotate
            if (horizontalInput != pc2.networkHorizontalInput)
            {
                print("rotating hori");
                transform.RotateAround(package.transform.position, -Vector3.up, horizontalAngle * multiLiftRotateSpeed * Time.fixedDeltaTime);
                transform.RotateAround(package.transform.position, -Vector3.up, networkHorizontalAngle * multiLiftRotateSpeed * Time.fixedDeltaTime);
            }
            if (verticalInput != pc2.networkVerticalInput)
            {
                print("rotating vert: " + networkVerticalAngle);
                transform.RotateAround(package.transform.position, Vector3.up, verticalAngle * multiLiftRotateSpeed * Time.fixedDeltaTime);
                transform.RotateAround(package.transform.position, Vector3.up, networkVerticalAngle * multiLiftRotateSpeed * Time.fixedDeltaTime);
            }
            return;
        }

        rb.velocity = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        rb.MoveRotation(rotation);
    }

    void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void MoveAndRotate(float hInput, float vInput, float nwHInput, float nwVInput)
    {
        moveDir = new Vector3(hInput+nwHInput, 0, vInput+nwVInput).normalized;

        // Move
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * character.movementSpeed, ref smoothMoveVelocity, smoothTime);

        if (playerClimbController.isCrouching) return;

        // Rotate
        if (moveDir == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
    }

    void MultiLiftMove(float hInput, float vInput, float nwHInput, float nwVInput)
    {
        MoveAndRotate(hInput, vInput, nwHInput, nwVInput);
        if (latestNWHInput != horizontalInput)
        {
            PV.RPC("OnHorizontalInput", RpcTarget.OthersBuffered, horizontalInput);
            print("sending hori: " + horizontalInput);
            latestNWHInput = horizontalInput;
        }
        if (latestNWVInput != verticalInput)
        {
            PV.RPC("OnVerticalInput", RpcTarget.OthersBuffered, verticalInput);
            print("sending vert: " + verticalInput);
            latestNWVInput = verticalInput;
        }
    }

    void MultiLiftRotate(float hInput, float vInput, float nwHInput, float nwVInput)
    {
        horizontalAngle = CalcRotAngle(gameObject, hInput, InputType.horizontal);
        verticalAngle = CalcRotAngle(gameObject, vInput, InputType.vertical);
        networkHorizontalAngle = CalcRotAngle(player2, nwHInput, InputType.horizontal);
        networkVerticalAngle = CalcRotAngle(player2, nwVInput, InputType.vertical);
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
    void OnHorizontalInput(float hInput)
    {
        networkHorizontalInput = hInput;
        print("got hori: " + hInput);
    }

    [PunRPC] 
    void OnVerticalInput(float vInput)
    {
        networkVerticalInput = vInput;
        print("got vert: " + vInput);
        print("setting networkVInput: " + networkVerticalInput);
    }

    float CalcRotAngle(GameObject player, float input, InputType inputType)
    {
        if (player == null) return 0;
        Vector3 fwd = player.transform.forward;
        float dir;

        switch (inputType)
        {
            case InputType.horizontal:
            {
                dir = fwd.z;
                break;
            }
            case InputType.vertical:
            {
                dir = fwd.x;
                break;
            }
            default:
            {
                dir = 0;
                break;
            }
        }
        
        return input * dir;
    }

    enum InputType
    {
        horizontal,
        vertical
    }
}
