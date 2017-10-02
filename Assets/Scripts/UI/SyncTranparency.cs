using UnityEngine;
using UnityEngine.UI;

public class SyncTranparency : MonoBehaviour {

    public Image syncTarget;
    [Range(0, 1)]
    public float disappearAt = 0.5f;

    Image sourceImage;

    void Start () {
        sourceImage = GetComponent<Image> ();
    }
	
	void Update () {
        float alpha = Mathf.Clamp01 (Mathf.InverseLerp (disappearAt, 1, sourceImage.color.a));
        syncTarget.color = new Color(syncTarget.color.r, syncTarget.color.g, syncTarget.color.b, alpha);
    }
}
