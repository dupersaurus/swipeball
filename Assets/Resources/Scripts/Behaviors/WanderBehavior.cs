using UnityEngine;
using System.Collections;

/// <summary>
/// Offensive AI that wanders the field until the ball is thrown
/// </summary>
public class WanderBehavior : AIBehavior {

    /// <summary>
    /// Ball has been thrown, determine if we can make a play on it.
    /// </summary>
    /// <param name="ball">Ball.</param>
    /// <param name="thrower">Thrower.</param>
    public override void BallThrown(Ball ball, AI thrower) {

        // Find closest point the ball will be to player. If the player can
        // make it to that point with or before the ball, chase it down.
        // Let's ignore curve for now.
        Vector3 ballDirection = ball.velocity.normalized;
        ballDirection.z = 0;
        Vector3 diff = gameObject.transform.position - ball.gameObject.transform.position;
        Vector3 ballDiff = Vector3.Project(diff, ballDirection);
        Vector3 intercept = ballDiff + ball.gameObject.transform.position;

        float playerTime = GetComponent<Character>().CalculateRunTime(intercept);
        float ballTime = ballDiff.sqrMagnitude / ball.velocity.sqrMagnitude;

        if (playerTime <= ballTime) {
            ChaseBehavior newBehavior = myAI.ChangeBehavior<ChaseBehavior>();
            newBehavior.Initialize(ball, thrower, intercept);
        }
    }

    /// <summary>
    /// Opportunistic chance to catch the ball
    /// </summary>
    /// <param name="ball">Ball.</param>
    public override void BallInReach(Ball ball) {
        
    }
}

