using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour {

	public IslandGenerator generator;

	[Header ("Inputs")]
	public InputField seedInput;
	public InputField widthInput;
	public InputField heightInput;
	public InputField scaleInput;
	public InputField depthInput;
	public InputField altitudeInput;
	public Slider fillInput;

	void Start () {
		widthInput.text = generator.islandData.maxWidth.ToString ();
		heightInput.text = generator.islandData.maxHeight.ToString ();
		scaleInput.text = generator.islandData.tileSize.ToString ();
		depthInput.text = generator.islandData.depth.ToString ();
		altitudeInput.text = generator.islandData.altitude.ToString ();
		fillInput.value = generator.islandData.randomFillPercent;
	}
	
	void UpdateInputValue () {
		generator.islandData.maxWidth = int.Parse(widthInput.text);
		generator.islandData.maxHeight = int.Parse (heightInput.text);
		generator.islandData.tileSize = float.Parse (scaleInput.text);
		generator.islandData.depth = float.Parse (depthInput.text);
		generator.islandData.altitude = float.Parse (altitudeInput.text);
		generator.islandData.randomFillPercent = fillInput.value;
	}

	public void Randomize() {
		generator.useRandomSeed = true;
		UpdateInputValue ();
		generator.GenerateIsland ();
	}

	public void GenerateWithSeed () {
		generator.useRandomSeed = false;
		generator.seed = seedInput.text;
		UpdateInputValue ();
		generator.GenerateIsland ();
	}
}
