using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AI))]
public class AIBehavior : MonoBehaviour, ITicker {

    protected AI myAI = null;
    protected Character myCharacter = null;
    protected Player myPlayer = null;

    // Use this for initialization
    virtual protected void Awake() {
        myAI = GetComponent<AI>();
        myPlayer = GetComponent<Player>();
        myCharacter = GetComponent<Character>();
    }
	
    /// <summary>
    /// Tell the behavior that is has taken control of the AI
    /// </summary>
    virtual public void TakeControl() {
        
    }

    virtual public void Tick(float delta) {
	
    }

    /// <summary>
    /// Notification that the ball has been put into play
    /// </summary>
    /// <param name="ball">Ball.</param>
    /// <param name="thrower">Thrower.</param>
    public virtual void BallThrown(Ball ball, AI thrower) {

    }

    public virtual void BallDead(Ball ball) {

    }

    /// <summary>
    /// Notification that the ball is within reach of the player. Called every tick in
    /// which it is in range.
    /// </summary>
    /// <param name="ball">Ball.</param>
    public virtual void BallInReach(Ball ball) {

    }
}

