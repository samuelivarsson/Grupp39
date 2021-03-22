using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollideCheck : MonoBehaviour
{
    ProductController productController;
    Products product;
    PlayerController playerController;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void OntriggerEnter(Collider other)
    {
        if(other.gameObject != playerController.gameObject && other.gameObject.tag == "ProductController")
        {
            productController = other.GetComponent<ProductController>();
            productController.setCanPickUp(true);
            productController.setLatestPlayer(playerController.gameObject.transform);
        }
        else if(other.gameObject != playerController.gameObject && other.gameObject.tag == "Product")
        {
            product = other.GetComponent<Products>();
            product.setCanPickUp(true);
            product.setLatestPlayer(playerController.gameObject.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject != playerController.gameObject && other.gameObject.tag == "ProductController")
        {
            productController = other.GetComponent<ProductController>();
            productController.setCanPickUp(false);
        }
        else if(other.gameObject != playerController.gameObject && other.gameObject.tag == "Product")
        {
            product = other.GetComponent<Products>();
            product.setCanPickUp(false);
        }
        
    }
    
    void OnTriggerStay(Collider other)
    {
        if(other.gameObject != playerController.gameObject && other.gameObject.tag == "ProductController")
        {
            productController = other.GetComponent<ProductController>();
            productController.setCanPickUp(true);
            productController.setLatestPlayer(playerController.gameObject.transform);
            
        }
        else if(other.gameObject != playerController.gameObject && other.gameObject.tag == "Product")
        {
            product = other.GetComponent<Products>();
            product.setCanPickUp(true);
            product.setLatestPlayer(playerController.gameObject.transform);
        }
    }
}
