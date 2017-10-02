using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NameGenerator))]
public class NameGeneratorEditor : Editor {

    public override void OnInspectorGUI () {
        NameGenerator generator = target as NameGenerator;

        DrawDefaultInspector ();

        GUILayout.Space (10);
        if (GUILayout.Button ("Train Generator")) {
            generator.BuildDatabase ();
        }
    }
}
