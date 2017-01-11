using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour {

    public delegate void BallThrow(Ball ball, AI thrower);
    public delegate void BallDead(Ball ball);

    public event BallThrow onBallThrow;
    public event BallDead onBallDead;

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private List<AI> ais;

    private Ball liveBall = null;

    /// <summary>
    /// AI currently in possession of the ball
    /// </summary>
    private AI possessor = null;

    private float referenceSwipeSpeed = 20;
    private float referenceBallSpeed = 6;

    private Vector2 throwPos;

    public AnimationCurve speedCurve;

    // Use this for initialization
    void Start() {
        for (int i = 0; i < ais.Count; i++) {
            ais[i].Initialize(this);
        }

        possessor = ais[0].GetComponent<AI>();
        PlayBall();
    }
	
    // Update is called once per frame
    void Update() {
        float delta = Time.deltaTime;

        if (liveBall) {
            liveBall.Tick(delta);
        }

        ais.ForEach(ai => ai.Tick(delta));
    }

    private T ChangeAIBehavior<T>(AI ai) where T : AIBehavior {
        T behavior = ai.ChangeBehavior<T>();
        behavior.TakeControl();

        return behavior;
    }

    public void TargetLocation(Vector2 pos) {
        throwPos = pos;
    }

    public void IssueCommand(Vector2 normal, float curve, float speed, float distance) {
        
    }

    /// <summary>
    /// Ball put in play is no longer needed
    /// </summary>
    protected void RetireBall() {
        if (liveBall) {
            Destroy(liveBall.gameObject);
            liveBall = null;
        }
    }

    public void ThrowBall(Vector2 normal, float curve, float speed, float distance) {
        if (liveBall) {
            return;
        }

        float ballSpeed = speedCurve.Evaluate(speed / referenceSwipeSpeed) * referenceBallSpeed;

        GameObject go = Instantiate<GameObject>(ballPrefab);
        liveBall = go.GetComponent<Ball>();
        liveBall.ThrowBall(this, possessor.gameObject.transform.position, normal, curve, ballSpeed, distance);

        onBallThrow(liveBall, possessor);
    }

    public void CatchBall(AI fielder) {
        if (possessor) {
            ChangeAIBehavior<WanderBehavior>(possessor);
        }

        possessor = fielder;
        ChangeAIBehavior<PossessorBehavior>(fielder);

        RetireBall();
    }

    /// <summary>
    /// Ball was dropped in catch attempt
    /// </summary>
    public void BallDropped() {

    }

    /// <summary>
    /// Ball was intercepted
    /// </summary>
    public void BallIntercepted() {

    }

    /// <summary>
    /// Ball goes dead without action
    /// </summary>
    public void BallDied() {
        Vector3 pos = liveBall.gameObject.transform.position;
        DeadBall(pos);
    }

    /// <summary>
    /// Turnover on a dead ball
    /// </summary>
    /// <param name="pos">Position of the dead ball</param>
    protected void DeadBall(Vector3 pos) {

        // Pick team member to recover 
        int oldTeam = possessor.team;
        AI recoverer = null;
        float closest = float.MaxValue;
        float dist;

        // Find the closest
        for (int i = 0; i < ais.Count; i++) {
            if (ais[i].team != oldTeam) {
                dist = (ais[i].gameObject.transform.position - pos).magnitude;

                if (dist < closest) {
                    recoverer = ais[i];
                    closest = dist;
                }
            }
        }

        // Set new behaviors
        for (int i = 0; i < ais.Count; i++) {
            if (ais[i] == recoverer) {
                RecoverBehavior behavior = ChangeAIBehavior<RecoverBehavior>(ais[i]);
                behavior.Initialize(pos);
            }
            else {
                ChangeAIBehavior<WaitingBehavior>(ais[i]);
            }
        }
    }

    /// <summary>
    /// An AI picks up a dead ball
    /// </summary>
    /// <param name="player">Player.</param>
    public void PickupBall(AI player) {
        possessor = player;
        PlayBall();
    }

    protected void PlayBall() {
        AIBehavior behavior;
        RetireBall();

        for (int i = 0; i < ais.Count; i++) {
            if (ais[i] == possessor) {
                ChangeAIBehavior<PossessorBehavior>(ais[i]);
            }
            else if (ais[i].team == possessor.team) {
                // TODO have team offensive policy
                ChangeAIBehavior<WanderBehavior>(ais[i]);
            }
            else {
                // TODO have team defensive policy
                ChangeAIBehavior<WaitingBehavior>(ais[i]);
            }
        }
    }
}