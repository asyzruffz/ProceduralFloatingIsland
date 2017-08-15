using UnityEngine;

public class LevelSelectHandler : MonoBehaviour {

    ScrollView view;

    void Start () {
        view = GetComponent<ScrollView> ();
    }

	public void BackToMenu () {
        GameController.Instance.BackToMenu ();
    }

    public void GoToIsland () {
        GameController.Instance.level = view.GetCurrentIndexViewed () + 1;
        GameController.Instance.GoToIsland ();
    }
}
