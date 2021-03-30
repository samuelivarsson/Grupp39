using Photon.Pun;
using UnityEngine;

public class ProductController : MonoBehaviour, Liftable
{
    public bool isLifted {get; set;} = false;
    public bool isPackaged {get; set;} = false;
    public string type {get; set;}

    public static Vector3 tileOffset = new Vector3(1.5f, 0.25f, 1.5f);
}
