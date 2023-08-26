using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPose : MonoBehaviour {
    public Vector3 shoulder;
    public Vector3 elbow;
    public Vector3 wrist;


    private void OnEnable()
    {
        transform.parent.GetComponent<DummyPoses>().PoseSwitch(gameObject);

    }
}
