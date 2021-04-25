using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSettings : MonoBehaviour
{
    public static RoomSettings Instance;

    public int maxHealth {get; set;}
    public int baseTime {get; set;}
    public int amountMultiplier {get; set;}
    public float taskDelay {get; set;}

    void Awake()
    {
        Instance = this;
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
    }

    void Medium()
    {
        maxHealth = 4;
        baseTime = 40;
        amountMultiplier = 20;
        taskDelay = 10f;
    }

    void Hard()
    {
        maxHealth = 3;
        baseTime = 30;
        amountMultiplier = 15;
        taskDelay = 5f;
    }
}
