using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskInfo : MonoBehaviour
{
    // The objects in the order
    private List<GameObject> orderedProducts;
    [SerializeField] RenderTexture product;
    [SerializeField] RawImage rawImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rawImage.texture = product;
    }
}
