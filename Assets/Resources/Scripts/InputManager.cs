using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

    private SwipeInput currentInput;
    private GameState game;

    private Vector2 throwStart = Vector2.zero;

    // Use this for initialization
    void Start() {
        game = FindObjectOfType<GameState>();

        if (!game) {
            throw new UnityException("Unable to locate game state");
        }

        currentInput = GetComponent<SwipeInput>();

        if (currentInput) {
            currentInput.OnInputStart += InputStart;
            currentInput.OnSelect += InputSelect;
            currentInput.OnThrow += InputThrow;
        }
        else {
            throw new UnityException("Unable to locate input");
        }
    }

    void InputStart(Vector2 pos) {
        game.TargetLocation(pos);
    }

    void InputSelect(Vector2 pos) {

    }

    void InputThrow(Vector2 throwNormal, float curve, float speed, float distance) {
        game.ThrowBall(throwNormal, curve, speed, distance);
    }
}

