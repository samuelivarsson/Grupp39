using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTriggerCheck : MonoBehaviour
{
    Material standardTile;
    Material standardDropZone;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlaceableTile") || other.CompareTag("NonPlaceableTile") || other.CompareTag("DropZone"))
        {
            PlayerManager.myPlayerLiftController.latestTile = other.gameObject;
            Renderer renderer = other.GetComponent<Renderer>();
            if (other.CompareTag("PlaceableTile"))
            {
                if (standardTile == null) standardTile = renderer.material;
                Highlight(renderer, standardTile);
            }
            else if (other.CompareTag("DropZone"))
            {
                if (standardDropZone == null) standardDropZone = renderer.material;
                Highlight(renderer, standardDropZone);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Renderer renderer = other.GetComponent<Renderer>();
        if (other.CompareTag("PlaceableTile")) renderer.material = standardTile;
        else if (other.CompareTag("DropZone")) renderer.material = standardDropZone;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlaceableTile"))
        {
            //Debug.Log("Stay + " + other.name);
        }
    }

    private void Highlight(Renderer renderer, Material material)
    {
        Material matTemp = new Material(material);
        matTemp.SetColor("_Color", Color.red);
        renderer.material = matTemp;
    }
}
