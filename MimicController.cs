using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicController : MonoBehaviour {
    public GameObject controller;
    public bool trackObject = true;

    private void Update()
    {
        MatchReference(controller.transform.position, controller.transform.rotation);
    }
    public void MatchReference(Vector3 referencePosition, Quaternion quat)
    {
        if (trackObject)
        {
            transform.position = transform.parent.position - referencePosition;
            transform.rotation = quat;
            //transform.eulerAngles = eulers;
        }

    }
}
