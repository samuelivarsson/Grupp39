using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{

    //List<GameObject> droppedDeliveries = new List<GameObject>();
    int gatheredPoints = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
   


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DropDown"))
        {
            //droppedDeliveries.Add(gameObject);
            gatheredPoints++;
            Destroy(gameObject);
            Debug.Log("Points:"+ gatheredPoints);
        }
    }

}
