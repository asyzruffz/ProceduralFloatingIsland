using UnityEngine;

[CreateAssetMenu (fileName = "NoiseData", menuName = "Procedural/Noise Data")]
public class NoiseData : UpdatableData {
    
    public float scale = 1;
    public int octave = 1;
    public float lacunarity = 2;
    [Range (0, 1)]
    public float persistance = 1;
    public Vector2 offset;

    protected override void OnValidate () {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octave < 0) {
            octave = 0;
        }

        base.OnValidate ();
    }
}
