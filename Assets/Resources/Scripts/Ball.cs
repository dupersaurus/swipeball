using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ball : MonoBehaviour, ITicker {

    struct HeightStep {
        public float time;
        public float height;

        public HeightStep(float stepTime, float stepHeight) {
            time = stepTime;
            height = stepHeight;
        }
    }

    private const float GRAVITY = -2f;
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

    private float launchTime = 0;

    private List<HeightStep> debugHeights;
    private List<HeightStep> flightHeights;

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
        
        // Debug
        if (savedPath != null) {
            Vector3 lastPos = savedPath[0].position;
            Vector3 lastHeight = lastPos;
            lastHeight.y += savedPath[0].height;

            for (int i = 1; i < savedPath.Length; i++) {
                Vector3 height = savedPath[i].position;
                height.y += savedPath[i].height;

                Debug.DrawLine(lastPos, savedPath[i].position, Color.yellow, 1, false);
                Debug.DrawLine(lastHeight, height, Color.magenta, 1, false);

                lastPos = savedPath[i].position;
                lastHeight = height;
            }

            Vector3 dpos = new Vector3();
            lastPos = new Vector3(-1, debugHeights[0].height);
            for (var i = 1; i < debugHeights.Count; i++) {
                dpos = lastPos;
                dpos.x = debugHeights[i].time;
                dpos.y = debugHeights[i].height;
                Debug.DrawLine(lastPos, dpos, Color.green, 1);
                lastPos = dpos;
            }
            
            if (flightHeights.Count > 0) {
                float startTime = flightHeights[0].time;
                dpos = new Vector3();
                lastPos = new Vector3(-1, flightHeights[0].height);
                for (var i = 1; i < flightHeights.Count; i++) {
                    dpos = lastPos;
                    dpos.x = flightHeights[i].time - startTime;
                    dpos.y = flightHeights[i].height;
                    Debug.DrawLine(lastPos, dpos, Color.blue, 1);
                    lastPos = dpos;
                }
            }
        }
        // Debug

        if (isDead || ballVelocity == Vector3.zero) {
            return;
        }

        Vector3 pos = gameObject.transform.position;
        pos += ballVelocity * delta;
        pos.z = 0;
        gameObject.transform.position = pos;

        float vertHeight = ballSprite.transform.localPosition.y;
        float vertVelocity = ballVelocity.z;
        CalculateHeightStep(ref vertHeight, ref vertVelocity, delta);

        pos = ballSprite.transform.localPosition;
        flightHeights.Add(new HeightStep(Time.time, pos.y));

        //pos.y += ballVelocity.z * delta;
        pos.y = vertHeight;
        ballVelocity.z = vertVelocity;

        if (pos.y <= 0) {
            pos.y = 0;
            isDead = true;
            gameState.BallDied();
            Debug.Log("hangtime >> " + (Time.time - launchTime));
        }

        ballSprite.gameObject.transform.localPosition = pos;

        /*bool climbing = ballVelocity.z >= 0;
        ballVelocity.z += (GRAVITY * delta);

        if (climbing && ballVelocity.z <= 0) {
            Debug.Log("tick >> highest: " + ballSprite.gameObject.transform.localPosition.y);
        }*/

        //ballVelocity = Quaternion.AngleAxis(curveRate * delta, Vector3.forward) * ballVelocity;
    }

    private void CalculateHeightStep(ref float height, ref float velocity, float timeStep) {
        height = height + velocity * timeStep;
        velocity = velocity + GRAVITY * timeStep;
    }

    public void ThrowBall(GameState game, Vector2 position, Vector2 direction, float curve, float speed, float distance) {
        gameState = game;

        if (speed < 2) {
            curve = 0;
        } else {
            curve = 0;
        }

        gameObject.transform.position = position;

        float launchAngle = 1; //Mathf.Clamp01(distance / referenceThrowDistance); // * 45;
        
        ballVelocity = direction * speed;
        ballVelocity.z = launchAngle * 2; //Quaternion.AngleAxis(launchAngle, Vector3.left) * direction;
        //ballVelocity.z = (1 - Mathf.Clamp01(distance / referenceThrowDistance)) * maxThrowArc; //(distance / speed) / 2;
        curveRate = curve;

        Debug.Log("ThrowBall >> height: " + ballSprite.transform.localPosition.y + ", velocity: " + ballVelocity.z);
        
        launchTime = Time.time;
        ProjectPath();
    }

    public BallPath[] ProjectPath() {
        if (savedPath == null) {
            List<BallPath> path = new List<BallPath>();
            Vector2 lateralVelocity = ballVelocity;

            if (debugHeights != null) {
                debugHeights.Clear();
            } else {
                debugHeights = new List<HeightStep>();
                flightHeights = new List<HeightStep>();
            }

            BallPath now = new BallPath();
            now.time = 0;
            now.position = gameObject.transform.position;
            now.height = Mathf.Abs(ballSprite.transform.localPosition.y);
            now.velocity = ballVelocity;
            path.Add(now);

            Debug.Log("ProjectPath >> height: " + now.height + ", velocity: " + ballVelocity.z);

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
        float fallTime = ProjectFallTime(height, verticalVelocity); //(Mathf.Sqrt(2 * -GRAVITY * height + verticalVelocity * verticalVelocity) - verticalVelocity) / -GRAVITY;
        float distance = lateralVelocity.magnitude * fallTime;
        BallPath path = new BallPath();
        List<BallPath> paths = new List<BallPath>();

        //Debug.Log("ProjectPathSegment >> height: " + height + " velocity: " + verticalVelocity + " falltime: " + fallTime);

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
            path.time = ray.distance / lateralVelocity.magnitude;
            //path.height = verticalVelocity * path.time + 0.5f * GRAVITY * (path.time * path.time) + height;
            path.height = ProjectHeightAtTime(height, verticalVelocity, path.time);
            path.velocity = Vector2.Reflect(lateralVelocity, ray.normal) * BOUNCE_FACTOR;
            path.velocity.z = ReboundVerticalVelocity(ProjectVerticalVelocityAtTime(verticalVelocity, path.time)); //verticalVelocity * (verticalVelocity > 0 ? BOUNCE_FACTOR : 1);

            paths.Add(path);
            paths.AddRange(ProjectPathSegment(path.velocity, path.velocity.z, ray.point, path.height));
        }

        return paths;
    }

    private float ProjectFallTime(float height, float speed) {
        //return (Mathf.Sqrt(2 * -GRAVITY * height + speed * speed) - speed) / -GRAVITY;

        float stepSpeed = speed;
        float stepHeight = height;
        float totalTime = 0;
        float step = 0.01f;
        bool highest = false;

        while (stepHeight >= 0) {
            debugHeights.Add(new HeightStep(totalTime, stepHeight));
            
            CalculateHeightStep(ref stepHeight, ref stepSpeed, step);
            totalTime += step; 
            
            if (!highest && stepSpeed < 0) {
                highest = true;
                Debug.Log("Project >> highest: " + stepHeight);
            }
        }

        return totalTime;
    }

    private float ProjectHeightAtTime(float height, float speed, float time) {
        float stepSpeed = speed;
        float stepHeight = height;
        float totalTime = 0;
        float step = 0.01f;

        while (totalTime < time) {
            CalculateHeightStep(ref stepHeight, ref stepSpeed, step);
            totalTime += step;
        }

        return stepHeight;
    }

    private float ProjectVerticalVelocityAtTime(float speed, float time) {
        float stepSpeed = speed;
        float stepHeight = 0;
        float totalTime = 0;
        float step = 0.01f;

        while (totalTime < time) {
            CalculateHeightStep(ref stepHeight, ref stepSpeed, step);
            totalTime += step;
        }

        return stepSpeed;
    }
        
    void OnCollisionEnter2D(Collision2D coll) {
        ContactPoint2D impact = coll.contacts[0];
        ballVelocity = Vector2.Reflect(ballVelocity, impact.normal);

        ballVelocity.x *= BOUNCE_FACTOR;
        ballVelocity.y *= BOUNCE_FACTOR;
        ballVelocity.z = ReboundVerticalVelocity(ballVelocity.z);

        //transform.position = impact.point;
        curveRate = 0;
    }

    /// <summary>
    /// Determine the new vertical speed of the ball after a rebound
    /// </summary>
    /// <param name="verticalVelocity">The vertical speed before the rebound</param>
    /// <returns>The new vertical speed</returns>
    private float ReboundVerticalVelocity(float verticalVelocity) {
        if (verticalVelocity > 0) {
            verticalVelocity *= BOUNCE_FACTOR;
        }

        return verticalVelocity;
    }
}

