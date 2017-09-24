using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController> {

    [Header ("Save Info")]
    public string saveFolder = "Saves";
    public GameSave saveData;

    [Header ("Gameplay Info")]
    public int level;

    [Header ("References")]
    public SaveSlots slotsHandler;
    public MenuDisplay portableCanvas;

    [HideInInspector]
    public bool isPaused;
    [HideInInspector]
    public bool isPausable;

    protected override void SingletonAwake () {
        DontDestroyOnLoad (gameObject);

		InitializeDatabase ();
	}
    
    void Start () {
		
	}
	
	void Update () {
        if (Input.GetButtonDown ("Cancel")) {
            TogglePause ();
        }
    }

    void InitializeDatabase () {
        NameGenerator nameGen = GetComponent<NameGenerator> ();
        if (!JsonFile.FilesExistIn ("NameGenerator")) {
            Debug.Log ("Initializing Database");
            nameGen.BuildDatabase ();
        }
    }

    public void CreateNewGame () {
        string username;
        do {
            username = GetComponent<NameGenerator> ().GenerateName ();
            username = char.ToUpper (username[0]) + username.Substring (1); // Capitalize first letter
        } while (username.Length > 0 && username.Length < 4);

        saveData.Name = username.Length > 0 ? username : "Anonymous";
        saveData.Seed = DateTime.Now.ToString ("yy-MM-dd-HH-mm-ss");
    }

    public void InstructToGoLevelSelect () {
        slotsHandler.AddFurtherAction (GoToSelectLevel);
    }

    public void GoToSelectLevel () {
        SceneManager.LoadScene ("LevelSelection");
        isPausable = false;
        portableCanvas.HideItem (0);
    }

    public void GoToIsland () {
        SceneManager.LoadScene ("LoadingScreen");
        isPausable = true;
    }

    public void BackToMenu () {
        SceneManager.LoadScene ("MainMenu");
        isPausable = false;
        level = 0;
        portableCanvas.ShowItem (3);
    }
    
    public void TogglePause () {
        if (isPausable) {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;

            if (isPaused) {
                portableCanvas.ShowItem (2);
            } else {
                portableCanvas.HideItem (2);
            }
        }
    }

    public void ExitGame () {
        Application.Quit ();
    }
}
