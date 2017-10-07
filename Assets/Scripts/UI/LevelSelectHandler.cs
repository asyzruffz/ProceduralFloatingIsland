using UnityEngine;

public class LevelSelectHandler : MonoBehaviour {

    ScrollView view;

    void Start () {
        view = GetComponent<ScrollView> ();

        if (GameController.Instance) {
            view.SetViewAt(GameController.Instance.level);
        }
    }

	public void BackToMenu () {
        if (GameController.Instance) {
            GameController.Instance.BackToMenu ();
        }
    }

    public void GoToIsland () {
        if (GameController.Instance) {
            GameController.Instance.level = view.GetCurrentIndexViewed ();
            GameController.Instance.GoToIsland ();
        }
    }
}
