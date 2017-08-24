using UnityEngine;
using UnityEngine.UI;

public class Displayer : MonoBehaviour {

    public NameGenerator nameGen;
    public StoryGenerator storyGen;
    public Text displayText;
    
	void Update () {
		if (Input.GetButtonDown("Fire2")) {
			string nameGot;
			do {
				nameGot = nameGen.GenerateName ();
			} while (nameGot.Length < 4);
            nameGot = char.ToUpper (nameGot[0]) + nameGot.Substring (1);
            displayText.text = nameGot + '\n' + storyGen.GenerateStory ();
        }
	}
}
