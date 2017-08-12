using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour {

    public IslandGenerator islandGenerator;

    public FadeScreen curtain;
    
	void Start () {
        islandGenerator.GenerateIsland ();
    }
	
	void Update () {
		
	}
}
