using UnityEngine;

public interface Liftable
{
    bool isLifted {get; set;}
    bool isPackaged {get; set;}

    bool canTape { get; set; }
}
