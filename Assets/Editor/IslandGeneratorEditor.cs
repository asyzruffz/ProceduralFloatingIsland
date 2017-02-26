﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IslandGenerator))]
public class IslandGeneratorEditor : Editor {

    public override void OnInspectorGUI () {
        IslandGenerator islandGen = target as IslandGenerator;

        if(DrawDefaultInspector ()) {
            if(islandGen.autoUpdate) {
                islandGen.GenerateIsland ();
            }
        }

        if(GUILayout.Button("Generate")) {
            islandGen.GenerateIsland ();
        }
    }
}
