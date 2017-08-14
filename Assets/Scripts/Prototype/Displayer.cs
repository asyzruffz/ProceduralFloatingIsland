﻿using UnityEngine;
using UnityEngine.UI;

public class Displayer : MonoBehaviour {

    public NameGenerator nameGen;
    public StoryGenerator storyGen;
    public Text displayText;
    
	void Update () {
		if (Input.GetButtonDown("Fire2")) {
            string nameGot = nameGen.GenerateName ();
            nameGot = char.ToUpper (nameGot[0]) + nameGot.Substring (1);
            displayText.text = nameGot + '\n' + storyGen.GenerateStory ();
        }
	}
}
