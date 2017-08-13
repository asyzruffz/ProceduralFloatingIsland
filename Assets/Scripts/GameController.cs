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
        saveData.Name = "Arbitrary";
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
