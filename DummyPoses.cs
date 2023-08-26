using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPoses : MonoBehaviour {
    public GameObject activePose;


    private DummyPoser dPoser;

    public DummyRotator dRot;

    [Header("Pose Lists")]
    public List<GameObject> randomPoses;
	// Use this for initialization
	void Start () {
        dPoser = GameObject.Find("GameManager").GetComponent<DummyPoser>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.P))
        {
            TogglePoseDebug();
        }
	}

    public void PoseSwitch(GameObject go)
    {
        if(activePose != null)
        {
            if(activePose != go)
            {
                activePose.SetActive(false);
            }
        }
        activePose = go;
        dRot.ResetRotation();
        Pose();

    }

    private void Pose()
    {
        dPoser.ReturnRotation();
        DummyPose dp = activePose.GetComponent<DummyPose>();
        dPoser.shoulderRot = dp.shoulder;
        dPoser.elbowRot = dp.elbow;
        dPoser.wristRot = dp.wrist;
        dPoser.pose = true;

    }

    public void RollPoses()
    {
        randomPoses = new List<GameObject>();
        List<DummyPose> trs = new List<DummyPose>();
        trs.AddRange(GetComponentsInChildren<DummyPose>(true));
        for(int e = 0; e < trs.Count; e++)
        {
            randomPoses.Add(trs[e].gameObject);
        }

        int n = randomPoses.Count;
        GameObject myGO;
        System.Random _random = new System.Random();

        for (int i = 0; i < n; i++)
        {
            int r = i + (int)(_random.NextDouble() * (n - i));
            myGO = randomPoses[r];
            randomPoses[r] = randomPoses[i];
            randomPoses[i] = myGO;
        }
     
    }

    public void GetPosesDebug()
    {
        randomPoses = new List<GameObject>();
        List<DummyPose> trs = new List<DummyPose>();
        trs.AddRange(GetComponentsInChildren<DummyPose>(true));
        for (int e = 0; e < trs.Count; e++)
        {
            randomPoses.Add(trs[e].gameObject);
        }
    }

    private int count = 0;
    private void TogglePoseDebug()
    {
        if (randomPoses.Count == 0)
        {
            GetPosesDebug();
        }

        activePose = randomPoses[count];
        Pose();
        count++;
        if (count == randomPoses.Count)
        {
            count = 0;
            GetComponent<GameControl>().assignment.text = "finished all poses";
            GetComponent<GameControl>().textClear(2);
        }


    }


}
