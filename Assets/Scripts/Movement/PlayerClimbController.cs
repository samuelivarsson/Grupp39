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
    Vector3 yPosChange;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerLiftController = GetComponent<PlayerLiftController>();
        heightChange = new Vector3(-0.1f, gameObject.transform.localScale.y*0.5f, -0.1f);
        yPosChange = new Vector3(0, gameObject.transform.localScale.y*0.5f, 0);
    }

    void Update()
    {
        if (!PV.IsMine) return;

        CheckCrouchAndStand();
        CheckClimb();
    }

    void CheckCrouchAndStand() 
    {        
        if (Input.GetKeyDown(PlayerController.crouchButton) && !isCrouching && playerLiftController.IsLifting(-1) && !isClimbing)
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
        gameObject.transform.localPosition -= yPosChange;
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
        gameObject.transform.localPosition += yPosChange;
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
        Vector3 halfOfHeight = new Vector3(0, GetComponent<CapsuleCollider>().height*gameObject.transform.localScale.y/2, 0);
        gameObject.transform.localPosition = latestPlayerClimbed.head.transform.localPosition + halfOfHeight;

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

    // public void ActivateHeadJoint(Rigidbody conBody)
    // {
    //     HingeJoint hingeJoint = head.GetComponent<HingeJoint>();        
    //     SetHingeJoint(hingeJoint, conBody);
    //     head.SetActive(true);
    // }

    // public void DeactivateHeadJoint()
    // {
    //     HingeJoint hingeJoint = head.GetComponent<HingeJoint>();        
    //     SetHingeJoint(hingeJoint, null);
    //     head.SetActive(false);
    // }

    // void SetHingeJoint(HingeJoint hingeJoint, Rigidbody conBody)
    // {
    //     hingeJoint.anchor = head.transform.localPosition;
    //     hingeJoint.autoConfigureConnectedAnchor = false;
    //     hingeJoint.connectedAnchor = Vector3.zero;
    //     hingeJoint.connectedBody = conBody;
    // }
}