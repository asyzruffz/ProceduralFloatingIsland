using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    
    AsyncOperation loadingOperation;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        // #TODO Set which level to load

        clock.Start ();
        StartCoroutine (LoadGameLevelWithProgress ("GameplayTest"));
    }
	
	void Update () {
        
	}

    IEnumerator LoadGameLevelWithProgress (string levelName) {
        yield return new WaitForSeconds (0.5f);

        loadingOperation = SceneManager.LoadSceneAsync (levelName);
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone) {
            Debug.Log ("Progress " + (loadingOperation.progress * 100) + "%");

            if (loadingOperation.progress >= 0.9f) {
                loadingOperation.allowSceneActivation = true;
                Debug.Log ("Load with " + clock.Elapsed () + " seconds.");
            }

            yield return null;
        }
    }
}
