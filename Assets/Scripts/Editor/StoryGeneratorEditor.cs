using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StoryGenerator))]
public class StoryGeneratorEditor : Editor {

    public override void OnInspectorGUI () {
        StoryGenerator generator = target as StoryGenerator;

        DrawDefaultInspector ();

        GUILayout.Space (10);
        if (GUILayout.Button ("Train Generator")) {
            generator.BuildDatabase ();
        }
    }
}
