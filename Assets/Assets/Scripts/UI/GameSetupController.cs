using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.IO;

public class GameSetupController : MonoBehaviour {

    private Vector3 startPos = new Vector3(10, 1.5f, 12);
    void Start() {
        CreatePlayer();
    }


    private void CreatePlayer() {
        Debug.Log("Creating Player");
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player"), startPos, Quaternion.identity);
    }
}
