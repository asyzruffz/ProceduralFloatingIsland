using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IslandGenerator))]
public class IslandGeneratorEditor : Editor {

    public override void OnInspectorGUI () {
        IslandGenerator islandGen = target as IslandGenerator;

        if(DrawDefaultInspector ()) {
            if(islandGen.autoUpdate) {
				LoggerTool.Post ("Generate island from Editor");
				islandGen.GenerateIsland (false);
            }
        }

		GUILayout.Space (10);
		if (GUILayout.Button("Generate")) {
			LoggerTool.Post ("Generate island from Editor");
			islandGen.GenerateIsland (false);
        }
    }
}
