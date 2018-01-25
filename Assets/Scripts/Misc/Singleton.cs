using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
	
	public static T Instance { get; private set; }
	
	void Awake() {
		if (Instance == null) {
			Instance = (T)this;
			SingletonAwake();
		} else {
			Debug.LogWarning ("Duplicate singleton " + GetType (), this);
			LoggerTool.Post ("Duplicate singleton " + GetType (), false);
			Destroy (gameObject);
		}
	}
	
	protected virtual void SingletonAwake() { }
}
