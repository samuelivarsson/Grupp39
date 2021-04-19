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

    [SerializeField] GameObject head;                                 //positionen på spelaren man ställer sig
    
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
        //head = gameObject.transform.GetChild(1);
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
            ClimbRequest();
            return;
        }
        if (Input.GetKeyDown(PlayerController.crouchButton) && isClimbing)
        {
            ClimbDownRequest();
        }
    }

    void Crouch()
    {
        gameObject.transform.localScale -= heightChange;
        gameObject.transform.localPosition -= yPosChange;
        isCrouching = true;
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
        PV.RPC("OnStand", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnStand()
    {
        gameObject.transform.localScale += heightChange;
        gameObject.transform.localPosition += yPosChange;
        isCrouching = false;
    }

    

    void ClimbRequest()
    {
        playerClimbed = latestCollision;
        posPreClimb = gameObject.transform.position;
        playerClimbed.GetPhotonView().RPC("OnClimbRequest", RpcTarget.OthersBuffered, PV.ViewID);
    }

    [PunRPC]
    void OnClimbRequest(int playerToClimbID) 
    {
        if(!PV.IsMine) return;
        
        if(isCrouching && !isClimbed)
        {
            
            PhotonView playerToClimbPV = PhotonView.Find(playerToClimbID);
            _OnClimb(playerToClimbPV);
            playerToClimbPV.GetComponent<Rigidbody>().isKinematic = false;            
            isClimbed = true;
            
            PV.RPC("OnClimb", RpcTarget.OthersBuffered, playerToClimbID);
            playerToClimbPV.RPC("OnClimbSucess", RpcTarget.OthersBuffered, PV.ViewID);
        }
    }
    
    [PunRPC]
    void OnClimb(int viewID)
    {
        PhotonView obj = PhotonView.Find(viewID);
        _OnClimb(obj);

    }

    void _OnClimb (PhotonView playerToClimbPV)
    {
        //HingeJoint hingeJoint = gameObject.AddComponent<HingeJoint>();
        //SetHingeJoint(hingeJoint, playerToClimbPV.GetComponent<Rigidbody>(), head.localPosition);
        
        //Transform playerToClimb = playerToClimbPV.gameObject.transform;
        //playerToClimb.parent = gameObject.transform;
        //playerToClimb.localPosition = head.localPosition;

        
        ActivateHeadJoint(playerToClimbPV.GetComponent<Rigidbody>());
    }

    [PunRPC]
    void OnClimbSucess(int _viewID)
    {
        if(!PV.IsMine) return;

        PhotonView.Find(_viewID).GetComponent<Rigidbody>().isKinematic = false;
        isClimbing = true;
        //RB.constraints = RigidbodyConstraints.FreezeAll;
    }

    void ClimbDownRequest()
    {
        //gameObject.transform.parent = null;
        //gameObject.transform.position = posPreClimb;
        
        //playerClimbed.GetComponent<PlayerClimbController>().isClimbed = false;
        //RB.constraints = RigidbodyConstraints.FreezeRotation;
        
        playerClimbed.GetPhotonView().RPC("OnClimbDownRequest", RpcTarget.OthersBuffered, PV.ViewID);
    }


    [PunRPC]
    void OnClimbDownRequest(int _viewID)
    {
        if(!PV.IsMine) return;

        PhotonView playerClimbingPV = PhotonView.Find(_viewID);
        
        _ClimbDown();
        playerClimbingPV.GetComponent<Rigidbody>().isKinematic = false;
        PV.RPC("OnClimbDown", RpcTarget.OthersBuffered);
        playerClimbingPV.RPC("OnClimbDownSuccess", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnClimbDown()
    {       
        _ClimbDown();        
    }

    void _ClimbDown()
    {
        //Destroy(GetComponent<HingeJoint>());
        DeactivateHeadJoint();
        isClimbed = false;
    }

    [PunRPC]
    void OnClimbDownSuccess()
    {    
        if(!PV.IsMine) return;

        playerClimbed.GetComponent<Rigidbody>().isKinematic = true;  
        RB.position = posPreClimb;
        isClimbing = false;
    }

    bool CanClimb(int _canClimbID)
    {
        return canClimbID == _canClimbID;
    }

    void SetHingeJoint(HingeJoint hingeJoint, Rigidbody conBody, Vector3 _head)
    {
        hingeJoint.anchor = _head;
        hingeJoint.autoConfigureConnectedAnchor = false;
        hingeJoint.connectedAnchor = Vector3.zero;
        hingeJoint.connectedBody = conBody;
    }

    void ActivateHeadJoint(Rigidbody conBody)
    {
        HingeJoint hingeJoint = head.GetComponent<HingeJoint>();        
        hingeJoint.connectedBody = conBody;
        head.SetActive(true);
    }

    void DeactivateHeadJoint()
    {
        HingeJoint hingeJoint = head.GetComponent<HingeJoint>();        
        hingeJoint.connectedBody = null;
        head.SetActive(false);
    }

}