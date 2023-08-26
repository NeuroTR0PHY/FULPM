using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRotator : MonoBehaviour {
    public float speedMultiplier;
    private Quaternion rot;
   // public bool invert;
  //  public Vector2 minMax;
	// Use this for initialization
	void Start () {
        rot = transform.rotation;	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.LeftArrow))
        {

            transform.Rotate(new Vector3(0, Time.deltaTime * speedMultiplier, 0));
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0, (Time.deltaTime * speedMultiplier) * -1, 0));
        }
    }

    public void ResetRotation()
    {
        transform.rotation = rot;
    }
}
