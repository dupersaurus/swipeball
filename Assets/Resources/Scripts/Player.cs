using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    [SerializeField]
    private int teamId = 0;

    public int team {
        get { return teamId; }
        set { teamId = value; }
    }

    public Color teamColor {
        get {
            switch (teamId) {
                default:
                    return new Color(1, 0, 0);

                case 1:
                    return new Color(0, 1, 1);
            }
        }
    }

    [SerializeField]
    private float maxSpeed = 2;

    public float runSpeed {
        get { return maxSpeed; }
    }

    [SerializeField]
    private float maxReach = 0.5f;

    public float reach {
        get { return maxReach; }
    }
}

