using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPoser : MonoBehaviour {
    public bool left;
    public GameObject dummy;
    
    [Header("Left Joints")]
    public List<GameObject> leftShoulder;
    public List<GameObject> leftElbow;
    public List<GameObject> leftWrist;

    [Header("Right Joints")]
    public List<GameObject> rightShoulder;
    public List<GameObject> rightElbow;
    public List<GameObject> rightWrist;

    [Header("Rotations")]
    public Vector3 shoulderRot;
    public Vector3 elbowRot;
    public Vector3 wristRot;

    private Quaternion[] leftQuat;
    private Quaternion[] rightQuat;

    [Header("Actions")]
    public bool pose;
    public bool reset;


    // Use this for initialization
    void Start () {
        leftQuat = new Quaternion[3];
        rightQuat = new Quaternion[3];

        leftQuat[0] = leftShoulder[0].transform.rotation;
        leftQuat[1] = leftElbow[0].transform.rotation;
        leftQuat[2] = leftWrist[0].transform.rotation;

        rightQuat[0] = rightShoulder[0].transform.rotation;
        rightQuat[1] = rightElbow[0].transform.rotation;
        rightQuat[2] = rightWrist[0].transform.rotation;

        //dummy = 
	}
	
	// Update is called once per frame
	void Update () {
        if (pose)
        {
            if (!left)
            {
                foreach(GameObject ls in leftShoulder) ls.transform.Rotate(new Vector3(shoulderRot.x * -1, shoulderRot.y * -1, shoulderRot.z));
                foreach(GameObject le in leftElbow) le.transform.Rotate(new Vector3(elbowRot.x * -1, elbowRot.y * -1, elbowRot.z));
                foreach(GameObject lw in leftWrist) lw.transform.Rotate(new Vector3(wristRot.x * -1, wristRot.y * -1, wristRot.z));
            }
            else
            {
                foreach(GameObject rs in rightShoulder) rs.transform.Rotate(shoulderRot);
                foreach(GameObject re in rightElbow) re.transform.Rotate(elbowRot);
                foreach(GameObject rw in rightWrist) rw.transform.Rotate(wristRot);
            }
            pose = false;
        }

        if (reset)
        {
            ReturnRotation();
            reset = false;
        }
	}

    public void ReturnRotation()
    {
        foreach (GameObject ls in leftShoulder) ls.transform.rotation = leftQuat[0];
        foreach (GameObject le in leftElbow) le.transform.rotation = leftQuat[1];
        foreach (GameObject lw in leftWrist) lw.transform.rotation = leftQuat[2];

        foreach (GameObject rs in rightShoulder) rs.transform.rotation = rightQuat[0];
        foreach (GameObject re in rightElbow) re.transform.rotation = rightQuat[1];
        foreach (GameObject rw in rightWrist) rw.transform.rotation = rightQuat[2];
    }

    public void ArmSideRotation()
    {
        ReturnRotation();
        foreach (GameObject ls in leftShoulder)
        {
            ls.transform.rotation = leftQuat[0];
            ls.transform.Rotate(0, -85, 0);

        }

        foreach (GameObject le in leftElbow)
        {
           // le.transform.rotation = leftQuat[1];
        }

        foreach (GameObject lw in leftWrist)
        {
            lw.transform.Rotate(90, 0, 0);
        }
        foreach (GameObject rs in rightShoulder)
        {
            rs.transform.rotation = rightQuat[0];
            rs.transform.Rotate(0, 85, 0);
        }


        foreach (GameObject re in rightElbow)
        {
            //re.transform.rotation = rightQuat[1];
        }

        foreach (GameObject rw in rightWrist)
        {
            rw.transform.Rotate(-90, 0, 0);
        }
    }

    public void SpecificArmSideRotation(bool left)
    {
        if (left)
        {
            foreach (GameObject ls in leftShoulder)
            {
                ls.transform.rotation = leftQuat[0];
                ls.transform.Rotate(0, -85, 0);

            }

            foreach (GameObject le in leftElbow)
            {
               // le.transform.rotation = leftQuat[1];
            }

            foreach (GameObject lw in leftWrist)
            {
                lw.transform.Rotate(90, 0, 0);
            }
        }
        else
        {
            foreach (GameObject rs in rightShoulder)
            {
                rs.transform.rotation = rightQuat[0];
                rs.transform.Rotate(0, 85, 0);
            }


            foreach (GameObject re in rightElbow)
            {
               // re.transform.rotation = rightQuat[1];
            }

            foreach (GameObject rw in rightWrist)
            {
                rw.transform.Rotate(-90, 0, 0);
            }
        }
    }
}
