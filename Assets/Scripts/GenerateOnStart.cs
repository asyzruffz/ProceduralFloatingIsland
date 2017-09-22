using UnityEngine;

public class GenerateOnStart : MonoBehaviour {

    public IslandGenerator generator;
    public FadeScreen curtain;
    public GameObject overviewCam;

    bool readyPlaying = false;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        clock.Start ();
        SetLevelSeed ();
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

    void SetLevelSeed () {
        if (GameController.Instance) {
            string thisSeed = GameController.Instance.saveData.Seed;
            thisSeed += ":" + GameController.Instance.level.ToString ();

            generator.seed = thisSeed;
        }
    }
}
