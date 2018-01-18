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
    public GameObject player;

    [Header ("References")]
    public SaveSlots slotsHandler;
    public MenuDisplay portableCanvas;
    public ContinueButtonEnabler contButton;

    [Header ("Status")]
    public bool isPaused;
    public bool isPausable;

    protected override void SingletonAwake () {
        DontDestroyOnLoad (gameObject);

		InitializeDatabase ();
	}
    
    void Start () {
		LoggerTool.Instance.Post ("Test logging..");
	}
	
	void Update () {
        if (Input.GetButtonDown ("Cancel")) {
            TogglePausePanel ();
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

    public void GoToPrototype () {
        SceneManager.LoadScene ("Prototype");
        portableCanvas.HideItem (3);
        portableCanvas.HideItem (4);
    }

    public void BackToMenu () {
        SceneManager.LoadScene ("MainMenu");
        isPausable = false;
        level = 0;

        contButton.CheckAnySaveFile ();
        portableCanvas.ShowItem (3);
        portableCanvas.ShowItem (4);
    }
    
    public void TogglePausePanel () {
        if (!Inventory.AnyInventoryOpened) {
            TogglePause (!isPaused);
        }

        if (isPaused) {
            portableCanvas.ShowItem (2);
            if (player) {
                player.GetComponent<PlayerInventory> ().CloseAllPlayerPanels ();
                isPaused = true;
            }
        } else {
            portableCanvas.HideItem (2);
        }
    }

    public void TogglePause (bool set) {
        if (isPausable) {
            isPaused = set;
            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    public void ExitGame () {
        Application.Quit ();
    }
}
