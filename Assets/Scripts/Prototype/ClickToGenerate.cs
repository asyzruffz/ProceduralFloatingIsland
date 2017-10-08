﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToGenerate : MonoBehaviour {

    public IslandGenerator islandGenerator;

	void Update () {
        if (Input.GetButtonDown ("Fire1")) {
            bool prevRandSet = islandGenerator.useRandomSeed;
            islandGenerator.useRandomSeed = true;
            islandGenerator.GenerateIsland ();
            islandGenerator.useRandomSeed = prevRandSet;
        }
    }
}
