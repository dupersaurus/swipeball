using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class Character : MonoBehaviour, ITicker {

    public delegate void CompleteMove(Vector2 pos);
    public event CompleteMove OnCompleteMove;

    [SerializeField]
    private GameObject heldBall;

    private Player player = null;

    private Vector3 targetMoveLocation = Vector3.zero;
    private Vector3 queuedMoveLocation = Vector3.zero;

    float runSpeed {
        get { return player.runSpeed; }
    }

    void Awake() {
        RemoveBall();
    }

    // Use this for initialization
    void Start() {
        player = GetComponent<Player>();
        AssignTeam(player.team);
    }
	
    // Update is called once per frame
    public void Tick(float delta) {
        if (queuedMoveLocation != Vector3.zero) {
            targetMoveLocation = queuedMoveLocation;
            queuedMoveLocation = Vector3.zero;
        }

        if (targetMoveLocation != Vector3.zero) {
            RunStep(delta);
        }
    }

    public void AssignTeam(int teamId) {
        player.team = teamId;
        GetComponentInChildren<SpriteRenderer>().color = player.teamColor;
    }

    private void RunStep(float delta) {
        Vector3 diff = targetMoveLocation - gameObject.transform.position;

        if (diff.magnitude <= runSpeed * delta) {
            if (OnCompleteMove != null) {
                OnCompleteMove(targetMoveLocation);
            }

            targetMoveLocation = Vector3.zero;
        }
        else {
            diff = diff.normalized * (runSpeed * delta);
        }

        gameObject.transform.position = gameObject.transform.position + diff;
    }

    public void RunToPosition(Vector2 pos) {
        queuedMoveLocation = pos;
    }

    /// <summary>
    /// Calculates the time it would take to run to a position, taking direction change into account.
    /// </summary>
    /// <returns>The time to run to the position, in seconds</returns>
    /// <param name="pos">The position to run to</param>
    public float CalculateRunTime(Vector3 pos) {

        // TODO take into account acceleration and direction change
        Vector3 diff = pos - gameObject.transform.position;
        return diff.magnitude / runSpeed;
    }

    public void GiveBall() {
        heldBall.SetActive(true);
    }

    public void RemoveBall() {
        heldBall.SetActive(false);
    }
}

