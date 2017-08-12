using UnityEngine;
using UnityEngine.UI;

public class SyncTranparency : MonoBehaviour {

    public Image syncTarget;

    Image sourceImage;

    void Start () {
        sourceImage = GetComponent<Image> ();
    }
	
	void Update () {
        syncTarget.color = new Color(syncTarget.color.r, syncTarget.color.g, syncTarget.color.b, sourceImage.color.a);
    }
}
