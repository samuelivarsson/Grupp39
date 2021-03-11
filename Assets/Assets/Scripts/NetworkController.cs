using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        //connect to master server
        PhotonNetwork.ConnectUsingSettings();

        //connect to region server
        //bool v = PhotonNetwork.ConnectToRegion("eu");
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to the " + PhotonNetwork.CloudRegion + " server!");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

