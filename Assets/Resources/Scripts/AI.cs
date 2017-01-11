using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour, ITicker {

    private Player myPlayer = null;
    private Character myCharacter = null;
    private AIBehavior currentBehavior;
    private GameState myGame;

    public int team {
        get { return myPlayer.team; }
    }

    public GameState game {
        get { return myGame; }
    }

    private Ball currentBall = null;

    AIBehavior behavior {
        get {
            if (currentBehavior == null) {
                AIBehavior[] behaviors = GetComponents<AIBehavior>();

                for (int i = 0; i < behaviors.Length; i++) {
                    if (behaviors[i].enabled) {
                        currentBehavior = behaviors[i];
                        break;
                    }
                }
            }

            return currentBehavior;
        }
    }

    void Awake() {
        myPlayer = GetComponent<Player>();
        myCharacter = GetComponent<Character>();
    }
    
    // Use this for initialization
    public void Initialize(GameState gameState) {
        myGame = gameState;
        gameState.onBallThrow += BallThrown;
        gameState.onBallDead += BallDead;
    }
	
    // Update is called once per frame
    public void Tick(float delta) {

        if (behavior) {
            behavior.Tick(delta);
        }

        myCharacter.Tick(delta);

        if (!currentBall || !currentBehavior) {
            return;
        }

        Vector3 distance = currentBall.transform.position - gameObject.transform.position;

        if (distance.sqrMagnitude <= myPlayer.reach * myPlayer.reach) {
            currentBehavior.BallInReach(currentBall);
        }
    }

    /// <summary>
    /// Ball has been thrown, determine what to do
    /// </summary>
    /// <param name="ball">Ball.</param>
    void BallThrown(Ball ball, AI thrower) {
        currentBall = ball;
        behavior.BallThrown(ball, thrower);
    }

    void BallDead(Ball ball) {
        currentBall = null;
        behavior.BallDead(ball);
    }

    /// <summary>
    /// Change the current behavior
    /// </summary>
    /// <typeparam name="T">The type of behavior to change to</typeparam>
    /// <returns>The new behavior</returns>
    public T ChangeBehavior<T>() where T : AIBehavior {
        if (currentBehavior) {
            currentBehavior.enabled = false;
        }

        currentBehavior = (AIBehavior)GetComponent<T>();

        if (!currentBehavior) {
            currentBehavior = gameObject.AddComponent<T>();
        } else {
            currentBehavior.enabled = true;
        }

        return (T)currentBehavior;
    }
}

