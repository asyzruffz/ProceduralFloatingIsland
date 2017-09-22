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

    protected override void SingletonAwake () {
        DontDestroyOnLoad (gameObject);

		InitializeDatabase ();
	}
    
    void Start () {
		
	}
	
	void Update () {
		
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

    void GoToSelectLevel () {
        SceneManager.LoadScene ("LevelSelection");
        portableCanvas.HideItem (0);
    }

    public void GoToIsland () {
        SceneManager.LoadScene ("LoadingScreen");
    }

    public void BackToMenu () {
        SceneManager.LoadScene ("MainMenu");
    }

    public void ExitGame () {
        Application.Quit ();
    }

	void InitializeDatabase () {
		NameGenerator nameGen = GetComponent<NameGenerator> ();
		if (!JsonFile.FilesExistIn("NameGenerator")) {
			Debug.Log ("Initializing Database");
			nameGen.BuildDatabase ();
		}
	}
}
