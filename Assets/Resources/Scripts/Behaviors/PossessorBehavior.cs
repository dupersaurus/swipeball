using System;
using System.Collections.Generic;

public class PossessorBehavior : AIBehavior {

    public override void TakeControl() {
        myCharacter.GiveBall();
    }

    public override void BallThrown(Ball ball, AI thrower) {
        myCharacter.RemoveBall();
    }

    public override void BallDead(Ball ball) {
        
    }
}