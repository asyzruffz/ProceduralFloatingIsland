using UnityEngine;

public class GenerateOnStart : MonoBehaviour {

    public IslandGenerator generator;
    public FadeScreen curtain;
    public GameObject overviewCam;

    bool readyPlaying = false;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        clock.Start ();
        generator.GenerateIsland ();
    }

    void Update () {
        if (!readyPlaying && generator.IsDone ()) {
            Debug.Log ("Generated with " + clock.Elapsed () + " seconds.");
            readyPlaying = true;
            overviewCam.SetActive (false);
            curtain.FadeToColour (Color.clear);
        }
    }
}
