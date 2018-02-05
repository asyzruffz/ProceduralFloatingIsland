using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    
    AsyncOperation loadingOperation;
    ExecutionTimer clock = new ExecutionTimer ();

    void Start () {
        string levelToPlay = SelectLevel ();

        clock.Start ();
        StartCoroutine (LoadGameLevelWithProgress (levelToPlay));
    }
	
	string SelectLevel () {
        if (GameController.Instance && GameController.Instance.level == 0) {
            return "Tutorial";
        } else {
            return "GameplayTest";
        }
    }

    IEnumerator LoadGameLevelWithProgress (string levelName) {
        yield return new WaitForSecondsRealtime (0.5f);

        loadingOperation = SceneManager.LoadSceneAsync (levelName);
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone) {
			LoggerTool.Post ("Loading progress " + (loadingOperation.progress * 100) + "%");

            if (loadingOperation.progress >= 0.9f) {
                loadingOperation.allowSceneActivation = true;
				LoggerTool.Post ("Loaded level [" + levelName + "] with " + clock.Elapsed () + " seconds.");
            }

            yield return null;
        }
    }
}
