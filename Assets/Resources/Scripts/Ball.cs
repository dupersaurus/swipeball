using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ball : MonoBehaviour, ITicker {

    private const float GRAVITY = -1f;
    private const float BOUNCE_FACTOR = 0.5f;

    public class BallPath {
        /// <summary>The time it will take the ball to get here, in seconds</summary>
        public float time;

        /// <summary>The lateral position of the ball at this moment of time</summary>
        public Vector2 position;

        /// <summary>The height of the ball at the moment</summary>
        public float height;

        /// <summary>The velocity of the ball at the moment</summary>
        public Vector3 velocity;
    }

    [SerializeField]
    private GameObject ballSprite;

    [SerializeField]
    private float referenceThrowDistance = 20.0f;

    [SerializeField]
    private float maxThrowArc = 2;

    private GameState gameState;

    private float startHeight = 0.2f;

    private Vector3 ballVelocity = Vector3.zero;
    private float curveRate;

    private bool isDead = false;
    private BallPath[] savedPath = null;

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

        savedPath = null;

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

        ballVelocity.z += GRAVITY * delta;

        ballVelocity = Quaternion.AngleAxis(curveRate * delta, Vector3.forward) * ballVelocity;
    }

    public void ThrowBall(GameState game, Vector2 position, Vector2 direction, float curve, float speed, float distance) {
        Debug.Log("throw distance >> " + distance);

        gameState = game;

        if (speed < 2) {
            curve = 0;
        } else {
            curve = 0;
        }

        gameObject.transform.position = position;

        ballVelocity = direction * speed;
        ballVelocity.z = (1 - Mathf.Clamp01(distance / referenceThrowDistance)) * maxThrowArc; //(distance / speed) / 2;
        curveRate = curve;
    }

    public BallPath[] ProjectPath() {
        if (savedPath == null) {
            List<BallPath> path = new List<BallPath>();
            Vector2 lateralVelocity = ballVelocity;

            BallPath now = new BallPath();
            now.time = 0;
            now.position = gameObject.transform.position;
            now.height = Mathf.Abs(ballSprite.transform.position.y);
            now.velocity = ballVelocity;
            path.Add(now);

            path.AddRange(ProjectPathSegment(lateralVelocity, ballVelocity.z, now.position, now.height));

            // All of the times are local to the segment, make them sequential
            for (int i = 1; i < path.Count; i++) {
                path[i].time = path[i].time + path[i - 1].time;
            }

            savedPath = path.ToArray();
        }

        return savedPath;
    }

    private List<BallPath> ProjectPathSegment(Vector2 lateralVelocity, float verticalVelocity, Vector3 position, float height) {
        // t = (sqrt(2ad + v^2) - v)/a
        // d = vt + 0.5at^2
        float fallTime = (Mathf.Sqrt(2 * -GRAVITY * height + verticalVelocity * verticalVelocity) - verticalVelocity) / -GRAVITY;
        float distance = lateralVelocity.magnitude * fallTime;
        BallPath path = new BallPath();
        List<BallPath> paths = new List<BallPath>();

        path.position = (Vector2)position + lateralVelocity.normalized * distance;
        path.time = fallTime;

        Vector2 offset = lateralVelocity.normalized * 0.1f;
        RaycastHit2D ray = Physics2D.Raycast(position + (Vector3)offset, lateralVelocity, distance);

        if (ray.collider == null) {
            paths.Add(path);
            path.height = 0;
            path.velocity = Vector3.zero;
        } else {
            path.position = ray.point;
            path.time = ray.distance / lateralVelocity.sqrMagnitude;
            path.height = verticalVelocity * path.time + 0.5f * GRAVITY * (path.time * path.time) + height;
            path.velocity = Vector2.Reflect(lateralVelocity, ray.normal) * BOUNCE_FACTOR;
            path.velocity.z = verticalVelocity * (verticalVelocity > 0 ? BOUNCE_FACTOR : 1);

            paths.Add(path);
            paths.AddRange(ProjectPathSegment(path.velocity, path.velocity.z, ray.point, path.height));
        }

        return paths;
    }
        
    void OnCollisionEnter2D(Collision2D coll) {
        ContactPoint2D impact = coll.contacts[0];
        ballVelocity = Vector2.Reflect(ballVelocity, impact.normal);

        ballVelocity.x *= BOUNCE_FACTOR;
        ballVelocity.y *= BOUNCE_FACTOR;

        if (ballVelocity.z > 0) {
            ballVelocity.z *= BOUNCE_FACTOR;
        }

        //transform.position = impact.point;
        curveRate = 0;
    }
}

