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
    public Toggle colliderInput;
    public Toggle flatInput;
    public Toggle treesInput;
    public InputField noiseScaleInput;
    public InputField octaveInput;
    public InputField lacunarityInput;
    public Slider persistenceInput;

    void Start () {
		widthInput.text = generator.islandData.maxWidth.ToString ();
		heightInput.text = generator.islandData.maxHeight.ToString ();
		scaleInput.text = generator.islandData.tileSize.ToString ();
		depthInput.text = generator.islandData.depth.ToString ();
		altitudeInput.text = generator.islandData.altitude.ToString ();
		fillInput.value = generator.islandData.randomFillPercent;
        colliderInput.isOn = generator.withCollider;
        flatInput.isOn = generator.flatShading;
        treesInput.isOn = generator.decorateTerrain;
        noiseScaleInput.text = generator.surfaceNoiseData.scale.ToString ();
        octaveInput.text = generator.surfaceNoiseData.octave.ToString ();
        lacunarityInput.text = generator.surfaceNoiseData.lacunarity.ToString ();
        persistenceInput.value = generator.surfaceNoiseData.persistance;
    }
	
	void UpdateInputValue () {
		generator.islandData.maxWidth = int.Parse (widthInput.text);
		generator.islandData.maxHeight = int.Parse (heightInput.text);
		generator.islandData.tileSize = float.Parse (scaleInput.text);
		generator.islandData.depth = float.Parse (depthInput.text);
		generator.islandData.altitude = float.Parse (altitudeInput.text);
		generator.islandData.randomFillPercent = fillInput.value;
        generator.withCollider = colliderInput.isOn;
        generator.flatShading = flatInput.isOn;
        generator.decorateTerrain = treesInput.isOn;
        generator.surfaceNoiseData.scale = float.Parse (noiseScaleInput.text);
        generator.surfaceNoiseData.octave = int.Parse (octaveInput.text);
        generator.surfaceNoiseData.lacunarity = float.Parse (lacunarityInput.text);
        generator.surfaceNoiseData.persistance = persistenceInput.value;
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

    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit ();
        }
    }
}
