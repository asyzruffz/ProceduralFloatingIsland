using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {

    public FadeScreen curtain;

    AsyncOperation loadingOperation;
    bool startFading = false;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        // #TODO Set which level to load

        clock.Start ();
        StartCoroutine (LoadGameLevelWithProgress ("GameplayTest"));
    }
	
	void Update () {
        if (startFading && !curtain.IsFading ()) {
            Debug.Log ("Load with " + clock.Elapsed () + " seconds.");
            startFading = false;
            curtain.FadeToColour (Color.clear);
        }
	}

    IEnumerator LoadGameLevelWithProgress (string levelName) {
        yield return new WaitForSeconds (1);

        loadingOperation = SceneManager.LoadSceneAsync (levelName);
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone) {
            Debug.Log ("Progress " + (loadingOperation.progress * 100) + "%");

            if (loadingOperation.progress >= 0.99f) {
                startFading = true;
                yield return new WaitForSeconds (curtain.fadeTime);
                loadingOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
