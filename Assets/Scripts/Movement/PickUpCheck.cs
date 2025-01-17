﻿using UnityEngine;
using Photon.Pun;

public class PickUpCheck : MonoBehaviour
{
    PhotonView PV;
    PlayerLiftController playerLiftController;
    PlayerPackController playerPackController;
    PlayerClimbController playerClimbController;
    PlayerMultiLiftController playerMultiLiftController;

    void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
        playerLiftController = GetComponentInParent<PlayerLiftController>();
        playerPackController = GetComponentInParent<PlayerPackController>();
        playerClimbController = GetComponentInParent<PlayerClimbController>();
        playerMultiLiftController = GetComponentInParent<PlayerMultiLiftController>();
    }

    void OnTriggerEnter(Collider other)
    {
        IHighlight highlighter = GetHighlighter(other.gameObject);
        if (highlighter != null && PV.IsMine && !playerMultiLiftController.isMultiLifting) highlighter.Highlight(true);

        if (other.CompareTag("ProductController") && other.GetComponent<ProductController>().isPackaged) return;

        if (other.gameObject != playerLiftController.gameObject && PV.IsMine)
        {
            if (other.gameObject.CompareTag("ProductController") || other.gameObject.CompareTag("ProductManager") || other.gameObject.CompareTag("PackageController") || other.gameObject.CompareTag("PackageManager"))
            {
                playerLiftController.canLiftID = other.gameObject.GetComponent<PhotonView>().ViewID;
                playerLiftController.latestCollision = other.gameObject;
                if (other.gameObject.CompareTag("PackageController"))
                {
                    playerPackController.canTapeID = other.gameObject.GetComponent<PhotonView>().ViewID;
                    playerPackController.latestCollision = other.gameObject;
                }
            }
        }

        if (other.CompareTag("PlayerController"))
        {
            playerClimbController.canClimbID = other.gameObject.GetComponent<PhotonView>().ViewID;
            playerClimbController.latestCollision = other.gameObject;
        }

        PhotonView view = PhotonView.Find(playerLiftController.liftingID);
        if (other.CompareTag("PackageController") && playerLiftController.IsLifting() && view != null && view.CompareTag("ProductController"))
        {
            playerPackController.canPackID = other.GetComponent<PhotonView>().ViewID;
            playerPackController.latestCollision = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        IHighlight highlighter = GetHighlighter(other.gameObject);
        if (highlighter != null && PV.IsMine) highlighter.Highlight(false);

        if (other.CompareTag("ProductController") && other.GetComponent<ProductController>().isPackaged) return;

        if (other.gameObject != playerLiftController.gameObject && PV.IsMine)
        {
            if (other.gameObject.CompareTag("ProductController") || other.gameObject.CompareTag("ProductManager") || other.gameObject.CompareTag("PackageController") || other.gameObject.CompareTag("PackageManager"))
            {
                playerLiftController.canLiftID = -1;
                if (other.gameObject.CompareTag("PackageController"))
                {
                    playerPackController.canTapeID = -1;
                }
            }
        }

        if (other.CompareTag("PlayerController"))
        {
            playerClimbController.canClimbID = -1;
        }

        PhotonView view = PhotonView.Find(playerLiftController.liftingID);
        if (other.CompareTag("PackageController") && playerLiftController.IsLifting() && view != null && view.CompareTag("ProductController"))
        {
            playerPackController.canPackID = -1;
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        IHighlight highlighter = GetHighlighter(other.gameObject);
        if (other.gameObject.CompareTag("PackageController") && playerMultiLiftController.isMultiLifting) highlighter.Highlight(false);
        if (other.CompareTag("ProductController") && other.GetComponent<ProductController>().isPackaged) return;

        if (other.gameObject != playerLiftController.gameObject && PV.IsMine)
        {
            if (other.gameObject.CompareTag("ProductController") || other.gameObject.CompareTag("ProductManager") || other.gameObject.CompareTag("PackageController") || other.gameObject.CompareTag("PackageManager"))
            {
                playerLiftController.canLiftID = other.gameObject.GetComponent<PhotonView>().ViewID;
                if (other.gameObject.CompareTag("PackageController"))
                {
                    playerPackController.canTapeID = other.gameObject.GetComponent<PhotonView>().ViewID;
                }
            }
        }

        if (other.CompareTag("PlayerController") && other.GetComponent<PlayerClimbController>().isCrouching)
        {
            playerClimbController.canClimbID = other.gameObject.GetComponent<PhotonView>().ViewID;
        }

        PhotonView view = PhotonView.Find(playerLiftController.liftingID);
        if (other.CompareTag("PackageController") && playerLiftController.IsLifting() && view != null && view.CompareTag("ProductController"))
        {
            playerPackController.canPackID = other.GetComponent<PhotonView>().ViewID;
        }
    }

    public static IHighlight GetHighlighter(GameObject obj)
    {
        return obj.CompareTag("ProductController") || obj.CompareTag("ProductManager") ? obj.GetComponent<ProductHighlight>() as IHighlight : obj.GetComponent<PackageHighlight>() as IHighlight;
    }
}
