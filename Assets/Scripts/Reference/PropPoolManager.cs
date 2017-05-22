using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[AddComponentMenu("Infinite Runner Tool/Pool Manager")]
public class PropPoolManager : MonoBehaviour {

	public List<poolObstacle> pObstacle;
	public List<string> Names;
	public int pooledAmount = 3;
	public List<bool> foldout;

	void Awake () {
		for(int i = 0; i < pObstacle.Count; i++) {
			for(int j = 0; j < pooledAmount; j++) {
				pObstacle[i].pools.Add(Instantiate(pObstacle[i].obstacle, Vector3.zero, Quaternion.identity));
				pObstacle[i].pools[j].transform.parent = transform;
				pObstacle[i].pools[j].SetActive(false);
			}
		}
	}
}

[System.Serializable]
public class poolObstacle {
	public GameObject obstacle;
	public List<GameObject> pools;
}
