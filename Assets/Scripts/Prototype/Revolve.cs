using UnityEngine;

public class Revolve : MonoBehaviour {

	public float rotateSpeed;
	
	void Update () {
		transform.Rotate (Vector3.up, rotateSpeed * Time.deltaTime);
	}
}
