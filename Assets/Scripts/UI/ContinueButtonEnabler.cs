using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContinueButtonEnabler : MonoBehaviour {

    void Start () {
        CheckAnySaveFile ();
	}

    public void CheckAnySaveFile () {
        GetComponent<Button> ().interactable = JsonFile.FilesExistIn (GameController.Instance.saveFolder);
    }
}
