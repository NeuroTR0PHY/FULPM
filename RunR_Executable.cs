using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
//using Microsoft.R.Host.Client;
using System;
using C = System.Console;

public class RunR_Executable : MonoBehaviour {


	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.R))
        {
            run_cmd();
        }
	}

    private void ExecuteR()
    {

    }
    private void run_cmd()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "G:/Nathan/BoxSync/Box Sync/ProprioceptionProject/LPM_Data/R_Project_LPM/R_Project_LPM/RunR.py";
       // start.Arguments = string.Format("{0} {1}", cmd, args);
        start.UseShellExecute = true;
        start.RedirectStandardOutput = false;
    }


}
