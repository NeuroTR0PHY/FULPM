using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipAdjust : MonoBehaviour {
    public float y;
    public float z;
	// Use this for initialization
	void Start () {
        transform.localPosition = new Vector3(0, y, z);
	}
	
}
