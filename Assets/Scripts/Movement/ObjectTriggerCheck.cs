using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTriggerCheck : MonoBehaviour
{
    PlayerController playerController;
    Material latestMaterial;

    void Awake()
    {
        playerController = gameObject.GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlaceableTile" || other.tag == "NonPlaceableTile" || other.tag == "DropZone")
        {
            playerController.SetLatestTile(other.gameObject);
            if (other.tag == "PlaceableTile" || other.tag == "DropZone")
            {
                Highlight(other.GetComponent<Renderer>());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "PlaceableTile" || other.tag == "DropZone")
        {
            other.GetComponent<Renderer>().material = latestMaterial;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "PlaceableTile")
        {
            //Debug.Log("Stay + " + other.name);
        }
    }

    private void Highlight(Renderer renderer)
    {
        latestMaterial = renderer.material;
        Material matTemp = new Material(latestMaterial);
        matTemp.SetColor("_Color", Color.red);
        renderer.material = matTemp;
    }
}
