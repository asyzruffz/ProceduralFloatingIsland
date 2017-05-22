using UnityEngine;
using UnityEngine.UI;

public class DisplaySliderValue : MonoBehaviour {

	public Slider slider;
    public bool percentageSymbol;

	Text displayText;

	void Start () {
		displayText = GetComponent<Text> ();
	}
	
	void Update () {
        if (percentageSymbol) {
            displayText.text = string.Format ("{0:0.##} % ", slider.value);
        } else {
            displayText.text = string.Format ("{0:0.##} ", slider.value);
        }
	}
}
