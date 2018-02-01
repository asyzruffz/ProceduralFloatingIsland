using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToGenerate : MonoBehaviour {

    public IslandGenerator islandGenerator;
	public bool useRandomSeed = true;

	ExecutionTimer clock = new ExecutionTimer ();
	bool startedRecording = false;

	void Update () {
        if (Input.GetButtonDown ("Fire1")) {
			LoggerTool.Post ("Generate island from click");
			bool prevRandSet = islandGenerator.useRandomSeed;
            islandGenerator.useRandomSeed = useRandomSeed;
			startedRecording = true;
			clock.Start ();
			islandGenerator.GenerateIsland ();
            islandGenerator.useRandomSeed = prevRandSet;
        }

		if (startedRecording && islandGenerator.IsDone()) {
			startedRecording = false;
			LoggerTool.Post ("Generated with " + clock.Elapsed () + " seconds.");
		}
    }
}
