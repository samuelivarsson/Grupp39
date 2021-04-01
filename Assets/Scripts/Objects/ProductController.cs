using Photon.Pun;
using UnityEngine;

public class ProductController : MonoBehaviour, LiftableProduct
{
    public bool isLifted {get; set;} = false;
    public bool isPackaged {get; set;} = false;
    public string type {get; set;}

    public static Vector3 tileOffset = new Vector3(1.5f, 0.25f, 1.5f);
    public static Vector3 cabinetOffset = new Vector3(1.5f, 0.5f, 1.5f);

}
