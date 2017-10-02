using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContinueButtonEnabler : MonoBehaviour {

    void Start () {
        GetComponent<Button> ().interactable = JsonFile.FilesExistIn (GameController.Instance.saveFolder);
	}
}
