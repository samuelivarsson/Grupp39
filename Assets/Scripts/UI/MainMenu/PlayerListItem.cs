﻿using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [SerializeField] TMP_Text text;

    public override void OnLeftRoom()
    {
        if (GetComponent<PhotonView>().IsMine) PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        string name = (string) initData[0];
        text.text = name;
        if (GetComponent<PhotonView>().IsMine) text.text += " (Du)";
        transform.SetParent(Launcher.Instance.playerListContent);
        GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }
}
