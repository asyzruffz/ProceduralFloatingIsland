using UnityEngine;

public class GenerateOnStart : MonoBehaviour {

    public IslandGenerator generator;
    public FadeScreen curtain;
    public GameObject overviewCam;

    bool readyPlaying = false;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        SetLevelSeed ();
		clock.Start ();
		generator.GenerateIsland ();
    }

    void Update () {
        if (!readyPlaying && generator.IsDone ()) {
			LoggerTool.Post ("Generated with " + clock.Elapsed () + " seconds.");
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
