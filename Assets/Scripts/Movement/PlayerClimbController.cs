using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerClimbController : MonoBehaviour
{
    public int liftingID {get; set;}
    public int canLiftID {get; set;}
    public int crouchingID {get; set;}
    public int canCrouchID {get; set;}

    public GameObject latestTile {get; set;}

    public GameObject latestCollision {get; set;}
    GameObject latestObject;
    
    PhotonView PV;
    Rigidbody RB;

    Vector3 heightChange = new Vector3(-0.1f, 0.25f, -0.1f);   
    Vector3 yPosChange = new Vector3(0, 0.25f, 0);


    void Awake()
    {
        PV = GetComponent<PhotonView>();
        RB = GetComponent<Rigidbody>();
        crouchingID = -1;
        liftingID = -1; 
    }

    void Update()
    {
        if (!PV.IsMine) return;

        CheckCrouchAndClimb();
    }

    void CheckCrouchAndClimb() 
    {
        PlayerLiftController liftController = GetComponent<PlayerLiftController>();

        if(Input.GetKeyDown(PlayerController.crouchButton) && IsCrouching(PV.ViewID))
        {
            Stand();
            return;
        }
        if(Input.GetKeyDown(PlayerController.crouchButton) && IsCrouching(-1) && !IsLifting(PV.ViewID)) //uh croucha inte när du lyfter saker, idk hur jag kollar det nuförtiden
        {
            Crouch();
        }
        
    }

    void Crouch()
    {
        gameObject.transform.localScale -= heightChange;
        gameObject.transform.localPosition -= yPosChange;
        RB.constraints = RigidbodyConstraints.FreezePosition;
        crouchingID = PV.ViewID;
        PV.RPC("OnCrouch", RpcTarget.OthersBuffered, PV.ViewID);
    }

    [PunRPC]
    void OnCrouch(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        obj.transform.localScale -= heightChange;
        obj.transform.localPosition -= yPosChange;
    }

    void Stand() 
    {
        gameObject.transform.localScale += heightChange;
        gameObject.transform.localPosition += yPosChange;
        RB.constraints = RigidbodyConstraints.None;
        RB.constraints = RigidbodyConstraints.FreezeRotation;
        crouchingID = -1;
        PV.RPC("OnStand", RpcTarget.OthersBuffered, PV.ViewID);
    }

    [PunRPC]
    void OnStand(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        obj.transform.localScale += heightChange;
        obj.transform.localPosition += yPosChange;
    }

    Liftable GetController(GameObject obj)
    {
        Liftable result;
        if (obj.CompareTag("ProductController")) result = obj.GetComponent<ProductController>();
        else result = obj.GetComponent<PackageController>();
        return result;
    }

    bool IsCrouching(int _crouchingID)
    {
        return crouchingID == _crouchingID;
    }

    bool IsLifting(int _liftingID)
    {
        return liftingID == _liftingID;
    }

}