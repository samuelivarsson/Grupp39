using System.Collections.Generic;

public interface LiftablePackage : Liftable
{
    bool canTape {get; set;}
    List<int> lifters {get; set;}
    bool tooHeavy {get; set;}
}
