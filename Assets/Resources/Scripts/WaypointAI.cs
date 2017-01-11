using UnityEngine;
using System.Collections;

public class WaypointAI : AI {

    private Character controller;

    [SerializeField]
    private Vector2[] waypoints;

    private int waypointIndex = 0;

    // Use this for initialization
    void Start() {
        controller = GetComponent<Character>();
        controller.OnCompleteMove += CompleteMove;

        controller.RunToPosition(waypoints[0]);
    }

    void CompleteMove(Vector2 pos) {
        waypointIndex++;

        if (waypointIndex > waypoints.Length) {
            waypointIndex = 0;
        }

        controller.RunToPosition(waypoints[waypointIndex]);
    }
}

