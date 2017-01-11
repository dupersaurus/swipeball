using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInput : MonoBehaviour {

	public delegate void TapStart(Vector2 pos);
	public delegate void TapSelect(Vector2 pos);
    public delegate void ThrowResponse(Vector2 throwNormal, float curve, float speed, float distance); 

	/// <summary>Distance to travel before establishing the throw, in units</summary>
	[SerializeField]
	private float _motionThreshold = 0.5f;

	/// <summary>Minimal angle before registering curve</summary>
	[SerializeField]
	private float _curveDeadzone = 5.0f;

	/// <summary>Scale to judge power with, in seconds</summary>
	[SerializeField]
	private float _powerScale = 2.0f;

	public event TapStart OnInputStart;
	public event TapSelect OnSelect;
	public event ThrowResponse OnThrow;

	private bool _isTouchDown = false;

	private Vector2 _throwStart;
	private Vector2 _launchVector;
	private Vector2 _netMotion;
	private float _totalMotion;
    private float _swipeTime;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		bool touchBegin = Input.GetMouseButtonDown (0);
		bool touchEnd = Input.GetMouseButtonUp (0);

        Vector2 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);

		if (!_isTouchDown && touchBegin) {
			StartThrow (pos);
		} else if (_isTouchDown) {
            _swipeTime += Time.deltaTime;

			if (touchEnd) {
				EndThrow (pos);
			} else {
				DragThrow (pos);
			}
		}
	}

	private void StartThrow(Vector2 pos) {
		_netMotion = Vector2.zero;
		_launchVector = Vector2.zero;
		_totalMotion = 0;
		_isTouchDown = true;
		_throwStart = pos;
        _swipeTime = 0;

		OnInputStart(pos);
	}

	private void EndThrow(Vector2 pos) {
        _isTouchDown = false;

		// No swipe
		if (_launchVector == Vector2.zero) {
			OnSelect(_throwStart);
		}
		else {
			float angle = Vector2.Angle(_launchVector, _netMotion);

			if (angle < _curveDeadzone) {
				angle = 0;
			}

            Vector3 cross = Vector3.Cross(_launchVector, _netMotion);

            if (cross.z < 0) {
                angle *= -1;
            }

            float velocity = _totalMotion / _swipeTime;
            OnThrow(_launchVector, angle, velocity, _totalMotion);
		}
	}

	private void DragThrow(Vector2 pos) {
		Vector2 delta = new Vector2 (Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"));
		_netMotion += delta;
		_totalMotion += delta.sqrMagnitude;

		// Determine the launch vector
		if (_launchVector == Vector2.zero && _netMotion.sqrMagnitude >= (_motionThreshold * _motionThreshold)) {
			_launchVector = _netMotion;
		}
	}
}
