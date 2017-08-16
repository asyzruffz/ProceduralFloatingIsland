using UnityEngine;

public class GenerateOnStart : MonoBehaviour {

    public IslandGenerator generator;
    public FadeScreen curtain;
    public GameObject overviewCam;

    bool readyPlaying = false;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        clock.Start ();
        if (GameController.Instance) {
            generator.seed = GameController.Instance.saveData.Seed;
        }
        generator.GenerateIsland ();
    }

    void Update () {
        if (!readyPlaying && generator.IsDone ()) {
            Debug.Log ("Generated with " + clock.Elapsed () + " seconds.");
            readyPlaying = true;

            if (overviewCam) {
                overviewCam.SetActive (false);
            }

            curtain.FadeToColour (Color.clear);
        }
    }
}
