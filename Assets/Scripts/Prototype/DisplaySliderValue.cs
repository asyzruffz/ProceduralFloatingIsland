using UnityEngine;
using UnityEngine.UI;

public class DisplaySliderValue : MonoBehaviour {

	public Slider slider;
	Text displayText;

	void Start () {
		displayText = GetComponent<Text> ();
	}
	
	void Update () {
		displayText.text = string.Format("{0:0.##} % ", slider.value);
	}
}
