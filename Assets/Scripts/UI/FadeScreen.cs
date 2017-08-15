using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour {

	public float fadeTime = 1;
    public bool playOnStart;

	Image img;
	float timer = 0;
	bool startFading = false;
    Color startColour;
    Color endColour;

    void Awake () {
		img = GetComponent<Image> ();
        startFading = playOnStart;
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

    public bool IsFading() {
        return startFading;
    }
}
