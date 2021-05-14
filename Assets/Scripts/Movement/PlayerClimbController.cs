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

    Vector3 standScale;
    Vector3 crouchScale; 

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerLiftController = GetComponent<PlayerLiftController>();
        standScale = gameObject.transform.localScale;
        crouchScale = new Vector3(gameObject.transform.localScale.x-0.1f, gameObject.transform.localScale.y*0.5f, gameObject.transform.localScale.z-0.1f);
    }

    void Update()
    {
        if (!PV.IsMine) return;

        CheckCrouchAndStand();
        CheckClimb();
    }

    void CheckCrouchAndStand() 
    {        
        if (Input.GetKeyDown(PlayerController.crouchButton) && CanCrouch())
        {
            Crouch();
            return;
        }
        if (Input.GetKeyDown(PlayerController.crouchButton) && CanStand())
        {
            Stand();
        }
    }
    
    void CheckClimb()
    {
        if (!latestCollision) return;

        if (Input.GetKeyDown(PlayerController.useButton)) print(CanClimb());
        if (Input.GetKeyDown(PlayerController.useButton) && CanClimb() && CanClimbPlayer(latestCollision.GetComponent<PhotonView>().ViewID))
        {
            Climb();
            return;
        }
        if (Input.GetKeyDown(PlayerController.crouchButton) && CanClimbDown())
        {
            ClimbDown();
        }
    }

    void Crouch()
    {
        PV.RPC("OnCrouch", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void OnCrouch()
    {
        // Check that the game state hasn't changed
        if (!CanCrouch()) return;

        gameObject.transform.localScale = crouchScale;
        isCrouching = true;
        rb.isKinematic = true;
    }

    void Stand() 
    {
        PV.RPC("OnStand", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void OnStand()
    {
        // Check that the game state hasn't changed
        if (!CanStand()) return;

        gameObject.transform.localScale = standScale;
        isCrouching = false;
        if (PV.IsMine) rb.isKinematic = false;
    }

    void Climb()
    {
        PV.RPC("OnClimb", RpcTarget.AllViaServer, latestCollision.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnClimb(int crouchingPlayerID) 
    {
        PhotonView crouchingPV = PhotonView.Find(crouchingPlayerID);
        if (crouchingPV == null) return;
        PlayerClimbController crouchingPCC = crouchingPV.GetComponent<PlayerClimbController>();

        // Check that the game state hasn't changed
        if (!crouchingPCC.isCrouching || crouchingPCC.isClimbed || !CanClimb()) return;
        
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
        PV.RPC("OnClimbDown", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void OnClimbDown()
    {
        // Check that the game state hasn't changed
        if (!CanClimbDown()) return;
        
        // Remove parent and reset position
        gameObject.transform.parent = null;
        gameObject.transform.position = posPreClimb;
        if (PV.IsMine) rb.isKinematic = false;

        // Set booleans
        isClimbing = false;
        latestPlayerClimbed.isClimbed = false;
    }

    bool CanCrouch()
    {
        return !playerLiftController.IsLifting() && !isCrouching && !isClimbing && !isClimbed;
    }

    bool CanStand()
    {
        return !isClimbed && isCrouching && !playerLiftController.IsLifting() && !isClimbing;
    }

    bool CanClimb()
    {
        return !playerLiftController.IsLifting() && !isCrouching && !isClimbed && !isClimbing;
    }

    bool CanClimbDown()
    {
        return !isCrouching && !isClimbed && isClimbing;
    }

    bool CanClimbPlayer(int _canClimbID)
    {
        return canClimbID == _canClimbID;
    }
}