using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomSettings : MonoBehaviour
{
    int maxHealth;
    int baseTime;
    int amountMultiplier;
    float taskDelay;

    void Awake()
    {
        Easy();
    }

    public void SetDifficulty(int d)
    {
        switch (d)
        {
            // Easy
            case 0:
                Easy();
                break;

            // Medium
            case 1:
                Medium();
                break;

            // Hard
            case 2:
                Hard();
                break;
        }
    }

    void Easy()
    {
        maxHealth = 5;
        baseTime = 60;
        amountMultiplier = 30;
        taskDelay = 15f;
        SetProperties();
    }

    void Medium()
    {
        maxHealth = 4;
        baseTime = 40;
        amountMultiplier = 20;
        taskDelay = 10f;
        SetProperties();
    }

    void Hard()
    {
        maxHealth = 3;
        baseTime = 30;
        amountMultiplier = 15;
        taskDelay = 5f;
        SetProperties();
    }

    void SetProperties()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Hashtable hash = new Hashtable();
        hash.Add("maxHealth", maxHealth);
        hash.Add("baseTime", baseTime);
        hash.Add("amountMultiplier",amountMultiplier);
        hash.Add("taskDelay", taskDelay);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }
}
