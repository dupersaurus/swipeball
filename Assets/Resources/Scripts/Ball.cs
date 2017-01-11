using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour, ITicker {

    [SerializeField]
    private GameObject ballSprite;

    private GameState gameState;

    private float startHeight = 0.2f;

    private Vector3 ballVelocity = Vector3.zero;
    private float curveRate;

    private bool isDead = false;

    /// <summary>
    /// Current velocity of the ball along the horizontal plane
    /// </summary>
    /// <value>The velocity.</value>
    public Vector2 velocity {
        get { return ballVelocity; }  
    }

    /// <summary>
    /// Current position of the ball, including height in the z-axis
    /// </summary>
    /// <value>The position.</value>
    public Vector3 position {
        get {
            Vector3 pos = gameObject.transform.position;
            pos.z = ballSprite.transform.localPosition.y;

            return pos;
        }
    }

    // Use this for initialization
    void Start() {
        Vector3 offset = new Vector3(0, startHeight, 0);
        ballSprite.transform.localPosition = offset;
    }
	
    // Update is called once per frame
    public void Tick(float delta) {
        if (isDead || ballVelocity == Vector3.zero) {
            return;
        }

        Vector3 pos = gameObject.transform.position;
        pos += ballVelocity * delta;
        pos.z = 0;
        gameObject.transform.position = pos;

        pos = ballSprite.transform.localPosition;
        pos.y += ballVelocity.z * delta;

        if (pos.y <= 0) {
            pos.y = 0;
            isDead = true;
            gameState.BallDied();
        }

        ballSprite.gameObject.transform.localPosition = pos;

        ballVelocity.z -= 1 * delta;

        ballVelocity = Quaternion.AngleAxis(curveRate * delta, Vector3.forward) * ballVelocity;
    }

    public void ThrowBall(GameState game, Vector2 position, Vector2 direction, float curve, float speed, float distance) {
        gameState = game;

        if (speed < 2) {
            curve = 0;
        } else {
            curve = 0;
        }

        gameObject.transform.position = position;

        ballVelocity = direction * speed;
        ballVelocity.z = (distance / speed) / 2;
        curveRate = curve;
    }
        
    void OnCollisionEnter2D(Collision2D coll) {
        ContactPoint2D impact = coll.contacts[0];
        ballVelocity = Vector2.Reflect(ballVelocity, impact.normal);

        ballVelocity.x *= 0.5f;
        ballVelocity.y *= 0.5f;

        if (ballVelocity.z > 0) {
            ballVelocity.z *= 0.5f;
        } else {
            ballVelocity.z *= 2;
        }

        //transform.position = impact.point;
        curveRate = 0;
    }
}

