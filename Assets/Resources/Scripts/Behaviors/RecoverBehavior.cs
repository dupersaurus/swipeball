using UnityEngine;
using System.Collections;

public class RecoverBehavior : AIBehavior {

    /// <summary>
    /// Pick up the dead ball
    /// </summary>
    /// <param name="pos">Position.</param>
    public void Initialize(Vector2 pos) {
        myCharacter.OnCompleteMove += MoveComplete;
        myCharacter.RunToPosition(pos);
    }

    private void MoveComplete(Vector2 pos) {
        myCharacter.OnCompleteMove -= MoveComplete;
        myAI.game.PickupBall(myAI);
    }
}

