using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductCollideCheck : MonoBehaviour
{
    ProductController productController;
    ProductManager product;
   
    PackageController package;

    void Awake()
    {
        productController = GetComponentInParent<ProductController>();
    }

    void OntriggerEnter(Collider other)
    {
       
        if (other.gameObject != productController.gameObject && other.gameObject.CompareTag("PackageController"))
        {               
            package = other.GetComponent<PackageController>();
            package.SetCanPackage(true);            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject != productController.gameObject && other.gameObject.CompareTag("PackageController"))
        {
                package = other.GetComponent<PackageController>();
                package.SetCanPackage(false);
        }

    }

    void OnTriggerStay(Collider other)
    { 
        if (other.gameObject != productController.gameObject && other.gameObject.CompareTag("PackageController"))
        {
            package = other.GetComponent<PackageController>();
            package.SetCanPackage(true);
        }
    }
}
