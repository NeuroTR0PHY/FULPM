using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;
using TMPro;
using System.IO;

public class GameControl : MonoBehaviour
{

    public bool assigning;
    public bool checkAssignment;

    private float resetCountDuration = 2;
    private float counter;
    private bool resetting;

    [HideInInspector]
    public bool assignmentComplete;
    //private bool track;

    [Header("Vive Trackers")]
    public GameObject sternum;
    public GameObject T1;
    public GameObject T2;
    public GameObject leftBicep;
    public GameObject rightBicep;
    public GameObject leftForearm;
    public GameObject rightForearm;

    [Header("Chair Static Markers")]
    public GameObject staticPrefab;
    public GameObject centroid;
    public GameObject movingCentroid;
    public List<GameObject> staticPoints;
    private bool assigningChair;

    [Header("Vive Tracker Bodies")]
    public List<GameObject> bodies;

    [Header("Vive Tracker Controllers")]
    public List<GameObject> controllers;

    public enum Trackers { unassigned, T1, T2, sternum, leftBicep, rightBicep, leftForearm, rightForearm }
    public Trackers trackerToAssign;

    [Header("Left Controller")]
    public GameObject leftController;
    public SteamVR_Input_Sources leftHandType;
    public SteamVR_Action_Boolean leftHandAction;
    public GameObject leftTip;

    [Header("Right Controller")]
    public GameObject rightController;
    public SteamVR_Input_Sources rightHandType;
    public SteamVR_Action_Boolean rightHandAction;
    public GameObject rightTip;

    [Header("Graphics")]
    public TextMeshPro assignment;
    public TextMeshPro sternumTMP;
    public TextMeshPro HMDTMP;
    public TextMeshPro LeftRawTMP;
    public TextMeshPro RightRawTMP;
    public TextMeshPro Angles;
    public TextMeshPro Task;
    public TextMeshPro Help;

    [Header("Reference")]
    public GameObject referenceSternum;
    public GameObject referenceSternumRot;
    public GameObject referenceHMD;
    public GameObject referenceHMDRot;
    public GameObject referenceBicepAvg;
    public GameObject referenceForearmAvg;
    public GameObject referenceDos;
    public GameObject referenceTres;
    public GameObject referenceQuatro;
    public GameObject HMD;
    public GameObject CameraRig;

    [Header("Dummy Posers")]
    public GameObject leftPoser;
    public GameObject rightPoser;

    public enum Movement { Passive, Active }
    public enum Reference { Contralateral, Memory }

    [Header("Task Parameters")]
    public int trialsPerCondition;
    public int currentTrial;
    public int totalOngoingTrials;
    public float sampleDuration = 2f;
    public enum SideTested { Left, Right }
    public SideTested sideTested;
    public Movement movement;
    public Reference reference;
    public List<int> handOrder;
    public bool testing = false;

    [Header("Participant Data")]
    public string pID;
    public int sessionNum;

    [Header("Debug")]
    public bool debugTrackers;
    public GameObject trainingLight;
    public List<GameObject> debugObjects;
    public enum TrackerDistDebug { AB, SternumRef }
    public TrackerDistDebug distDebug;
    public bool distanceDebuging;
    public enum ReferenceSternumType { TransformOnly, WithRotation }
    public ReferenceSternumType sternumType;

    // Use this for initialization
    void Start()
    {
        textClear();
        DebugTextClear();
    }

    // Update is called once per frame
    void Update()
    {

        if (!assigning && !assigningChair)
        {
            if (!assignmentComplete)
            {
                if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.PageUp)) { assigning = true; TrackerEmptyCheck(); }
                if (Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.PageDown)) { checkAssignment = true; }
            }




            //if (Input.GetKeyUp(KeyCode.S)) { assigningChair = true; }
        }
        if (assigning)
        {
            if (RightCheck())
            {
                Assign(rightController);
            }
            if (LeftCheck())
            {
                Assign(leftController);
            }

        }

        if (checkAssignment)
        {
            if (RightCheck())
            {
                CancelInvoke();
                assignment.text = Closest(rightController).GetComponent<AnatomicalTracker>().trackerAssignment.ToString();
                checkAssignment = false;
                Invoke("textClear", 2);
            }
            if (LeftCheck())
            {
                CancelInvoke();

                assignment.text = Closest(leftController).GetComponent<AnatomicalTracker>().trackerAssignment.ToString();
                checkAssignment = false;
                Invoke("textClear", 2);
            }
        }

        if (assigningChair)
        {
            if (RightCheck())
            {
                // Assign(rightController);
            }
            if (LeftCheck())
            {
                //Assign(leftController);
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            resetting = true;
            counter += Time.deltaTime;
            //Debug.Log(counter);
            if (counter >= resetCountDuration) { resetAssignments(); assignmentComplete = false; }

        }
        if (Input.GetKeyUp(KeyCode.R)) { counter = 0; resetting = false; }

        if (Input.GetKeyDown(KeyCode.T)) { ToggleTrackers(); }
        //if (track) FitCheck();
        if (Input.GetKeyDown(KeyCode.Escape)) { HelpMenu(); }
        if (debugTrackers)
        {
            DebugTrackers();
            debugTrackers = false;
            assignmentComplete = true;
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            ControllerFind(0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            ControllerFind(1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            ControllerFind(2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            ControllerFind(3);
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            ControllerFind(4);
        }
        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            ControllerFind(5);
        }
        if (Input.GetKeyUp(KeyCode.Alpha7))
        {
            ControllerFind(6);
        }

        if (distanceDebuging) DebugDistance(distDebug);


        if (avgRefMeasure)
        {
            AveragePositionsBilateral avg = new AveragePositionsBilateral();
            avg.left = leftBicep.transform.position;
            avg.right = rightBicep.transform.position;
            bicepPs.Add(avg);

            AveragePositionsBilateral avg2 = new AveragePositionsBilateral();
            avg2.left = leftForearm.transform.position;
            avg2.right = rightForearm.transform.position;
            forearmPs.Add(avg2);

            sternumPs.Add(sternum.transform.position);
            HMDPs.Add(HMD.transform.position);
        }
    }


    private Vector3 AverageLeft(List<AveragePositionsBilateral> list)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        foreach (AveragePositionsBilateral v3 in list)
        {
            x += v3.left.x;
            y += v3.left.y;
            z += v3.left.z;
        }
        x = x / list.Count;
        y = y / list.Count;
        z = z / list.Count;
        return new Vector3(x, y, z);
    }

    private Vector3 AverageRight(List<AveragePositionsBilateral> list)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        foreach (AveragePositionsBilateral v3 in list)
        {
            x += v3.right.x;
            y += v3.right.y;
            z += v3.right.z;
        }
        x = x / list.Count;
        y = y / list.Count;
        z = z / list.Count;
        return new Vector3(x, y, z);
    }

    private Vector3 AverageUnilateral(List<Vector3> list)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        foreach (Vector3 v3 in list)
        {
            x += v3.x;
            y += v3.y;
            z += v3.z;
        }
        x = x / list.Count;
        y = y / list.Count;
        z = z / list.Count;
        return new Vector3(x, y, z);
    }

    public void DebugDistance(TrackerDistDebug trackerDist)
    {
        string text = "";
        if (trackerDist == TrackerDistDebug.AB)
        {
            text = Vector3.Distance(debugObjects[0].transform.position, debugObjects[1].transform.position).ToString();
        }
        if (trackerDist == TrackerDistDebug.SternumRef)
        {
            PlaceReference();
            if (sternumType == ReferenceSternumType.TransformOnly)
            {
                Vector3 lcont_refSternum = referenceSternum.transform.InverseTransformPoint(debugObjects[0].transform.position);
                Vector3 rcont_refSternum = referenceSternum.transform.InverseTransformPoint(debugObjects[1].transform.position);

                lcont_refSternum = new Vector3(Mathf.Abs(lcont_refSternum.x), lcont_refSternum.y, lcont_refSternum.z);
                rcont_refSternum = new Vector3(Mathf.Abs(rcont_refSternum.x), rcont_refSternum.y, rcont_refSternum.z);
                text = lcont_refSternum + "and" + rcont_refSternum + "and" + Vector3.Distance(lcont_refSternum, rcont_refSternum);
            }
            if (sternumType == ReferenceSternumType.WithRotation)
            {
                Vector3 lcont_refSternum = referenceSternumRot.transform.InverseTransformPoint(debugObjects[0].transform.position);
                Vector3 rcont_refSternum = referenceSternumRot.transform.InverseTransformPoint(debugObjects[1].transform.position);

                lcont_refSternum = new Vector3(Mathf.Abs(lcont_refSternum.x), lcont_refSternum.y, lcont_refSternum.z);
                rcont_refSternum = new Vector3(Mathf.Abs(rcont_refSternum.x), rcont_refSternum.y, rcont_refSternum.z);
                text = lcont_refSternum + "and" + rcont_refSternum + "and" + Vector3.Distance(lcont_refSternum, rcont_refSternum);
            }
        }
        Debug.Log(text);
    }

    private void ControllerFind(int cIndex)
    {
        GameObject c = controllers[cIndex];
        if ((int)c.GetComponent<SteamVR_RenderModel>().index >= 15)
        {
            c.GetComponent<SteamVR_RenderModel>().index = (SteamVR_TrackedObject.EIndex)1;
            c.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)1;
            c.GetComponent<SteamVR_RenderModel>().enabled = false;
            c.GetComponent<SteamVR_RenderModel>().enabled = true;
            Debug.Log(1 + "and" + (SteamVR_TrackedObject.EIndex)1);
        }
        else
        {
            int i = (int)c.GetComponent<SteamVR_RenderModel>().index;
            i++;
            c.GetComponent<SteamVR_RenderModel>().index = (SteamVR_TrackedObject.EIndex)i;
            c.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)i;
            c.GetComponent<SteamVR_RenderModel>().enabled = false;
            c.GetComponent<SteamVR_RenderModel>().enabled = true;
            Debug.Log(i + "and" + (SteamVR_TrackedObject.EIndex)i);
        }

    }

    public bool RightCheck()
    {
        return rightHandAction.GetState(rightHandType);
    }

    public bool LeftCheck()
    {
        return leftHandAction.GetState(leftHandType);
    }

    private void HelpMenu()
    {
        Help.gameObject.SetActive(!Help.gameObject.activeSelf);
    }

    private void TrackerEmptyCheck()
    {

        if (sternum == null) trackerToAssign = Trackers.sternum;
        else if (T1 == null) trackerToAssign = Trackers.T1;
        else if (T2 == null) trackerToAssign = Trackers.T2;
        else if (leftBicep == null) trackerToAssign = Trackers.leftBicep;
        else if (rightBicep == null) trackerToAssign = Trackers.rightBicep;
        else if (leftForearm == null) trackerToAssign = Trackers.leftForearm;
        else if (rightForearm == null) trackerToAssign = Trackers.rightForearm;
        else { assigning = false; textClear(); assignmentComplete = true; }

        if (assigning) updateText();
    }

    private void Assign(GameObject controller)
    {
        //TrackerEmptyCheck();
        Collider[] hitColliders = Physics.OverlapSphere(controller.transform.position, 1f);
        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
            gos.Add(hitColliders[i].gameObject);
        }
        gos = gos.OrderBy(x => Vector3.Distance(controller.transform.position, x.transform.position)).ToList();

        if (trackerToAssign == Trackers.sternum) { sternum = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.sternum; }
        if (trackerToAssign == Trackers.T1) { T1 = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.T1; }
        if (trackerToAssign == Trackers.T2) { T2 = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.T2; }
        if (trackerToAssign == Trackers.leftBicep) { leftBicep = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.leftBicep; }
        if (trackerToAssign == Trackers.rightBicep) { rightBicep = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.rightBicep; }
        if (trackerToAssign == Trackers.leftForearm) { leftForearm = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.leftForearm; }
        if (trackerToAssign == Trackers.rightForearm) { rightForearm = gos[0]; gos[0].GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.rightForearm; }
        assigning = false;
        textClear();
    }

    private void AssignChair(GameObject controller)
    {
        //chairNumber++; -.0754 and 0.0382
        //Vector3 pos = 
        // if(chairNumber == 1) 
    }

    private GameObject Closest(GameObject controller)
    {
        Collider[] hitColliders = Physics.OverlapSphere(controller.transform.position, 1f);
        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
            gos.Add(hitColliders[i].gameObject);
        }
        gos = gos.OrderBy(x => Vector3.Distance(controller.transform.position, x.transform.position)).ToList();
        return gos[0];
    }

    private void updateText()
    {
        assignment.text = trackerToAssign.ToString();
    }

    private void textClear()
    {
        assignment.text = "";
    }

    public void textClear(float seconds)
    {
        Invoke("textClear", seconds);
    }
    private void DebugTextClear()
    {
        HMDTMP.text = "";
        LeftRawTMP.text = "";
        RightRawTMP.text = "";
        sternumTMP.text = "";
        Angles.text = "";
    }

    public bool CheckAssignments()
    {
        if (sternum != null && T1 != null && leftBicep != null && leftForearm != null && T2 != null && rightBicep != null && rightForearm != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void resetAssignments()
    {
        if (sternum != null)
        {
            sternum.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            sternum = null;
        }
        if (T1 != null)
        {
            T1.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            T1 = null;
        }
        if (T2 != null)
        {
            T2.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            T2 = null;
        }
        if (leftBicep != null)
        {
            leftBicep.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            leftBicep = null;
        }
        if (rightBicep != null)
        {
            rightBicep.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            rightBicep = null;
        }
        if (leftForearm != null)
        {
            leftForearm.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            leftForearm = null;
        }
        if (rightForearm != null)
        {
            rightForearm.GetComponent<AnatomicalTracker>().trackerAssignment = Trackers.unassigned;
            rightForearm = null;
        }


        assignment.text = "Assignments Reset";
        Invoke("textClear", 2f);
    }



    //////////////////////////
    ///solution: place a reference object half way between the headset and the sternum. Make sure this objects z is facing perpindicular to the world z. 
    ///Take the absolute difference for each tracker pair on the x and then the regular value on y and z (up/down and forward/back, respectively).
    ///Take the distance between these altered vectors. That is the disparity between the two trackers.
    private StreamWriter distPerFramewriter;
    private StreamWriter kinWriter;
    private StreamWriter relativeKinWriter;
    private StreamWriter avgLocWriter;

    //Record a second of left and right bicep location and average the location for each.
    //Position a reference perpindicular at the midpoint line between the left and right bicep markers.
    //record absolute location for all markers on every trial. !!!

    //Must record kinematic for memory of all positions (A, B , and Return), must change the writer output for memory trials to compare A with return
    //to do so add a bool in the method which determines whether to record for contralateral or memory trials
    //create enum with A, B and return and swap through them in the memory trials. record those entries to a new column
    //on memory trials with return recording log writer and kinematic writer, else log just the kinematicwriter.
    //must gather avg for A and Return, then perform inversetransform 
    public enum MemoryPosition { A, B, Return, NotMemory }
    public MemoryPosition memoryPosition;

    //Average distances from the sternum rotation reference
    float forearmDistanceSternum;
    float bicepDistanceSternum;
    float controllerDistance;

    //Average distances from the bicep average reference
    float forearmDistanceBicepAvg;
    float bicepDistanceBicepAvg;
    float controllerDistanceBicepAvg;


    //average distances from the forearm average reference
    float forearmDistanceForearmAvg;
    float bicepDistanceForearmAvg;
    float controllerDistanceForearmAvg;

    //average distances from the forearm + bicep average reference
    float forearmDistanceDosAvg;
    float bicepDistanceDosAvg;
    float controllerDistanceDosAvg;

    //average distances from the forearm + bicep + sternum average reference
    float forearmDistanceTresAvg;
    float bicepDistanceTresAvg;
    float controllerDistanceTresAvg;

    //average distances from the forearm + bicep + sternum + HMD average reference
    float forearmDistanceQuatroAvg;
    float bicepDistanceQuatroAvg;
    float controllerDistanceQuatroAvg;

    //lists of relative locations to each reference for each tracker
    List<Vector3> l_controller_sternumRot;
    List<Vector3> r_controller_sternumRot;
    List<Vector3> l_controller_bicepAvg;
    List<Vector3> r_controller_bicepAvg;
    List<Vector3> l_controller_forearmAvg;
    List<Vector3> r_controller_forearmAvg;
    List<Vector3> l_controller_dos;
    List<Vector3> r_controller_dos;
    List<Vector3> l_controller_tres;
    List<Vector3> r_controller_tres;
    List<Vector3> l_controller_quatro;
    List<Vector3> r_controller_quatro;

    List<Vector3> l_bicep_sternumRot;
    List<Vector3> r_bicep_sternumRot;
    List<Vector3> l_bicep_bicepAvg;
    List<Vector3> r_bicep_bicepAvg;
    List<Vector3> l_bicep_forearmAvg;
    List<Vector3> r_bicep_forearmAvg;
    List<Vector3> l_bicep_dos;
    List<Vector3> r_bicep_dos;
    List<Vector3> l_bicep_tres;
    List<Vector3> r_bicep_tres;
    List<Vector3> l_bicep_quatro;
    List<Vector3> r_bicep_quatro;

    List<Vector3> l_forearm_sternumRot;
    List<Vector3> r_forearm_sternumRot;
    List<Vector3> l_forearm_bicepAvg;
    List<Vector3> r_forearm_bicepAvg;
    List<Vector3> l_forearm_forearmAvg;
    List<Vector3> r_forearm_forearmAvg;
    List<Vector3> l_forearm_dos;
    List<Vector3> r_forearm_dos;
    List<Vector3> l_forearm_tres;
    List<Vector3> r_forearm_tres;
    List<Vector3> l_forearm_quatro;
    List<Vector3> r_forearm_quatro;

    private void ResetRelativeLocationLists()
    {
        l_controller_sternumRot = new List<Vector3>();
        r_controller_sternumRot = new List<Vector3>();
        l_controller_bicepAvg = new List<Vector3>();
        r_controller_bicepAvg = new List<Vector3>();
        l_controller_forearmAvg = new List<Vector3>();
        r_controller_forearmAvg = new List<Vector3>();
        l_controller_dos = new List<Vector3>();
        r_controller_dos = new List<Vector3>();
        l_controller_tres = new List<Vector3>();
        r_controller_tres = new List<Vector3>();
        l_controller_quatro = new List<Vector3>();
        r_controller_quatro = new List<Vector3>();

        l_bicep_sternumRot = new List<Vector3>();
        r_bicep_sternumRot = new List<Vector3>();
        l_bicep_bicepAvg = new List<Vector3>();
        r_bicep_bicepAvg = new List<Vector3>();
        l_bicep_forearmAvg = new List<Vector3>();
        r_bicep_forearmAvg = new List<Vector3>();
        l_bicep_dos = new List<Vector3>();
        r_bicep_dos = new List<Vector3>();
        l_bicep_tres = new List<Vector3>();
        r_bicep_tres = new List<Vector3>();
        l_bicep_quatro = new List<Vector3>();
        r_bicep_quatro = new List<Vector3>();

        l_forearm_sternumRot = new List<Vector3>();
        r_forearm_sternumRot = new List<Vector3>();
        l_forearm_bicepAvg = new List<Vector3>();
        r_forearm_bicepAvg = new List<Vector3>();
        l_forearm_forearmAvg = new List<Vector3>();
        r_forearm_forearmAvg = new List<Vector3>();
        l_forearm_dos = new List<Vector3>();
        r_forearm_dos = new List<Vector3>();
        l_forearm_tres = new List<Vector3>();
        r_forearm_tres = new List<Vector3>();
        l_forearm_quatro = new List<Vector3>();
        r_forearm_quatro = new List<Vector3>();
    }

    //locations for postures in memory block
    List<Vector3> POSAs_bicep_l;
    List<Vector3> ReturnPOSs_bicep_l;
    List<Vector3> POSAs_forearm_l;
    List<Vector3> ReturnPOSs_forearm_l;
    List<Vector3> POSAs_controller_l;
    List<Vector3> ReturnPOSs_controller_l;

    List<Vector3> POSAs_bicep_r;
    List<Vector3> ReturnPOSs_bicep_r;
    List<Vector3> POSAs_forearm_r;
    List<Vector3> ReturnPOSs_forearm_r;
    List<Vector3> POSAs_controller_r;
    List<Vector3> ReturnPOSs_controller_r;

    public float FitCheck(bool memory, bool returnRecording)
    {   if(l_controller_sternumRot == null) { ResetRelativeLocationLists(); }
        PlaceReference();
        Vector3 lfrPos_refSternum = referenceSternumRot.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refSternum = referenceSternumRot.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refSternum = referenceSternumRot.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refSternum = referenceSternumRot.transform.InverseTransformPoint(rightBicep.transform.position);
        Vector3 lcont_refSternum = referenceSternumRot.transform.InverseTransformPoint(leftController.transform.position);
        Vector3 rcont_refSternum = referenceSternumRot.transform.InverseTransformPoint(rightController.transform.position);

        //leftPoser.GetComponent<MimicController>().MatchReference(lcont_refSternum, leftController.transform.rotation);
        //rightPoser.GetComponent<MimicController>().MatchReference(rcont_refSternum, rightController.transform.rotation);


        lfrPos_refSternum = new Vector3(Mathf.Abs(lfrPos_refSternum.x), lfrPos_refSternum.y, lfrPos_refSternum.z);
        rfrPos_refSternum = new Vector3(Mathf.Abs(rfrPos_refSternum.x), rfrPos_refSternum.y, rfrPos_refSternum.z);
        lbcPos_refSternum = new Vector3(Mathf.Abs(lbcPos_refSternum.x), lbcPos_refSternum.y, lbcPos_refSternum.z);
        rbcPos_refSternum = new Vector3(Mathf.Abs(rbcPos_refSternum.x), rbcPos_refSternum.y, rbcPos_refSternum.z);
        lcont_refSternum = new Vector3(Mathf.Abs(lcont_refSternum.x), lcont_refSternum.y, lcont_refSternum.z);
        rcont_refSternum = new Vector3(Mathf.Abs(rcont_refSternum.x), rcont_refSternum.y, rcont_refSternum.z);

        l_forearm_sternumRot.Add(lfrPos_refSternum);
        r_forearm_sternumRot.Add(rfrPos_refSternum);
        l_bicep_sternumRot.Add(lbcPos_refSternum);
        r_bicep_sternumRot.Add(rbcPos_refSternum);
        l_controller_sternumRot.Add(lcont_refSternum);
        r_controller_sternumRot.Add(rcont_refSternum);

        forearmDistanceSternum = Vector3.Distance(lfrPos_refSternum, rfrPos_refSternum) / 2;
        bicepDistanceSternum = Vector3.Distance(lbcPos_refSternum, rbcPos_refSternum) / 2;
        controllerDistance = Vector3.Distance(lcont_refSternum, rcont_refSternum) / 2;


        Vector3 lfrPos_refBicAvg = referenceBicepAvg.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refBicAvg = referenceBicepAvg.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refBicAvg = referenceBicepAvg.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refBicAvg = referenceBicepAvg.transform.InverseTransformPoint(rightBicep.transform.position);
        Vector3 lcont_refBicAvg = referenceBicepAvg.transform.InverseTransformPoint(leftController.transform.position);
        Vector3 rcont_refBicAvg = referenceBicepAvg.transform.InverseTransformPoint(rightController.transform.position);

        lfrPos_refBicAvg = new Vector3(Mathf.Abs(lfrPos_refBicAvg.x), lfrPos_refBicAvg.y, lfrPos_refBicAvg.z);
        rfrPos_refBicAvg = new Vector3(Mathf.Abs(rfrPos_refBicAvg.x), rfrPos_refBicAvg.y, rfrPos_refBicAvg.z);
        lbcPos_refBicAvg = new Vector3(Mathf.Abs(lbcPos_refBicAvg.x), lbcPos_refBicAvg.y, lbcPos_refBicAvg.z);
        rbcPos_refBicAvg = new Vector3(Mathf.Abs(rbcPos_refBicAvg.x), rbcPos_refBicAvg.y, rbcPos_refBicAvg.z);
        lcont_refBicAvg = new Vector3(Mathf.Abs(lcont_refBicAvg.x), lcont_refBicAvg.y, lcont_refBicAvg.z);
        rcont_refBicAvg = new Vector3(Mathf.Abs(rcont_refBicAvg.x), rcont_refBicAvg.y, rcont_refBicAvg.z);

        l_controller_bicepAvg.Add(lcont_refBicAvg);
        r_controller_bicepAvg.Add(rcont_refBicAvg);
        l_bicep_bicepAvg.Add(lbcPos_refBicAvg);
        r_bicep_bicepAvg.Add(rbcPos_refBicAvg);
        l_forearm_bicepAvg.Add(lfrPos_refBicAvg);
        r_forearm_bicepAvg.Add(rfrPos_refBicAvg);

        
        forearmDistanceBicepAvg = (Vector3.Distance(lfrPos_refBicAvg, rfrPos_refBicAvg) / 2);
        bicepDistanceBicepAvg = Vector3.Distance(lbcPos_refBicAvg, rbcPos_refBicAvg) / 2;
        controllerDistanceBicepAvg = Vector3.Distance(lcont_refBicAvg, rcont_refBicAvg) / 2;

        Vector3 lfrPos_refForAvg = referenceForearmAvg.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refForAvg = referenceForearmAvg.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refForAvg = referenceForearmAvg.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refForAvg = referenceForearmAvg.transform.InverseTransformPoint(rightBicep.transform.position);
        Vector3 lcont_refForAvg = referenceForearmAvg.transform.InverseTransformPoint(leftController.transform.position);
        Vector3 rcont_refForAvg = referenceForearmAvg.transform.InverseTransformPoint(rightController.transform.position);

        lfrPos_refForAvg = new Vector3(Mathf.Abs(lfrPos_refForAvg.x), lfrPos_refForAvg.y, lfrPos_refForAvg.z);
        rfrPos_refForAvg = new Vector3(Mathf.Abs(rfrPos_refForAvg.x), rfrPos_refForAvg.y, rfrPos_refForAvg.z);
        lbcPos_refForAvg = new Vector3(Mathf.Abs(lbcPos_refForAvg.x), lbcPos_refForAvg.y, lbcPos_refForAvg.z);
        rbcPos_refForAvg = new Vector3(Mathf.Abs(rbcPos_refForAvg.x), rbcPos_refForAvg.y, rbcPos_refForAvg.z);
        lcont_refForAvg = new Vector3(Mathf.Abs(lcont_refForAvg.x), lcont_refForAvg.y, lcont_refForAvg.z);
        rcont_refForAvg = new Vector3(Mathf.Abs(rcont_refForAvg.x), rcont_refForAvg.y, rcont_refForAvg.z);

        l_controller_forearmAvg.Add(lcont_refForAvg);
        r_controller_forearmAvg.Add(rcont_refForAvg);
        l_bicep_forearmAvg.Add(lfrPos_refForAvg);
        r_bicep_forearmAvg.Add(rfrPos_refForAvg);
        l_forearm_forearmAvg.Add(lfrPos_refForAvg);
        r_forearm_forearmAvg.Add(rfrPos_refForAvg);
       

        forearmDistanceForearmAvg = Vector3.Distance(lfrPos_refForAvg, rfrPos_refForAvg) / 2;
        bicepDistanceForearmAvg = Vector3.Distance(lbcPos_refForAvg, rbcPos_refForAvg) / 2;
        controllerDistanceForearmAvg = Vector3.Distance(lcont_refForAvg, rcont_refForAvg) / 2;

        Vector3 lfrPos_refDos = referenceDos.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refDos = referenceDos.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refDos = referenceDos.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refDos = referenceDos.transform.InverseTransformPoint(rightBicep.transform.position);
        Vector3 lcont_refDos = referenceDos.transform.InverseTransformPoint(leftController.transform.position);
        Vector3 rcont_refDos = referenceDos.transform.InverseTransformPoint(rightController.transform.position);

        lfrPos_refDos = new Vector3(Mathf.Abs(lfrPos_refDos.x), lfrPos_refDos.y, lfrPos_refDos.z);
        rfrPos_refDos = new Vector3(Mathf.Abs(rfrPos_refDos.x), rfrPos_refDos.y, rfrPos_refDos.z);
        lbcPos_refDos = new Vector3(Mathf.Abs(lbcPos_refDos.x), lbcPos_refDos.y, lbcPos_refDos.z);
        rbcPos_refDos = new Vector3(Mathf.Abs(rbcPos_refDos.x), rbcPos_refDos.y, rbcPos_refDos.z);
        lcont_refDos = new Vector3(Mathf.Abs(lcont_refDos.x), lcont_refDos.y, lcont_refDos.z);
        rcont_refDos = new Vector3(Mathf.Abs(rcont_refDos.x), rcont_refDos.y, rcont_refDos.z);

        l_controller_dos.Add(lcont_refDos);
        r_controller_dos.Add(rcont_refDos);
        l_bicep_dos.Add(lbcPos_refDos);
        r_bicep_dos.Add(rbcPos_refDos);
        l_forearm_dos.Add(lfrPos_refDos);
        r_forearm_dos.Add(rfrPos_refDos);
        

        forearmDistanceDosAvg = Vector3.Distance(lfrPos_refDos, rfrPos_refDos) / 2;
        bicepDistanceDosAvg = Vector3.Distance(lbcPos_refDos, rbcPos_refDos) / 2;
        controllerDistanceDosAvg = Vector3.Distance(lcont_refDos, rcont_refDos) / 2;


        Vector3 lfrPos_refTres = referenceTres.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refTres = referenceTres.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refTres = referenceTres.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refTres = referenceTres.transform.InverseTransformPoint(rightBicep.transform.position);
        Vector3 lcont_refTres = referenceTres.transform.InverseTransformPoint(leftController.transform.position);
        Vector3 rcont_refTres = referenceTres.transform.InverseTransformPoint(rightController.transform.position);

        lfrPos_refTres = new Vector3(Mathf.Abs(lfrPos_refTres.x), lfrPos_refTres.y, lfrPos_refTres.z);
        rfrPos_refTres = new Vector3(Mathf.Abs(rfrPos_refTres.x), rfrPos_refTres.y, rfrPos_refTres.z);
        lbcPos_refTres = new Vector3(Mathf.Abs(lbcPos_refTres.x), lbcPos_refTres.y, lbcPos_refTres.z);
        rbcPos_refTres = new Vector3(Mathf.Abs(rbcPos_refTres.x), rbcPos_refTres.y, rbcPos_refTres.z);
        lcont_refTres = new Vector3(Mathf.Abs(lcont_refTres.x), lcont_refTres.y, lcont_refTres.z);
        rcont_refTres = new Vector3(Mathf.Abs(rcont_refTres.x), rcont_refTres.y, rcont_refTres.z);

        l_controller_tres.Add(lcont_refTres);
        r_controller_tres.Add(rcont_refTres);
        l_bicep_tres.Add(lbcPos_refTres);
        r_bicep_tres.Add(rbcPos_refTres);
        l_forearm_tres.Add(lfrPos_refTres);
        r_forearm_tres.Add(rfrPos_refTres);
        

        forearmDistanceTresAvg = Vector3.Distance(lfrPos_refTres, rfrPos_refTres) / 2;
        bicepDistanceTresAvg = Vector3.Distance(lbcPos_refTres, rbcPos_refTres) / 2;
        controllerDistanceTresAvg = Vector3.Distance(lcont_refTres, rcont_refTres) / 2;


        Vector3 lfrPos_refQuatro = referenceQuatro.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refQuatro = referenceQuatro.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refQuatro = referenceQuatro.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refQuatro = referenceQuatro.transform.InverseTransformPoint(rightBicep.transform.position);
        Vector3 lcont_refQuatro = referenceQuatro.transform.InverseTransformPoint(leftController.transform.position);
        Vector3 rcont_refQuatro = referenceQuatro.transform.InverseTransformPoint(rightController.transform.position);

        lfrPos_refQuatro = new Vector3(Mathf.Abs(lfrPos_refQuatro.x), lfrPos_refQuatro.y, lfrPos_refQuatro.z);
        rfrPos_refQuatro = new Vector3(Mathf.Abs(rfrPos_refQuatro.x), rfrPos_refQuatro.y, rfrPos_refQuatro.z);
        lbcPos_refQuatro = new Vector3(Mathf.Abs(lbcPos_refQuatro.x), lbcPos_refQuatro.y, lbcPos_refQuatro.z);
        rbcPos_refQuatro = new Vector3(Mathf.Abs(rbcPos_refQuatro.x), rbcPos_refQuatro.y, rbcPos_refQuatro.z);
        lcont_refQuatro = new Vector3(Mathf.Abs(lcont_refQuatro.x), lcont_refQuatro.y, lcont_refQuatro.z);
        rcont_refQuatro = new Vector3(Mathf.Abs(rcont_refQuatro.x), rcont_refQuatro.y, rcont_refQuatro.z);

        l_controller_quatro.Add(lcont_refQuatro);
        r_controller_quatro.Add(rcont_refQuatro);
        l_bicep_quatro.Add(lbcPos_refQuatro);
        r_bicep_quatro.Add(rbcPos_refQuatro);
        l_forearm_quatro.Add(lfrPos_refQuatro);
        r_forearm_quatro.Add(rfrPos_refQuatro);

        forearmDistanceQuatroAvg = Vector3.Distance(lfrPos_refQuatro, rfrPos_refQuatro) / 2;
        bicepDistanceQuatroAvg = Vector3.Distance(lbcPos_refQuatro, rbcPos_refQuatro) / 2;
        controllerDistanceQuatroAvg = Vector3.Distance(lcont_refQuatro, rcont_refQuatro) / 2;



        //Add seat marker positions to the kinWriter output, may be useful eventually
        //add scores against the HMD, Sternum, Sternum Rot, Bicepavg, and forearm avg to the writer output
        if (!memory)
        {
            DistFrameWriter();

            KinWriter();
            RelativeKinWriter();
        }
        if (memory)
        {
            if (returnRecording) //Mmeory trials require post processing to average the pisition of each marker at point A and return and then a comparison of their similarity.
            {
                //Writer();
                KinWriter();
            }
            else
            {
                KinWriter();
            }

            if(memoryPosition == MemoryPosition.A)
            {
                if(sideTested == SideTested.Left)
                {
                    POSAs_bicep_l.Add(leftBicep.transform.position);
                    POSAs_controller_l.Add(leftController.transform.position);
                    POSAs_forearm_l.Add(leftForearm.transform.position);
                }
                else
                {
                    POSAs_bicep_r.Add(rightBicep.transform.position);
                    POSAs_controller_r.Add(rightController.transform.position);
                    POSAs_forearm_r.Add(rightForearm.transform.position);
                }

            }
            if(memoryPosition == MemoryPosition.Return)
            {
                if (sideTested == SideTested.Left)
                {
                    ReturnPOSs_bicep_l.Add(leftBicep.transform.position);
                    ReturnPOSs_controller_l.Add(leftController.transform.position);
                    ReturnPOSs_forearm_l.Add(leftForearm.transform.position);
                }
                else
                {
                    ReturnPOSs_bicep_r.Add(rightBicep.transform.position);
                    ReturnPOSs_controller_r.Add(rightController.transform.position);
                    ReturnPOSs_forearm_r.Add(rightForearm.transform.position);
                }
            }
        }

        Debug.Log("Controller " + controllerDistanceBicepAvg + "and Bicep " + bicepDistanceBicepAvg + "an dForearm " + forearmDistanceBicepAvg);
        // Debug.Log("ForearmSternum: " + forearmDistanceSternum + ", BicepSternum: " + bicepDistanceSternum);
        string sternumText = "Forearm: " + forearmDistanceSternum.ToString("F2") + ", Bicep: " + bicepDistanceSternum.ToString("F2") + ", Cntrl: " + controllerDistance.ToString("F2");
        // Debug.Log(Vector3.Distance(leftBicep.transform.position,sternum.transform.position),)
        // float forearmDisparity ;
        //  float bicepDisparity;
        //  float shoulderDisparity;


        Vector3 lfrPos_refHMD = referenceHMD.transform.InverseTransformPoint(leftForearm.transform.position);
        Vector3 rfrPos_refHMD = referenceHMD.transform.InverseTransformPoint(rightForearm.transform.position);
        Vector3 lbcPos_refHMD = referenceHMD.transform.InverseTransformPoint(leftBicep.transform.position);
        Vector3 rbcPos_refHMD = referenceHMD.transform.InverseTransformPoint(rightBicep.transform.position);

        lfrPos_refHMD = new Vector3(Mathf.Abs(lfrPos_refHMD.x), lfrPos_refHMD.y, lfrPos_refHMD.z);
        rfrPos_refHMD = new Vector3(Mathf.Abs(rfrPos_refHMD.x), rfrPos_refHMD.y, rfrPos_refHMD.z);
        lbcPos_refHMD = new Vector3(Mathf.Abs(lbcPos_refHMD.x), lbcPos_refHMD.y, lbcPos_refHMD.z);
        rbcPos_refHMD = new Vector3(Mathf.Abs(rbcPos_refHMD.x), rbcPos_refHMD.y, rbcPos_refHMD.z);

        float forearmDistanceHMD = Vector3.Distance(lfrPos_refHMD, rfrPos_refHMD);
        float bicepDistanceHMD = Vector3.Distance(lbcPos_refHMD, rbcPos_refHMD);
        // Debug.Log("ForearmHMD: " + forearmDistanceHMD + ", BicepHMD: " + bicepDistanceHMD);
        string HMDText = "Forearm: " + forearmDistanceHMD.ToString("F2") + ", Bicep: " + bicepDistanceHMD.ToString("F2");
        //sternumTMP.text = sternumText;
        // HMDTMP.text = HMDText;

        string leftRawText = "Bicep: " + Vector3.Distance(leftBicep.transform.position, sternum.transform.position).ToString("F2") + "/" + Vector3.Distance(leftBicep.transform.position, HMD.transform.position).ToString("F2") + "Fore: " + Vector3.Distance(leftForearm.transform.position, sternum.transform.position).ToString("F2") + "/" + Vector3.Distance(leftForearm.transform.position, HMD.transform.position).ToString("F2");
        // LeftRawTMP.text = "L_Raw:: " + leftRawText;

        string rightRawText = "Bicep: " + Vector3.Distance(rightBicep.transform.position, sternum.transform.position).ToString("F2") + "/" + Vector3.Distance(rightBicep.transform.position, HMD.transform.position).ToString("F2") + "Fore: " + Vector3.Distance(rightForearm.transform.position, sternum.transform.position).ToString("F2") + "/" + Vector3.Distance(rightForearm.transform.position, HMD.transform.position).ToString("F2");
        // RightRawTMP.text = "R_Raw:: " + rightRawText;
        return 0;
    }
    private void DistFrameWriter()
    {

        WriterContent text = new WriterContent();
        //participant and trial info
        text = Writing("ID", pID, text);
        text = Writing("Session", sessionNum.ToString(), text);
        text = Writing("Date", System.DateTime.Now.ToString("MM/dd/yyyy"), text);
        text = Writing("Time", System.DateTime.Now.ToString("hh:mm:ss:ms"), text);
        text = Writing("SideTested", sideTested.ToString(), text);
        text = Writing("Movement", movement.ToString(), text);
        text = Writing("Reference", reference.ToString(), text);
        text = Writing("TrialNumber", currentTrial.ToString(), text);
        text = Writing("TrialTally", totalOngoingTrials.ToString(), text);
        text = Writing("MemoryPosition", memoryPosition.ToString(), text);
        //
        //Distances
        text = Writing("bicepDistSternumRot", bicepDistanceSternum.ToString(), text);
        text = Writing("bicepDistBicepAvg", bicepDistanceBicepAvg.ToString(), text);
        text = Writing("bicepDistForearmAvg", bicepDistanceForearmAvg.ToString(), text);
        text = Writing("bicepDistDosAvg", bicepDistanceDosAvg.ToString(), text);
        text = Writing("bicepDistTresAvg", bicepDistanceTresAvg.ToString(), text);
        text = Writing("bicepDistQuatroAvg", bicepDistanceQuatroAvg.ToString(), text);

        text = Writing("forearmDistSternumRot", forearmDistanceSternum.ToString(), text);
        text = Writing("forearmDistBicepAvg", forearmDistanceBicepAvg.ToString(), text);
        text = Writing("forearmDistForearmAvg", forearmDistanceForearmAvg.ToString(), text);
        text = Writing("forearmDistDosAvg", forearmDistanceDosAvg.ToString(), text);
        text = Writing("forearmDistTresAvg", forearmDistanceTresAvg.ToString(), text);
        text = Writing("forearmDistQuatroAvg", forearmDistanceQuatroAvg.ToString(), text);

        text = Writing("controllerDistSternumRot", controllerDistance.ToString(), text);
        text = Writing("controllerDistBicepAvg", controllerDistanceBicepAvg.ToString(), text);
        text = Writing("controllerDistForearmAvg", controllerDistanceForearmAvg.ToString(), text);
        text = Writing("controllerDistDosAvg", controllerDistanceDosAvg.ToString(), text);
        text = Writing("controllerDistTresAvg", controllerDistanceTresAvg.ToString(), text);
        text = Writing("controllerDistQuatroAvg", controllerDistanceQuatroAvg.ToString(), text);

        //

        //writer.WriteLine(pID + ";" + sessionNum.ToString() + ";" + System.DateTime.Now.ToString("MM/dd/yyyy") + ";" + System.DateTime.Now.ToString("hh:mm:ss") + ";" + sideTested.ToString() + ";" + movement.ToString() + ";" + reference.ToString() + ";" + currentTrial.ToString() + ";" + forearmDistanceSternum.ToString() + ";" + bicepDistanceSternum.ToString() + ";" + controllerDistance.ToString() + ";" + forearmDistanceBicepAvg.ToString() + ";" + bicepDistanceBicepAvg.ToString() + ";" + controllerDistanceBicepAvg.ToString() + ";" + forearmDistanceForearmpAvg.ToString() + ";" + bicepDistanceForearmAvg.ToString() + ";" + controllerDistanceForearmAvg.ToString());
        if (distPerFramewriter == null)
        {
            distPerFramewriter = new StreamWriter(GetPath());
            distPerFramewriter.WriteLine(text.header);
            //writer.WriteLine("ID ; Session ; Date ; Time ; SideTested ; Movement ; Reference ; trialNum ; forearmDistSternum ; bicepDistSternum ; controllerDistSternum ; forearmDistBicepAvg ; bicepDistBicepAvg ; controllerDistBicepAvg ; forearmDistForAvg ; bicepDistForAvg ; controllerDistForAvg");
        }
        distPerFramewriter.WriteLine(text.body);
    }
    //must add bool check for memory vs contra so that distances are calculate based on the correct comparisons.
    private void AvgLocWriter(bool memory)
    {
        //Concatenates data into columns for entry into a csv file
        WriterContent text = new WriterContent();
        //participant and trial info
        text = Writing("ID", pID, text);
        text = Writing("Session", sessionNum.ToString(), text);
        text = Writing("Date", System.DateTime.Now.ToString("MM/dd/yyyy"), text);
        text = Writing("Time", System.DateTime.Now.ToString("hh:mm:ss:ms"), text);
        text = Writing("SideTested", sideTested.ToString(), text);
        text = Writing("Movement", movement.ToString(), text);
        text = Writing("Reference", reference.ToString(), text);
        text = Writing("TrialNumber", currentTrial.ToString(), text);
        text = Writing("TrialTally", totalOngoingTrials.ToString(), text);
        text = Writing("MemoryPosition", memoryPosition.ToString(), text);
        //
        if (!memory)
        {
            //Distances
            text = Writing("controller_sternumRot", Vector3.Distance(AverageUnilateral(l_controller_sternumRot), AverageUnilateral(r_controller_sternumRot)).ToString(), text);
            text = Writing("controller_bicepAvg", Vector3.Distance(AverageUnilateral(l_controller_bicepAvg), AverageUnilateral(r_controller_bicepAvg)).ToString(), text);
            text = Writing("controller_forearmAvg", Vector3.Distance(AverageUnilateral(l_controller_forearmAvg), AverageUnilateral(r_controller_forearmAvg)).ToString(), text);
            text = Writing("controller_dos", Vector3.Distance(AverageUnilateral(l_controller_dos), AverageUnilateral(r_controller_dos)).ToString(), text);
            text = Writing("controller_tres", Vector3.Distance(AverageUnilateral(l_controller_tres), AverageUnilateral(r_controller_tres)).ToString(), text);
            text = Writing("controller_quatro", Vector3.Distance(AverageUnilateral(l_controller_quatro), AverageUnilateral(r_controller_quatro)).ToString(), text);

            //position
            //bicep
            text = Writing("bicep_sternumRot", Vector3.Distance(AverageUnilateral(l_bicep_sternumRot), AverageUnilateral(r_bicep_sternumRot)).ToString(), text);
            text = Writing("bicep_bicepAvg", Vector3.Distance(AverageUnilateral(l_bicep_bicepAvg), AverageUnilateral(r_bicep_bicepAvg)).ToString(), text);
            text = Writing("bicep_forearmAvg", Vector3.Distance(AverageUnilateral(l_bicep_forearmAvg), AverageUnilateral(r_bicep_forearmAvg)).ToString(), text);
            text = Writing("bicep_dos", Vector3.Distance(AverageUnilateral(l_bicep_dos), AverageUnilateral(r_bicep_dos)).ToString(), text);
            text = Writing("bicep_tres", Vector3.Distance(AverageUnilateral(l_bicep_tres), AverageUnilateral(r_bicep_tres)).ToString(), text);
            text = Writing("bicep_quatro", Vector3.Distance(AverageUnilateral(l_bicep_quatro), AverageUnilateral(r_bicep_quatro)).ToString(), text);

            //positions
            //forearm
            text = Writing("forearm_sternumRot", Vector3.Distance(AverageUnilateral(l_forearm_sternumRot), AverageUnilateral(r_forearm_sternumRot)).ToString(), text);
            text = Writing("forearm_bicepAvg", Vector3.Distance(AverageUnilateral(l_forearm_bicepAvg), AverageUnilateral(r_forearm_bicepAvg)).ToString(), text);
            text = Writing("forearm_forearmAvg", Vector3.Distance(AverageUnilateral(l_forearm_forearmAvg), AverageUnilateral(r_forearm_forearmAvg)).ToString(), text);
            text = Writing("forearm_dos", Vector3.Distance(AverageUnilateral(l_forearm_dos), AverageUnilateral(r_forearm_dos)).ToString(), text);
            text = Writing("forearm_tres", Vector3.Distance(AverageUnilateral(l_forearm_tres), AverageUnilateral(r_forearm_tres)).ToString(), text);
            text = Writing("forearm_quatro", Vector3.Distance(AverageUnilateral(l_forearm_quatro), AverageUnilateral(r_forearm_quatro)).ToString(), text);
            //
        }
        if (memory)
        {
            //Distances
            if(sideTested == SideTested.Left)
            {
                text = Writing("controller", Vector3.Distance(AverageUnilateral(POSAs_controller_l), AverageUnilateral(ReturnPOSs_controller_l)).ToString(), text);
                text = Writing("forearm", Vector3.Distance(AverageUnilateral(POSAs_forearm_l), AverageUnilateral(ReturnPOSs_forearm_l)).ToString(), text);
                text = Writing("bicep", Vector3.Distance(AverageUnilateral(POSAs_forearm_l), AverageUnilateral(ReturnPOSs_bicep_l)).ToString(), text);
                //
            }
            else
            {
                text = Writing("controller", Vector3.Distance(AverageUnilateral(POSAs_controller_r), AverageUnilateral(ReturnPOSs_controller_r)).ToString(), text);
                text = Writing("forearm", Vector3.Distance(AverageUnilateral(POSAs_forearm_r), AverageUnilateral(ReturnPOSs_forearm_r)).ToString(), text);
                text = Writing("bicep", Vector3.Distance(AverageUnilateral(POSAs_forearm_r), AverageUnilateral(ReturnPOSs_bicep_r)).ToString(), text);
                //
            }

        }


        if (avgLocWriter == null)
        {
            avgLocWriter = new StreamWriter(GetPathAverageLoc());
            avgLocWriter.WriteLine(text.header);
        }
        avgLocWriter.WriteLine(text.body);
    }

    private void KinWriter()
    {
        //Concatenates data into columns for entry into a csv file
        WriterContent text = new WriterContent();
        //participant and trial info
        text = Writing("ID", pID, text);
        text = Writing("Session", sessionNum.ToString(), text);
        text = Writing("Date", System.DateTime.Now.ToString("MM/dd/yyyy"), text);
        text = Writing("Time", System.DateTime.Now.ToString("hh:mm:ss:ms"), text);
        text = Writing("SideTested", sideTested.ToString(), text);
        text = Writing("Movement", movement.ToString(), text);
        text = Writing("Reference", reference.ToString(), text);
        text = Writing("TrialNumber", currentTrial.ToString(), text);
        text = Writing("TrialTally", totalOngoingTrials.ToString(), text);
        text = Writing("MemoryPosition", memoryPosition.ToString(), text);
        //
        //Positions
        text = Writing("forearm_L_X", leftForearm.transform.position.x, text);
        text = Writing("forearm_L_Y", leftForearm.transform.position.y, text);
        text = Writing("forearm_L_Z", leftForearm.transform.position.z, text);
        text = Writing("forearm_R_X", rightForearm.transform.position.x, text);
        text = Writing("forearm_R_Y", rightForearm.transform.position.y, text);
        text = Writing("forearm_R_Z", rightForearm.transform.position.z, text);
        text = Writing("bicep_L_X", leftBicep.transform.position.x, text);
        text = Writing("bicep_L_Y", leftBicep.transform.position.y, text);
        text = Writing("bicep_L_Z", leftBicep.transform.position.z, text);
        text = Writing("bicep_R_X", rightBicep.transform.position.x, text);
        text = Writing("bicep_R_Y", rightBicep.transform.position.y, text);
        text = Writing("bicep_R_Z", rightBicep.transform.position.z, text);
        text = Writing("controller_L_X", leftController.transform.position.x, text);
        text = Writing("controller_L_Y", leftController.transform.position.y, text);
        text = Writing("controller_L_Z", leftController.transform.position.z, text);
        text = Writing("controller_R_X", rightController.transform.position.x, text);
        text = Writing("controller_R_Y", rightController.transform.position.y, text);
        text = Writing("controller_R_Z", rightController.transform.position.z, text);
        text = Writing("sternum_X", sternum.transform.position.x, text);
        text = Writing("sternum_Y", sternum.transform.position.y, text);
        text = Writing("sternum_Z", sternum.transform.position.z, text);
        text = Writing("ref_sternumRot_X", referenceSternumRot.transform.position.x, text);
        text = Writing("ref_sternumRot_Y", referenceSternumRot.transform.position.y, text);
        text = Writing("ref_sternumRot_Z", referenceSternumRot.transform.position.z, text);
        text = Writing("HMD_X", HMD.transform.position.x, text);
        text = Writing("HMD_Y", HMD.transform.position.y, text);
        text = Writing("HMD_Z", HMD.transform.position.z, text);
        text = Writing("CameraRig_X", CameraRig.transform.position.x, text);
        text = Writing("CameraRig_Y", CameraRig.transform.position.y, text);
        text = Writing("CameraRig_Z", CameraRig.transform.position.z, text);
        text = Writing("ref_bicepAvg_X", referenceBicepAvg.transform.position.x, text);
        text = Writing("ref_bicepAvg_Y", referenceBicepAvg.transform.position.y, text);
        text = Writing("ref_bicepAvg_Z", referenceBicepAvg.transform.position.z, text);
        text = Writing("ref_forearmAvg_X", referenceForearmAvg.transform.position.x, text);
        text = Writing("ref_forearmAvg_Y", referenceForearmAvg.transform.position.y, text);
        text = Writing("ref_forearmAvg_Z", referenceForearmAvg.transform.position.z, text);
        text = Writing("ref_Dos_X", referenceDos.transform.position.x, text);
        text = Writing("ref_Dos_Y", referenceDos.transform.position.y, text);
        text = Writing("ref_Dos_Z", referenceDos.transform.position.z, text);
        text = Writing("ref_Tres_X", referenceTres.transform.position.x, text);
        text = Writing("ref_Tres_Y", referenceTres.transform.position.y, text);
        text = Writing("ref_Tres_Z", referenceTres.transform.position.z, text);
        text = Writing("ref_Quatro_X", referenceQuatro.transform.position.x, text);
        text = Writing("ref_Quatro_Y", referenceQuatro.transform.position.y, text);
        text = Writing("ref_Quatro_Z", referenceQuatro.transform.position.z, text);

        //
        //Rotations
        text = Writing("forearm_L_X_Rot", leftForearm.transform.eulerAngles.x, text);
        text = Writing("forearm_L_Y_Rot", leftForearm.transform.eulerAngles.y, text);
        text = Writing("forearm_L_Z_Rot", leftForearm.transform.eulerAngles.z, text);
        text = Writing("forearm_R_X_Rot", rightForearm.transform.eulerAngles.x, text);
        text = Writing("forearm_R_Y_Rot", rightForearm.transform.eulerAngles.y, text);
        text = Writing("forearm_R_Z_Rot", rightForearm.transform.eulerAngles.z, text);
        text = Writing("bicep_L_X_Rot", leftBicep.transform.eulerAngles.x, text);
        text = Writing("bicep_L_Y_Rot", leftBicep.transform.eulerAngles.y, text);
        text = Writing("bicep_L_Z_Rot", leftBicep.transform.eulerAngles.z, text);
        text = Writing("bicep_R_X_Rot", rightBicep.transform.eulerAngles.x, text);
        text = Writing("bicep_R_Y_Rot", rightBicep.transform.eulerAngles.y, text);
        text = Writing("bicep_R_Z_Rot", rightBicep.transform.eulerAngles.z, text);
        text = Writing("controller_L_X_Rot", leftController.transform.eulerAngles.x, text);
        text = Writing("controller_L_Y_Rot", leftController.transform.eulerAngles.y, text);
        text = Writing("controller_L_Z_Rot", leftController.transform.eulerAngles.z, text);
        text = Writing("controller_R_X_Rot", rightController.transform.eulerAngles.x, text);
        text = Writing("controller_R_Y_Rot", rightController.transform.eulerAngles.y, text);
        text = Writing("controller_R_Z_Rot", rightController.transform.eulerAngles.z, text);
        text = Writing("sternum_X_Rot", sternum.transform.eulerAngles.x, text);
        text = Writing("sternum_Y_Rot", sternum.transform.eulerAngles.y, text);
        text = Writing("sternum_Z_Rot", sternum.transform.eulerAngles.z, text);
        text = Writing("ref_sternumRot_X", referenceSternumRot.transform.eulerAngles.x, text);
        text = Writing("ref_sternumRot_Y", referenceSternumRot.transform.eulerAngles.y, text);
        text = Writing("ref_sternumRot_Z", referenceSternumRot.transform.eulerAngles.z, text);
        text = Writing("HMD_X_Rot", HMD.transform.eulerAngles.x, text);
        text = Writing("HMD_Y_Rot", HMD.transform.eulerAngles.y, text);
        text = Writing("HMD_Z_Rot", HMD.transform.eulerAngles.z, text);
        text = Writing("CameraRig_X_Rot", CameraRig.transform.eulerAngles.x, text);
        text = Writing("CameraRig_Y_Rot", CameraRig.transform.eulerAngles.y, text);
        text = Writing("CameraRig_Z_Rot", CameraRig.transform.eulerAngles.z, text);
        text = Writing("ref_bicepAvg_X", referenceBicepAvg.transform.eulerAngles.x, text);
        text = Writing("ref_bicepAvg_Y", referenceBicepAvg.transform.eulerAngles.y, text);
        text = Writing("ref_bicepAvg_Z", referenceBicepAvg.transform.eulerAngles.z, text);
        text = Writing("ref_forearmAvg_X", referenceForearmAvg.transform.eulerAngles.x, text);
        text = Writing("ref_forearmAvg_Y", referenceForearmAvg.transform.eulerAngles.y, text);
        text = Writing("ref_forearmAvg_Z", referenceForearmAvg.transform.eulerAngles.z, text);
        text = Writing("ref_Dos_X", referenceDos.transform.eulerAngles.x, text);
        text = Writing("ref_Dos_Y", referenceDos.transform.eulerAngles.y, text);
        text = Writing("ref_Dos_Z", referenceDos.transform.eulerAngles.z, text);
        text = Writing("ref_Tres_X", referenceTres.transform.eulerAngles.x, text);
        text = Writing("ref_Tres_Y", referenceTres.transform.eulerAngles.y, text);
        text = Writing("ref_Tres_Z", referenceTres.transform.eulerAngles.z, text);
        text = Writing("ref_Quatro_X", referenceQuatro.transform.eulerAngles.x, text);
        text = Writing("ref_Quatro_Y", referenceQuatro.transform.eulerAngles.y, text);
        text = Writing("ref_Quatro_Z", referenceQuatro.transform.eulerAngles.z, text);
        //




        if (kinWriter == null)
        {
            kinWriter = new StreamWriter(GetPathKinematic());
            kinWriter.WriteLine(text.header);
            //kinWriter.WriteLine("ID ; Session ; Date ; Time ; SideTested ; Movement ; Reference ; trialNum ; MemoryPosition ; forearm_L_X ; forearm_L_Y ; forearm_L_Z ; forearm_R_X ; forearm_R_Y ; forearm_R_Z ; bicep_L_X ; bicep_L_Y ; bicep_L_Z ; bicep_R_X ; bicep_R_Y ; bicep_R_Z ;controller_L_X ; controller_L_Y ; controller_L_Z ; controller_R_X ; controller_R_Y ; controller_R_Z ; Sternum_X ; Sternum_Y ; Sternum_Z ; HMD_X ; HMD_Y ; HMD_Z ; CamRig_X; CamRig_X ; CamRig_Z ; BicepAvgRef_X ; BicepAvgRef_Y ; BicepAvgRef_Z; ForearmAvgRef_X ; ForearmAvgRef_Y ; ForearmAvgRef_Z ; forearm_L_X_rot ; forearm_L_Y_rot ; forearm_L_Z_rot ; forearm_R_X_rot ; forearm_R_Y_rot ; forearm_R_Z_rot ; bicep_L_X_rot ; bicep_L_Y_rot ; bicep_L_Z_rot ; bicep_R_X_rot ; bicep_R_Y_rot ; bicep_R_Z_rot ;controller_L_X_rot ; controller_L_Y_rot ; controller_L_Z_rot ; controller_R_X_rot ; controller_R_Y_rot ; controller_R_Z_rot ; Sternum_X_rot ; Sternum_Y_rot ; Sternum_Z_rot");
        }
        kinWriter.WriteLine(text.body);
        //kinWriter.WriteLine(pID + ";" + sessionNum.ToString() + ";" + System.DateTime.Now.ToString("MM/dd/yyyy") + ";" + System.DateTime.Now.ToString("hh:mm:ss:ms") + ";" + sideTested.ToString() + ";" + movement.ToString() + ";" + reference.ToString() + ";" + currentTrial.ToString() +";" +   memoryPosition.ToString()   + ";" + leftForearm.transform.position.x + ";" + leftForearm.transform.position.y + ";" + leftForearm.transform.position.z + ";" + rightForearm.transform.position.x + ";" + rightForearm.transform.position.y + ";" + rightForearm.transform.position.z + ";" + leftBicep.transform.position.x + ";" + leftBicep.transform.position.y + ";" + leftBicep.transform.position.z + ";" + rightBicep.transform.position.x + ";" + rightBicep.transform.position.y + ";" + rightBicep.transform.position.z + ";" + leftController.transform.position.x + ";" + leftController.transform.position.y + ";" + leftController.transform.position.z + ";" + rightController.transform.position.x + ";" + rightController.transform.position.y + ";" + rightController.transform.position.z + ";" + sternum.transform.position.x + ";" + sternum.transform.position.y + ";" + sternum.transform.position.z + ";" + HMD.transform.position.x + ";" + HMD.transform.position.y + ";" + HMD.transform.position.z + ";" + CameraRig.transform.position.x + ";" + CameraRig.transform.position.y + ";" + CameraRig.transform.position.z + ";" + referenceBicepAvg.transform.position.x + ";" + referenceBicepAvg.transform.position.y + ";" + referenceBicepAvg.transform.position.z + ";" + referenceForearmAvg.transform.position.x + ";" + referenceForearmAvg.transform.position.y + ";" + referenceForearmAvg.transform.position.z + ";" + leftForearm.transform.eulerAngles.x + ";" + leftForearm.transform.eulerAngles.y + ";" + leftForearm.transform.eulerAngles.z + ";" + rightForearm.transform.eulerAngles.x + ";" + rightForearm.transform.eulerAngles.y + ";" + rightForearm.transform.eulerAngles.z + ";" + leftBicep.transform.eulerAngles.x + ";" + leftBicep.transform.eulerAngles.y + ";" + leftBicep.transform.eulerAngles.z + ";" + rightBicep.transform.eulerAngles.x + ";" + rightBicep.transform.eulerAngles.y + ";" + rightBicep.transform.eulerAngles.z + ";" + leftController.transform.eulerAngles.x + ";" + leftController.transform.eulerAngles.y + ";" + leftController.transform.eulerAngles.z + ";" + rightController.transform.eulerAngles.x + ";" + rightController.transform.eulerAngles.y + ";" + rightController.transform.eulerAngles.z + ";" + sternum.transform.eulerAngles.x + ";" + sternum.transform.eulerAngles.y + ";" + sternum.transform.eulerAngles.z);

    }

    private void RelativeKinWriter()
    {//Concatenates data into columns for entry into a csv file
        WriterContent text = new WriterContent();
        //participant and trial info
        text = Writing("ID", pID, text);
        text = Writing("Session", sessionNum.ToString(), text);
        text = Writing("Date", System.DateTime.Now.ToString("MM/dd/yyyy"), text);
        text = Writing("Time", System.DateTime.Now.ToString("hh:mm:ss:ms"), text);
        text = Writing("SideTested", sideTested.ToString(), text);
        text = Writing("Movement", movement.ToString(), text);
        text = Writing("Reference", reference.ToString(), text);
        text = Writing("TrialNumber", currentTrial.ToString(), text);
        text = Writing("TrialTally", totalOngoingTrials.ToString(), text);
        text = Writing("MemoryPosition", memoryPosition.ToString(), text);
        //
        //Positions
        //controller
        text = Writing("l_controller_sternumRot_x", l_controller_sternumRot[l_controller_sternumRot.Count - 1].x.ToString(), text);
        text = Writing("l_controller_sternumRot_y", l_controller_sternumRot[l_controller_sternumRot.Count - 1].y.ToString(), text);
        text = Writing("l_controller_sternumRot_z", l_controller_sternumRot[l_controller_sternumRot.Count - 1].z.ToString(), text);

        text = Writing("r_controller_sternumRot_x", r_controller_sternumRot[r_controller_sternumRot.Count - 1].x.ToString(), text);
        text = Writing("r_controller_sternumRot_y", r_controller_sternumRot[r_controller_sternumRot.Count - 1].y.ToString(), text);
        text = Writing("r_controller_sternumRot_z", r_controller_sternumRot[r_controller_sternumRot.Count - 1].z.ToString(), text);

        text = Writing("l_controller_bicepAvg_x", l_controller_bicepAvg[l_controller_bicepAvg.Count - 1].x.ToString(), text);
        text = Writing("l_controller_bicepAvg_y", l_controller_bicepAvg[l_controller_bicepAvg.Count - 1].y.ToString(), text);
        text = Writing("l_controller_bicepAvg_z", l_controller_bicepAvg[l_controller_bicepAvg.Count - 1].z.ToString(), text);

        text = Writing("r_controller_bicepAvg_x", r_controller_bicepAvg[r_controller_bicepAvg.Count - 1].x.ToString(), text);
        text = Writing("r_controller_bicepAvg_y", r_controller_bicepAvg[r_controller_bicepAvg.Count - 1].y.ToString(), text);
        text = Writing("r_controller_bicepAvg_z", r_controller_bicepAvg[r_controller_bicepAvg.Count - 1].z.ToString(), text);

        text = Writing("l_controller_forearmAvg_x", l_controller_forearmAvg[l_controller_forearmAvg.Count - 1].x.ToString(), text);
        text = Writing("l_controller_forearmAvg_y", l_controller_forearmAvg[l_controller_forearmAvg.Count - 1].y.ToString(), text);
        text = Writing("l_controller_forearmAvg_z", l_controller_forearmAvg[l_controller_forearmAvg.Count - 1].z.ToString(), text);

        text = Writing("r_controller_forearmAvg_x", r_controller_forearmAvg[r_controller_forearmAvg.Count - 1].x.ToString(), text);
        text = Writing("r_controller_forearmAvg_y", r_controller_forearmAvg[r_controller_forearmAvg.Count - 1].y.ToString(), text);
        text = Writing("r_controller_forearmAvg_z", r_controller_forearmAvg[r_controller_forearmAvg.Count - 1].z.ToString(), text);

        text = Writing("l_controller_dos_x", l_controller_dos[l_controller_dos.Count - 1].x.ToString(), text);
        text = Writing("l_controller_dos_y", l_controller_dos[l_controller_dos.Count - 1].y.ToString(), text);
        text = Writing("l_controller_dos_z", l_controller_dos[l_controller_dos.Count - 1].z.ToString(), text);

        text = Writing("r_controller_dos_x", r_controller_dos[r_controller_dos.Count - 1].x.ToString(), text);
        text = Writing("r_controller_dos_y", r_controller_dos[r_controller_dos.Count - 1].y.ToString(), text);
        text = Writing("r_controller_dos_z", r_controller_dos[r_controller_dos.Count - 1].z.ToString(), text);

        text = Writing("l_controller_tres_x", l_controller_tres[l_controller_tres.Count - 1].x.ToString(), text);
        text = Writing("l_controller_tres_y", l_controller_tres[l_controller_tres.Count - 1].y.ToString(), text);
        text = Writing("l_controller_tres_z", l_controller_tres[l_controller_tres.Count - 1].z.ToString(), text);

        text = Writing("r_controller_tres_x", r_controller_tres[r_controller_tres.Count - 1].x.ToString(), text);
        text = Writing("r_controller_tres_y", r_controller_tres[r_controller_tres.Count - 1].y.ToString(), text);
        text = Writing("r_controller_tres_z", r_controller_tres[r_controller_tres.Count - 1].z.ToString(), text);

        text = Writing("l_controller_quatro_x", l_controller_quatro[l_controller_quatro.Count - 1].x.ToString(), text);
        text = Writing("l_controller_quatro_y", l_controller_quatro[l_controller_quatro.Count - 1].y.ToString(), text);
        text = Writing("l_controller_quatro_z", l_controller_quatro[l_controller_quatro.Count - 1].z.ToString(), text);

        text = Writing("r_controller_quatro_x", r_controller_quatro[r_controller_quatro.Count - 1].x.ToString(), text);
        text = Writing("r_controller_quatro_y", r_controller_quatro[r_controller_quatro.Count - 1].y.ToString(), text);
        text = Writing("r_controller_quatro_z", r_controller_quatro[r_controller_quatro.Count - 1].z.ToString(), text);

        //position
        //bicep
        text = Writing("l_bicep_sternumRot_x", l_bicep_sternumRot[l_bicep_sternumRot.Count - 1].x.ToString(), text);
        text = Writing("l_bicep_sternumRot_y", l_bicep_sternumRot[l_bicep_sternumRot.Count - 1].y.ToString(), text);
        text = Writing("l_bicep_sternumRot_z", l_bicep_sternumRot[l_bicep_sternumRot.Count - 1].z.ToString(), text);

        text = Writing("r_bicep_sternumRot_x", r_bicep_sternumRot[r_bicep_sternumRot.Count - 1].x.ToString(), text);
        text = Writing("r_bicep_sternumRot_y", r_bicep_sternumRot[r_bicep_sternumRot.Count - 1].y.ToString(), text);
        text = Writing("r_bicep_sternumRot_z", r_bicep_sternumRot[r_bicep_sternumRot.Count - 1].z.ToString(), text);

        text = Writing("l_bicep_bicepAvg_x", l_bicep_bicepAvg[l_bicep_bicepAvg.Count - 1].x.ToString(), text);
        text = Writing("l_bicep_bicepAvg_y", l_bicep_bicepAvg[l_bicep_bicepAvg.Count - 1].y.ToString(), text);
        text = Writing("l_bicep_bicepAvg_z", l_bicep_bicepAvg[l_bicep_bicepAvg.Count - 1].z.ToString(), text);

        text = Writing("r_bicep_bicepAvg_x", r_bicep_bicepAvg[r_bicep_bicepAvg.Count - 1].x.ToString(), text);
        text = Writing("r_bicep_bicepAvg_y", r_bicep_bicepAvg[r_bicep_bicepAvg.Count - 1].y.ToString(), text);
        text = Writing("r_bicep_bicepAvg_z", r_bicep_bicepAvg[r_bicep_bicepAvg.Count - 1].z.ToString(), text);

        text = Writing("l_bicep_forearmAvg_x", l_bicep_forearmAvg[l_bicep_forearmAvg.Count - 1].x.ToString(), text);
        text = Writing("l_bicep_forearmAvg_y", l_bicep_forearmAvg[l_bicep_forearmAvg.Count - 1].y.ToString(), text);
        text = Writing("l_bicep_forearmAvg_z", l_bicep_forearmAvg[l_bicep_forearmAvg.Count - 1].z.ToString(), text);

        text = Writing("r_bicep_forearmAvg_x", r_bicep_forearmAvg[r_bicep_forearmAvg.Count - 1].x.ToString(), text);
        text = Writing("r_bicep_forearmAvg_y", r_bicep_forearmAvg[r_bicep_forearmAvg.Count - 1].y.ToString(), text);
        text = Writing("r_bicep_forearmAvg_z", r_bicep_forearmAvg[r_bicep_forearmAvg.Count - 1].z.ToString(), text);

        text = Writing("l_bicep_dos_x", l_bicep_dos[l_bicep_dos.Count - 1].x.ToString(), text);
        text = Writing("l_bicep_dos_y", l_bicep_dos[l_bicep_dos.Count - 1].y.ToString(), text);
        text = Writing("l_bicep_dos_z", l_bicep_dos[l_bicep_dos.Count - 1].z.ToString(), text);

        text = Writing("r_bicep_dos_x", r_bicep_dos[r_bicep_dos.Count - 1].x.ToString(), text);
        text = Writing("r_bicep_dos_y", r_bicep_dos[r_bicep_dos.Count - 1].y.ToString(), text);
        text = Writing("r_bicep_dos_z", r_bicep_dos[r_bicep_dos.Count - 1].z.ToString(), text);

        text = Writing("l_bicep_tres_x", l_bicep_tres[l_bicep_tres.Count - 1].x.ToString(), text);
        text = Writing("l_bicep_tres_y", l_bicep_tres[l_bicep_tres.Count - 1].y.ToString(), text);
        text = Writing("l_bicep_tres_z", l_bicep_tres[l_bicep_tres.Count - 1].z.ToString(), text);

        text = Writing("r_bicep_tres_x", r_bicep_tres[r_bicep_tres.Count - 1].x.ToString(), text);
        text = Writing("r_bicep_tres_y", r_bicep_tres[r_bicep_tres.Count - 1].y.ToString(), text);
        text = Writing("r_bicep_tres_z", r_bicep_tres[r_bicep_tres.Count - 1].z.ToString(), text);

        text = Writing("l_bicep_quatro_x", l_bicep_quatro[l_bicep_quatro.Count - 1].x.ToString(), text);
        text = Writing("l_bicep_quatro_y", l_bicep_quatro[l_bicep_quatro.Count - 1].y.ToString(), text);
        text = Writing("l_bicep_quatro_z", l_bicep_quatro[l_bicep_quatro.Count - 1].z.ToString(), text);

        text = Writing("r_bicep_quatro_x", r_bicep_quatro[r_bicep_quatro.Count - 1].x.ToString(), text);
        text = Writing("r_bicep_quatro_y", r_bicep_quatro[r_bicep_quatro.Count - 1].y.ToString(), text);
        text = Writing("r_bicep_quatro_z", r_bicep_quatro[r_bicep_quatro.Count - 1].z.ToString(), text);

        //positions
        //forearm
        text = Writing("l_forearm_sternumRot_x", l_forearm_sternumRot[l_forearm_sternumRot.Count - 1].x.ToString(), text);
        text = Writing("l_forearm_sternumRot_y", l_forearm_sternumRot[l_forearm_sternumRot.Count - 1].y.ToString(), text);
        text = Writing("l_forearm_sternumRot_z", l_forearm_sternumRot[l_forearm_sternumRot.Count - 1].z.ToString(), text);

        text = Writing("r_forearm_sternumRot_x", r_forearm_sternumRot[r_forearm_sternumRot.Count - 1].x.ToString(), text);
        text = Writing("r_forearm_sternumRot_y", r_forearm_sternumRot[r_forearm_sternumRot.Count - 1].y.ToString(), text);
        text = Writing("r_forearm_sternumRot_z", r_forearm_sternumRot[r_forearm_sternumRot.Count - 1].z.ToString(), text);

        text = Writing("l_forearm_bicepAvg_x", l_forearm_bicepAvg[l_forearm_bicepAvg.Count - 1].x.ToString(), text);
        text = Writing("l_forearm_bicepAvg_y", l_forearm_bicepAvg[l_forearm_bicepAvg.Count - 1].y.ToString(), text);
        text = Writing("l_forearm_bicepAvg_z", l_forearm_bicepAvg[l_forearm_bicepAvg.Count - 1].z.ToString(), text);

        text = Writing("r_forearm_bicepAvg_x", r_forearm_bicepAvg[r_forearm_bicepAvg.Count - 1].x.ToString(), text);
        text = Writing("r_forearm_bicepAvg_y", r_forearm_bicepAvg[r_forearm_bicepAvg.Count - 1].y.ToString(), text);
        text = Writing("r_forearm_bicepAvg_z", r_forearm_bicepAvg[r_forearm_bicepAvg.Count - 1].z.ToString(), text);

        text = Writing("l_forearm_forearmAvg_x", l_forearm_forearmAvg[l_forearm_forearmAvg.Count - 1].x.ToString(), text);
        text = Writing("l_forearm_forearmAvg_y", l_forearm_forearmAvg[l_forearm_forearmAvg.Count - 1].y.ToString(), text);
        text = Writing("l_forearm_forearmAvg_z", l_forearm_forearmAvg[l_forearm_forearmAvg.Count - 1].z.ToString(), text);

        text = Writing("r_forearm_forearmAvg_x", r_forearm_forearmAvg[r_forearm_forearmAvg.Count - 1].x.ToString(), text);
        text = Writing("r_forearm_forearmAvg_y", r_forearm_forearmAvg[r_forearm_forearmAvg.Count - 1].y.ToString(), text);
        text = Writing("r_forearm_forearmAvg_z", r_forearm_forearmAvg[r_forearm_forearmAvg.Count - 1].z.ToString(), text);

        text = Writing("l_forearm_dos_x", l_forearm_dos[l_forearm_dos.Count - 1].x.ToString(), text);
        text = Writing("l_forearm_dos_y", l_forearm_dos[l_forearm_dos.Count - 1].y.ToString(), text);
        text = Writing("l_forearm_dos_z", l_forearm_dos[l_forearm_dos.Count - 1].z.ToString(), text);

        text = Writing("r_forearm_dos_x", r_forearm_dos[r_forearm_dos.Count - 1].x.ToString(), text);
        text = Writing("r_forearm_dos_y", r_forearm_dos[r_forearm_dos.Count - 1].y.ToString(), text);
        text = Writing("r_forearm_dos_z", r_forearm_dos[r_forearm_dos.Count - 1].z.ToString(), text);

        text = Writing("l_forearm_tres_x", l_forearm_tres[l_forearm_tres.Count - 1].x.ToString(), text);
        text = Writing("l_forearm_tres_y", l_forearm_tres[l_forearm_tres.Count - 1].y.ToString(), text);
        text = Writing("l_forearm_tres_z", l_forearm_tres[l_forearm_tres.Count - 1].z.ToString(), text);

        text = Writing("r_forearm_tres_x", r_forearm_tres[r_forearm_tres.Count - 1].x.ToString(), text);
        text = Writing("r_forearm_tres_y", r_forearm_tres[r_forearm_tres.Count - 1].y.ToString(), text);
        text = Writing("r_forearm_tres_z", r_forearm_tres[r_forearm_tres.Count - 1].z.ToString(), text);

        text = Writing("l_forearm_quatro_x", l_forearm_quatro[l_forearm_quatro.Count - 1].x.ToString(), text);
        text = Writing("l_forearm_quatro_y", l_forearm_quatro[l_forearm_quatro.Count - 1].y.ToString(), text);
        text = Writing("l_forearm_quatro_z", l_forearm_quatro[l_forearm_quatro.Count - 1].z.ToString(), text);

        text = Writing("r_forearm_quatro_x", r_forearm_quatro[r_forearm_quatro.Count - 1].x.ToString(), text);
        text = Writing("r_forearm_quatro_y", r_forearm_quatro[r_forearm_quatro.Count - 1].y.ToString(), text);
        text = Writing("r_forearm_quatro_z", r_forearm_quatro[r_forearm_quatro.Count - 1].z.ToString(), text);

    


        if (relativeKinWriter == null)
        {
            relativeKinWriter = new StreamWriter(GetRelativePathKinematic());
            relativeKinWriter.WriteLine(text.header);
            //kinWriter.WriteLine("ID ; Session ; Date ; Time ; SideTested ; Movement ; Reference ; trialNum ; MemoryPosition ; forearm_L_X ; forearm_L_Y ; forearm_L_Z ; forearm_R_X ; forearm_R_Y ; forearm_R_Z ; bicep_L_X ; bicep_L_Y ; bicep_L_Z ; bicep_R_X ; bicep_R_Y ; bicep_R_Z ;controller_L_X ; controller_L_Y ; controller_L_Z ; controller_R_X ; controller_R_Y ; controller_R_Z ; Sternum_X ; Sternum_Y ; Sternum_Z ; HMD_X ; HMD_Y ; HMD_Z ; CamRig_X; CamRig_X ; CamRig_Z ; BicepAvgRef_X ; BicepAvgRef_Y ; BicepAvgRef_Z; ForearmAvgRef_X ; ForearmAvgRef_Y ; ForearmAvgRef_Z ; forearm_L_X_rot ; forearm_L_Y_rot ; forearm_L_Z_rot ; forearm_R_X_rot ; forearm_R_Y_rot ; forearm_R_Z_rot ; bicep_L_X_rot ; bicep_L_Y_rot ; bicep_L_Z_rot ; bicep_R_X_rot ; bicep_R_Y_rot ; bicep_R_Z_rot ;controller_L_X_rot ; controller_L_Y_rot ; controller_L_Z_rot ; controller_R_X_rot ; controller_R_Y_rot ; controller_R_Z_rot ; Sternum_X_rot ; Sternum_Y_rot ; Sternum_Z_rot");
        }
        relativeKinWriter.WriteLine(text.body);
    }

    private void ResetPositionLists()
    {
        POSAs_bicep_l = new List<Vector3>();
        POSAs_bicep_r = new List<Vector3>();
        POSAs_controller_l = new List<Vector3>();
        POSAs_controller_r = new List<Vector3>();
        POSAs_forearm_l = new List<Vector3>();
        POSAs_forearm_r = new List<Vector3>();

        ReturnPOSs_bicep_l = new List<Vector3>();
        ReturnPOSs_bicep_r = new List<Vector3>();
        ReturnPOSs_controller_l = new List<Vector3>();
        ReturnPOSs_controller_r = new List<Vector3>();
        ReturnPOSs_forearm_l = new List<Vector3>();
        ReturnPOSs_forearm_r = new List<Vector3>();
    }
    public void LaterWrite(bool memory)
    {
        AvgLocWriter(memory);
        ResetRelativeLocationLists();
        ResetPositionLists();
    }

    private WriterContent Writing(string header, string newContent, WriterContent toAppend)
    {
        toAppend.header += ";";
        toAppend.body += ";";
        toAppend.header += header;
        toAppend.body += newContent;
        return toAppend;
    }

    private WriterContent Writing(string header, float newContent, WriterContent toAppend)
    {
        toAppend.header += ";";
        toAppend.body += ";";
        toAppend.header += header;
        toAppend.body += newContent;
        return toAppend;
    }

    [System.Serializable]
    public class WriterContent
    {
        public string header;
        public string body;
    }




    private string GetPath()
    {
        if (Application.isEditor)
        {
            return Application.dataPath + "/CSV/" + pID + sessionNum.ToString() + ".csv";
        }

        else
        {
            return Application.persistentDataPath + "/" + pID + sessionNum.ToString() + ".csv";
        }
    }

    private string GetPathKinematic()
    {
        if (Application.isEditor)
        {
            return Application.dataPath + "/CSV/" + pID + sessionNum.ToString() + "_Kinematic.csv";
        }

        else
        {
            return Application.persistentDataPath + "/" + pID + sessionNum.ToString() + "_Kinematic.csv";
        }
    }
    private string GetRelativePathKinematic()
    {
        if (Application.isEditor)
        {
            return Application.dataPath + "/CSV/" + pID + sessionNum.ToString() + "_RelativeKinematic.csv";
        }

        else
        {
            return Application.persistentDataPath + "/" + pID + sessionNum.ToString() + "_RelativeKinematic.csv";
        }
    }

    private string GetPathAverageLoc()
    {
        if (Application.isEditor)
        {
            return Application.dataPath + "/CSV/" + pID + sessionNum.ToString() + "_AvgLoc.csv";
        }

        else
        {
            return Application.persistentDataPath + "/" + pID + sessionNum.ToString() + "_AvgLoc.csv";
        }
    }

    private void PlaceReference()
    {
        referenceSternum.transform.position = sternum.transform.position;
        referenceSternumRot.transform.position = sternum.transform.position;
        referenceHMD.transform.position = HMD.transform.position;

        var rsr = referenceSternumRot.transform;
        rsr.eulerAngles = new Vector3(rsr.eulerAngles.x, sternum.transform.eulerAngles.y, rsr.eulerAngles.z);

        var rhr = referenceHMDRot.transform;
        rhr.eulerAngles = new Vector3(rhr.eulerAngles.x, HMD.transform.eulerAngles.y, rhr.eulerAngles.z);

        Angles.text = "Sternum: " + rsr.eulerAngles.y.ToString("F2") + "HMD: " + rhr.eulerAngles.y.ToString("F2");
    }

    [System.Serializable]
    public class AveragePositionsBilateral
    {
        public Vector3 left;
        public Vector3 right;
    }

    private bool avgRefMeasure;
    public List<AveragePositionsBilateral> bicepPs;
    public List<AveragePositionsBilateral> forearmPs;
    public List<Vector3> sternumPs;
    public List<Vector3> HMDPs;

    public void MeasureAveragedReferences(bool record)
    {
        if (record) avgRefMeasure = true;
        else
        {
            avgRefMeasure = false;
            PlaceAvgRefs();
        }
    }

    public void PlaceAvgRefs()
    {
        //Bicep and forearm average reference placement
        Vector3 lBicep = AverageLeft(bicepPs);
        Vector3 rBicep = AverageRight(bicepPs);
        Vector3 lForearm = AverageLeft(forearmPs);
        Vector3 rForearm = AverageRight(forearmPs);

        referenceBicepAvg.transform.position = Vector3.Lerp(lBicep, rBicep, 0.5f);
        referenceForearmAvg.transform.position = Vector3.Lerp(lForearm, rForearm, 0.5f);

        referenceBicepAvg.transform.LookAt(new Vector3(lBicep.x, referenceBicepAvg.transform.position.y, lBicep.z));
        referenceBicepAvg.transform.Rotate(0, 90, 0);

        referenceForearmAvg.transform.LookAt(new Vector3(lForearm.x, referenceForearmAvg.transform.position.y, lForearm.z));
        referenceForearmAvg.transform.Rotate(0, 90, 0);
        //
        //referenceDos placement (bicep + forearm)
        Vector3 lDos = Vector3.Lerp(lBicep, lForearm, 0.5f);
        Vector3 rDos = Vector3.Lerp(rBicep, rForearm, 0.5f);
        referenceDos.transform.position = Vector3.Lerp(lDos, rDos, 0.5f);

        referenceDos.transform.LookAt(new Vector3(lDos.x, referenceDos.transform.position.y, lDos.z));
        referenceDos.transform.Rotate(0, 90, 0);
        //
        //referenceTres placement (bicep + forearm + sternum)
        Vector3 _sternum = AverageUnilateral(sternumPs);
        referenceTres.transform.position = Vector3.Lerp(referenceDos.transform.position, _sternum, 0.5f);
        //
        //refernce Quatro placement (bicep + forearm + sternum + HMD)
        Vector3 _HMD = AverageUnilateral(HMDPs);
        referenceQuatro.transform.position = Vector3.Lerp(referenceTres.transform.position, _HMD, 0.5f);



        bicepPs = new List<AveragePositionsBilateral>();
        forearmPs = new List<AveragePositionsBilateral>();
        sternumPs = new List<Vector3>();
        HMDPs = new List<Vector3>();
    }

    public List<int> HandednessRoll()
    {
        List<int> hands = new List<int>();
        for (int i = 0; i < 5; i++) hands.Add(0);
        for (int i = 0; i < 5; i++) hands.Add(1);

        int n = hands.Count;

        System.Random _random = new System.Random();
        int myInt;

        for (int i = 0; i < n; i++)
        {
            int r = i + (int)(_random.NextDouble() * (n - i));
            myInt = hands[r];
            hands[r] = hands[i];
            hands[i] = myInt;
        }
        return hands;
    }

    public void HideDummy(bool hide)
    {
        if (hide)
        {
            GetComponent<DummyPoser>().dummy.SetActive(false);
        }
        else
        {
            GetComponent<DummyPoser>().dummy.SetActive(true);
        }

    }

    public void StopWriting()
    {
        distPerFramewriter.Close();
    }

    public void ToggleTrackers()
    {
        if (sternum.GetComponent<MeshRenderer>() != null) sternum.GetComponent<MeshRenderer>().enabled = !sternum.GetComponent<MeshRenderer>().enabled;
        if (leftController.GetComponent<MeshRenderer>() != null) leftController.GetComponent<MeshRenderer>().enabled = !leftController.GetComponent<MeshRenderer>().enabled;
        if (rightController.GetComponent<MeshRenderer>() != null) rightController.GetComponent<MeshRenderer>().enabled = !rightController.GetComponent<MeshRenderer>().enabled;
        if (T1.GetComponent<MeshRenderer>() != null) T1.GetComponent<MeshRenderer>().enabled = !T1.GetComponent<MeshRenderer>().enabled;
        if (T2.GetComponent<MeshRenderer>() != null) T2.GetComponent<MeshRenderer>().enabled = !T2.GetComponent<MeshRenderer>().enabled;
        if (leftBicep.GetComponent<MeshRenderer>() != null) leftBicep.GetComponent<MeshRenderer>().enabled = !leftBicep.GetComponent<MeshRenderer>().enabled;
        if (rightBicep.GetComponent<MeshRenderer>() != null) rightBicep.GetComponent<MeshRenderer>().enabled = !rightBicep.GetComponent<MeshRenderer>().enabled;
        if (leftForearm.GetComponent<MeshRenderer>() != null) leftForearm.GetComponent<MeshRenderer>().enabled = !leftForearm.GetComponent<MeshRenderer>().enabled;
        if (rightForearm.GetComponent<MeshRenderer>() != null) rightForearm.GetComponent<MeshRenderer>().enabled = !rightForearm.GetComponent<MeshRenderer>().enabled;

        foreach (GameObject go in bodies)
        {
            go.GetComponent<MeshRenderer>().enabled = !go.GetComponent<MeshRenderer>().enabled;
        }

    }

    private void DebugTrackers()
    {
        sternum = GameObject.Find("controller");
        T1 = GameObject.Find("controller (1)");
        T2 = GameObject.Find("controller (2)");
        leftBicep = GameObject.Find("controller (3)");
        rightBicep = GameObject.Find("controller (4)");
        leftForearm = GameObject.Find("controller (5)");
        rightForearm = GameObject.Find("controller (6)");
    }



}
