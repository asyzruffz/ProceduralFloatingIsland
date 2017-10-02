using UnityEngine;

public class ExecutionTimer {

    float startTime;
	
	public void Start () {
        startTime = Time.realtimeSinceStartup;
    }
	
	public float Elapsed () {
		return Time.realtimeSinceStartup - startTime;
    }
}
