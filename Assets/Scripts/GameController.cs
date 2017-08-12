using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : Singleton<GameController> {

    protected override void SingletonAwake () {
        DontDestroyOnLoad (gameObject);
    }
    
    void Start () {
		// #TODO Check save games to enable Continue button
	}
	
	void Update () {
		
	}

    public void ExitGame () {
        Application.Quit ();
    }
}
