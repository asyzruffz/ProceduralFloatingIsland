using UnityEngine;

public class GenerateOnStart : MonoBehaviour {

    public IslandGenerator generator;
    public FadeScreen curtain;

    void Start () {
        ExecutionTimer clock = new ExecutionTimer ();
        clock.Start ();
        generator.GenerateIsland ();
        Debug.Log ("Generate with " + clock.Elapsed () + " seconds.");

        curtain.FadeToColour (Color.clear);
    }
}
