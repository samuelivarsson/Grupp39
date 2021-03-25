using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductCollideCheck : MonoBehaviour
{
    ProductController productController;
    Products product;
   
    Package package;

    void Awake()
    {
        productController = GetComponentInParent<ProductController>();
    }

    void OntriggerEnter(Collider other)
    {
       
         if (other.gameObject != productController.gameObject && other.gameObject.tag == "Package")
        {               
            package = other.GetComponent<Package>();
            package.setLifted(true);            
        }
    }

    void OnTriggerExit(Collider other)
    {
         if (other.gameObject != productController.gameObject && other.gameObject.tag == "Package")
        {
            package = other.GetComponent<Package>();
            package.setLifted(false);
        }

    }

    void OnTriggerStay(Collider other)
    { if (other.gameObject != productController.gameObject && other.gameObject.tag == "Package")
        {
            package = other.GetComponent<Package>();
            package.setLifted(true);


        }
    }
}
