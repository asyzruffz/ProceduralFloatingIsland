using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : Singleton<GameController> {

    public string saveFolder = "Saves";
    public GameSave saveData;

    protected override void SingletonAwake () {
        DontDestroyOnLoad (gameObject);
    }
    
    void Start () {
		
	}
	
	void Update () {
		
	}
    
    public void ExitGame () {
        Application.Quit ();
    }
}
