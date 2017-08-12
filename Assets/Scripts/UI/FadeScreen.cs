using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeScreen : MonoBehaviour {

	public float fadeTime = 1;

	Image img;
	float timer = 0;
	bool startFading = true;
    Color startColour;
    Color endColour;

    void Start () {
		img = GetComponent<Image> ();
	}
	
	void Update () {
		if (startFading) {
			timer += Time.deltaTime;
			float t = Mathf.Clamp01 (timer / fadeTime);
			img.color = Color.Lerp (startColour, endColour, t);

			if (t >= 1) {
				startFading = false;
				timer = 0;
			}
		}
	}

	public void FadeToColour(Color colour) {
		startFading = true;
        startColour = img.color;
        endColour = colour;
    }
}
