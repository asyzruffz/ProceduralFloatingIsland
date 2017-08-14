using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController> {

    public string saveFolder = "Saves";
    public GameSave saveData;

    [Header ("References")]
    public SaveSlots slotsHandler;
    public GameObject portableCanvas;

    protected override void SingletonAwake () {
        DontDestroyOnLoad (gameObject);
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
        portableCanvas.SetActive (false);
    }

    public void ExitGame () {
        Application.Quit ();
    }
}
