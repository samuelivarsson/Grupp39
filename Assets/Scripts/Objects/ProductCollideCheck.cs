using UnityEngine;
using Photon.Pun;

public class ProductCollideCheck : MonoBehaviour
{
    ProductController productController;
    PlayerPackController playerPackController;

    void Awake()
    {
        productController = GetComponentInParent<ProductController>();
        playerPackController = PlayerManager.myPlayerPackController;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ProductController") && other.GetComponent<ProductController>().isPackaged) return;

        if (other.gameObject != productController.gameObject && other.gameObject.CompareTag("PackageController") && productController.isLifted)
        {
            playerPackController.canPackID = other.GetComponent<PhotonView>().ViewID;
            playerPackController.latestCollision = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ProductController") && other.GetComponent<ProductController>().isPackaged) return;

        if (other.gameObject != productController.gameObject && other.gameObject.CompareTag("PackageController") && productController.isLifted)
        {
            playerPackController.canPackID = -1;     
        }

    }

    void OnTriggerStay(Collider other)
    { 
        if (other.CompareTag("ProductController") && other.GetComponent<ProductController>().isPackaged) return;

        if (other.gameObject != productController.gameObject && other.gameObject.CompareTag("PackageController") && productController.isLifted)
        {
            playerPackController.canPackID = other.GetComponent<PhotonView>().ViewID;     
        }
    }
}
