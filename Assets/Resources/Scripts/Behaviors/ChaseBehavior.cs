using UnityEngine;
using System.Collections;

/// <summary>
/// Chase down a thrown ball
/// </summary>
public class ChaseBehavior : AIBehavior {
    
    Vector3 ballIntercept;
    Ball targetBall = null;

    /// <summary>
    /// Initialize the behavior with the thrown ball information
    /// </summary>
    /// <param name="ball">The thrown ball</param>
    /// <param name="thrower">The player that threw the ball</param>
    public void Initialize(Ball ball, AI thrower, Vector3 intercept) {
        targetBall = ball;
        ballIntercept = intercept;
        Debug.Log("Run to >> " + intercept);

        ChaseBall();
    }

    protected void Start() {
        ChaseBall();
    }

    private void ChaseBall() {
        if (targetBall) {
            myCharacter.OnCompleteMove += RunComplete;
            myCharacter.RunToPosition(ballIntercept);
        }
    }

    private void RunComplete(Vector2 pos) {
        myCharacter.OnCompleteMove -= RunComplete;
    }

    /// <summary>
    /// Attempt to catch the ball. Doesn't make the actual attempt until 
    /// the ball is at is closest approach to the player.
    /// </summary>
    /// <param name="ball">Ball.</param>
    public override void BallInReach(Ball ball) {
        myAI.game.CatchBall(myAI);
    }

    public override void BallDead(Ball ball) {
        myAI.ChangeBehavior<WanderBehavior>();
    }
}

