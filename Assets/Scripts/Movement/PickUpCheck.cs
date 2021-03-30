using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PickUpCheck : MonoBehaviour
{
    PhotonView PV;
    PlayerLiftController playerLiftController;
    PlayerPackController playerPackController;

    void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
        playerLiftController = GetComponentInParent<PlayerLiftController>();
        playerPackController = GetComponentInParent<PlayerPackController>();
    }

    void OnTriggerEnter(Collider other)
    {
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
    }

    void OnTriggerExit(Collider other)
    {
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
    }
    
    void OnTriggerStay(Collider other)
    {
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
    }
}
