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
        Ball.BallPath[] path = ball.ProjectPath();
        Vector2 playerBallDiff;
        Vector2 ballDirection;
        Vector2 intercept;
        float playerTime;
        float ballTime;

        Vector2 closestIntercept = Vector2.zero;
        float closestTime = float.MaxValue;

        if (path == null) {
            return;
        }

        for (int i = 1; i < path.Length; i++) {
            ballDirection = path[i].position - path[i - 1].position;
            playerBallDiff = (Vector2)gameObject.transform.position - path[i - 1].position;
            intercept = (Vector2)Vector3.Project(playerBallDiff, ballDirection) + path[i - 1].position;

            playerTime = GetComponent<Character>().CalculateRunTime(intercept);
            ballTime = playerBallDiff.sqrMagnitude / path[i - 1].velocity.sqrMagnitude;

            if (playerTime <= ballTime && playerTime < closestTime) {
                closestIntercept = intercept;
                closestTime = playerTime;
            }
        }

        if (closestTime < 10000) {
            ChaseBehavior newBehavior = myAI.ChangeBehavior<ChaseBehavior>();
            newBehavior.Initialize(ball, thrower, closestIntercept);
        }
    }

    /// <summary>
    /// Opportunistic chance to catch the ball
    /// </summary>
    /// <param name="ball">Ball.</param>
    public override void BallInReach(Ball ball) {
        
    }
}

