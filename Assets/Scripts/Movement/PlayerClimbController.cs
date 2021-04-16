using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerClimbController : MonoBehaviour
{
    public int canClimbID {get; set;} = -1;
    public bool isCrouching {get; set;} = false;
    public bool isClimbing {get; set;} = false;     //när jag klättrat upp på någon
    public bool isClimbed {get; set;} = false;      //när nån klättrat upp på mig

    public GameObject latestTile {get; set;}

    Vector3 posPreClimb;                            //positionen man hade innan man klättrade upp på någon
    public GameObject latestCollision {get; set;} 
    
    GameObject playerClimbed;                       //spelaren man klättrar upp på

    Transform head;                                 //positionen på spelaren man ställer sig
    
    PlayerLiftController playerLiftController;
    PhotonView PV;
    Rigidbody RB;

    Vector3 heightChange; 
    Vector3 yPosChange;


    void Awake()
    {
        PV = GetComponent<PhotonView>();
        RB = GetComponent<Rigidbody>();
        playerLiftController = GetComponent<PlayerLiftController>();
        head = gameObject.transform.GetChild(1);
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
        if (Input.GetKeyDown(PlayerController.crouchButton) && isCrouching && !isClimbed)
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
        gameObject.transform.localScale -= heightChange;
        gameObject.transform.localPosition -= yPosChange;
        isCrouching = true;
        RB.constraints = RigidbodyConstraints.FreezeAll;
        PV.RPC("OnCrouch", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnCrouch()
    {
        gameObject.transform.localScale -= heightChange;
        gameObject.transform.localPosition -= yPosChange;
        isCrouching = true;
    }

    void Stand() 
    {
        gameObject.transform.localScale += heightChange;
        gameObject.transform.localPosition += yPosChange;
        isCrouching = false;
        RB.constraints = RigidbodyConstraints.FreezeRotation;
        PV.RPC("OnStand", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnStand()
    {
        gameObject.transform.localScale += heightChange;
        gameObject.transform.localPosition += yPosChange;
        isCrouching = false;
    }

    void Climb()
    {
        playerClimbed = latestCollision;
        posPreClimb = gameObject.transform.position;
        gameObject.transform.parent = playerClimbed.transform;
        gameObject.transform.localPosition = head.localPosition;
        isClimbing = true;
        playerClimbed.GetComponent<PlayerClimbController>().isClimbed = true;
        RB.constraints = RigidbodyConstraints.FreezeAll;
        PV.RPC("OnClimb", RpcTarget.OthersBuffered, playerClimbed.GetComponent<PhotonView>().ViewID);
    }
    
    [PunRPC]
    void OnClimb(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        gameObject.transform.parent = obj.transform;
        PlayerClimbController otherClimbController = obj.GetComponent<PlayerClimbController>(); 
        otherClimbController.isClimbed = true;
        isClimbing = true;
    }

    void ClimbDown()
    {
        gameObject.transform.parent = null;
        gameObject.transform.position = posPreClimb;
        isClimbing = false;
        playerClimbed.GetComponent<PlayerClimbController>().isClimbed = false;
        RB.constraints = RigidbodyConstraints.FreezeRotation;
        PV.RPC("OnClimbDown", RpcTarget.OthersBuffered, playerClimbed.GetComponent<PhotonView>().ViewID, posPreClimb);
    }

    [PunRPC]
    void OnClimbDown(int viewID, Vector3 _posPreClimb)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        gameObject.transform.parent = null;
        isClimbing = false;
        obj.GetComponent<PlayerClimbController>().isClimbed = false;
    }

    bool CanClimb(int _canClimbID)
    {
        return canClimbID == _canClimbID;
    }
}