using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceNewPoint : MonoBehaviour {
    public Transform A;
    public Transform B;
    public Transform C;

	
	// Update is called once per frame
	void Update () {
        C.position = Midpoint(A.position, B.position);
        C.LookAt(new Vector3(A.position.x, C.position.y, A.position.z));
        C.Rotate(0, 90, 0);
	}
    private Vector3 Midpoint(Vector3 a, Vector3 b)
    {
        return Vector3.Lerp(a, b, 0.5f);
    }

    private Vector3 Heading(Vector3 a, Vector3 b)
    {
        return a - b;
    }
}
