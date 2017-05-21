using UnityEngine;

public class UI : MonoBehaviour {

	public int x;
	public int y;
	public string text = "Reset";
	public TrackObstacle trackObstacle;

	void OnGUI() {
		if(GUI.Button(new Rect(Screen.height/x,Screen.width/y,100,50),text)){
			trackObstacle.Recycle();
			trackObstacle.seedobstacle();
		}
	}
}
