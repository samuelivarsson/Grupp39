using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PickUpCheck : MonoBehaviour
{
    ProductController productController;
    ProductManager productManager;
    PlayerController playerController;
    PhotonView PV;
    ProductController packageController;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        PV = playerController.GetComponent<PhotonView>();
    }

    void OntriggerEnter(Collider other)
    {
        if(other.gameObject != playerController.gameObject && PV.IsMine && other.gameObject.CompareTag("ProductController"))
        {
            productController = other.GetComponent<ProductController>();
            productController.SetCanPickUp(true);
        }
        else if(other.gameObject != playerController.gameObject && PV.IsMine && (other.gameObject.CompareTag("ProductManager") || other.gameObject.CompareTag("PackageManager")))
        {
            productManager = other.GetComponent<ProductManager>();
            productManager.SetCanPickUp(true);
        }
        else if (other.gameObject != playerController.gameObject && other.gameObject.CompareTag("PackageController"))
        {         
            packageController = other.GetComponent<ProductController>();
            packageController.SetCanPickUp(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject != playerController.gameObject && PV.IsMine && other.gameObject.CompareTag("ProductController"))
        {
            productController = other.GetComponent<ProductController>();
            productController.SetCanPickUp(false);
        }
        else if(other.gameObject != playerController.gameObject && PV.IsMine && (other.gameObject.CompareTag("ProductManager") || other.gameObject.CompareTag("PackageManager")))
        {
            productManager = other.GetComponent<ProductManager>();
            productManager.SetCanPickUp(false);
        }
        else if (other.gameObject != playerController.gameObject && other.gameObject.CompareTag("PackageController"))
        {
            packageController = other.GetComponent<ProductController>();
            packageController.SetCanPickUp(false);
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if(other.gameObject != playerController.gameObject && PV.IsMine && other.gameObject.CompareTag("ProductController"))
        {
            productController = other.GetComponent<ProductController>();
            productController.SetCanPickUp(true);
            
        }
        else if(other.gameObject != playerController.gameObject && PV.IsMine && (other.gameObject.CompareTag("ProductManager") || other.gameObject.CompareTag("PackageManager")))
        {
            productManager = other.GetComponent<ProductManager>();
            productManager.SetCanPickUp(true);
        }
        else if (other.gameObject != playerController.gameObject && other.gameObject.CompareTag("PackageController"))
        {
            packageController = other.GetComponent<ProductController>();
            packageController.SetCanPickUp(true);
        }
    }
}
