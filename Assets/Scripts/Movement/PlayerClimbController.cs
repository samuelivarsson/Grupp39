using UnityEngine;
using Photon.Pun;

public class PlayerClimbController : MonoBehaviour
{
    // View ID of a player this player can climb right now.
    public int canClimbID {get; set;} = -1;

    // Is this player crouching?
    public bool isCrouching {get; set;} = false;

    // Is this player climbing?
    public bool isClimbing {get; set;} = false;

    // Does this player have another player on him?
    public bool isClimbed {get; set;} = false;

    // Position before climbing
    Vector3 posPreClimb;

    // Latest player this player could climb.
    public GameObject latestCollision {get; set;} 
    
    // Latest player this player has climbed upon.
    PlayerClimbController latestPlayerClimbed;

    [SerializeField] GameObject head;
    
    PlayerLiftController playerLiftController;
    PhotonView PV;
    Rigidbody rb;

    Vector3 heightChange; 

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerLiftController = GetComponent<PlayerLiftController>();
        heightChange = new Vector3(-0.1f, gameObject.transform.localScale.y*0.5f, -0.1f);
    }

    void Update()
    {
        if (!PV.IsMine) return;

        CheckCrouchAndStand();
        CheckClimb();
    }

    void CheckCrouchAndStand() 
    {        
        if (Input.GetKeyDown(PlayerController.crouchButton) && !isCrouching && !playerLiftController.IsLifting() && !isClimbing)
        {
            Crouch();
            return;
        }
        if (Input.GetKeyDown(PlayerController.crouchButton) && isCrouching)
        {
            Stand();
        }
    }
    
    void CheckClimb()
    {
        if (!latestCollision) return;

        if (Input.GetKeyDown(PlayerController.useButton) && !isClimbing && CanClimb(latestCollision.GetComponent<PhotonView>().ViewID))
        {
            Climb();
            return;
        }
        if (Input.GetKeyDown(PlayerController.crouchButton) && isClimbing)
        {
            ClimbDown();
        }
    }

    void Crouch()
    {
        PV.RPC("OnCrouch", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    void OnCrouch()
    {
        gameObject.transform.localScale -= heightChange;
        isCrouching = true;
        rb.isKinematic = true;
    }

    void Stand() 
    {
        PV.RPC("OnStand", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    void OnStand()
    {
        if (isClimbed) return;

        gameObject.transform.localScale += heightChange;
        isCrouching = false;
        if (PV.IsMine) rb.isKinematic = false;
    }

    void Climb()
    {
        PV.RPC("OnClimb", RpcTarget.AllBufferedViaServer, latestCollision.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnClimb(int crouchingPlayerID) 
    {
        PlayerClimbController crouchingPCC = PhotonView.Find(crouchingPlayerID).GetComponent<PlayerClimbController>();
        if(!crouchingPCC.isCrouching || crouchingPCC.isClimbed) return;
        
        // Save climbed player and position
        latestPlayerClimbed = crouchingPCC;
        posPreClimb = gameObject.transform.position;

        // Set parent and new position
        gameObject.transform.parent = latestPlayerClimbed.transform;
        gameObject.transform.localPosition = latestPlayerClimbed.head.transform.localPosition;

        // Set booleans
        rb.isKinematic = true;
        crouchingPCC.isClimbed = true;
        isClimbing = true;
    }

    void ClimbDown()
    {
        PV.RPC("OnClimbDown", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    void OnClimbDown()
    {
        // Remove parent and reset position
        gameObject.transform.parent = null;
        gameObject.transform.position = posPreClimb;
        if (PV.IsMine) rb.isKinematic = false;

        // Set booleans
        isClimbing = false;
        latestPlayerClimbed.isClimbed = false;
    }

    bool CanClimb(int _canClimbID)
    {
        return canClimbID == _canClimbID;
    }
}