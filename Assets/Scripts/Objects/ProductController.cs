﻿using Photon.Pun;
using UnityEngine;

public class ProductController : MonoBehaviour, LiftableProduct, IPunInstantiateMagicCallback
{
    public bool isLifted {get; set;} = false;
    public bool isPackaged {get; set;} = false;
    public string type {get; set;}

    public static Vector3 tileOffset = new Vector3(1.5f, 0.25f, 1.5f);
    public static Vector3 tableOffset = new Vector3(1.5f, 0.55f, 1.5f);

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        type = (string) initData[0];
    }
}
