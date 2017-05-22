using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TrackObstacle : MonoBehaviour {

	public bool useother = false;
	public PropPoolManager poolManager;
	public List<Obstacles> obstacles = new List<Obstacles>();
	public List<bool>showobject = new List<bool>();
	public float mainprobab = 0.5f;

	public enum transenum {X=0, Y=1,Z=2, None=3};

	public GameObject gam;
	public Mesh mesh;
	public int PooledAmount=2;


	//Vector3[] vertices;
	List<Vector3> previouspoint=new List<Vector3>();

#if UNITY_EDITOR
	public void createm (){
		GameObject Cube = new GameObject();
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		DestroyImmediate(plane.GetComponent<MeshCollider>());
		Selection.activeObject = SceneView.currentDrawingSceneView;

		Camera sceneCam = SceneView.currentDrawingSceneView.camera;
		Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f,0.5f,10f));
		
		plane.transform.position = spawnPos;
		Cube.transform.position = spawnPos;
		Cube.transform.rotation = Quaternion.identity;
		plane.transform.rotation = Cube.transform.rotation;
		
		plane.transform.parent = Cube.transform;
		Cube.AddComponent<TrackObstacle>();
		Cube.GetComponent<TrackObstacle>().gam = plane;
		Cube.name = "TrackObstacle";
		plane.name = "mesh";
		PrefabUtility.InstantiatePrefab(Cube);
	}
#endif

	void Awake() {
		//Always reset scale of transform object to Unity to avoid bugs
		transform.localScale = new Vector3(1,1,1);
		mesh = gam.GetComponent<MeshFilter>().mesh;
		//vertices=mesh.vertices;
		previouspoint = new List<Vector3>();

		if(!useother){
	
			if(mesh){

				for(int i=0; i<obstacles.Count;++i){
					for(int j=0; j<PooledAmount; ++j){
						obstacles[i].poolTrack.Add(Instantiate(obstacles[i].obstacle,Vector3.zero,Quaternion.identity));
							obstacles[i].poolTrack[j].transform.parent=transform;

							obstacles[i].poolTrack[j].SetActive(false);
					}
				}
				seedobstacle();

			}else{
				Debug.LogError("Mesh Has not been assigned");
			}
		}
	}

	void Start(){
		if(useother==true){
			poolManager=GameObject.FindGameObjectWithTag("PoolManager").GetComponent<PropPoolManager>();
		}
		if(useother==true){
			for(int i=0;i<obstacles.Count; ++i){
				obstacles[i].poolTrack.Clear();
				for(int j=0; j<poolManager.pObstacle[obstacles[i].otherID].pools.Count; ++j){
					obstacles[i].poolTrack.Add(poolManager.pObstacle[obstacles[i].otherID].pools[j]);
					
				}
			}
			seedobstacle();
		}
	}

	public void Recycle(){
		for(int i=0; i<obstacles.Count;++i){
			for(int j=0; j<obstacles[i].poolTrack.Count; ++j){
				obstacles[i].poolTrack[j].SetActive(false);
			}
		}
		previouspoint.Clear();

	}
	public void seedobstacle() {
		List<float> probabs=new List<float>();
		for(int i=0; i<obstacles.Count;++i){
			probabs.Add(obstacles[i].probability);
		}
		Vector3[] verts = mesh.vertices;
		
		int[] indices = mesh.triangles;

		for(int i = 0; i < mesh.triangles.Length;)
			
		{
			
			Vector3 P1 = verts[indices[i++]];
			Vector3 P2 = verts[indices[i++]];
			Vector3 P3 = verts[indices[i++]];
			
			Vector3 pos = gam.transform.TransformPoint((P1+P2+P3)/3);

			Vector3 n1 = verts[indices[i++]];
			Vector3 n2 = verts[indices[i++]];
			Vector3 n3 = verts[indices[i++]];

			Vector3 rot = gam.transform.TransformDirection((n1+n2+n3)/3);

			int Randtrack = Probability(probabs, obstacles.Count);
			transenum trans = obstacles[Randtrack].trans;

            bool b = true;
            for (int j = 0; j<previouspoint.Count; j++){
			    if(Vector3.Distance(pos, previouspoint[j]) < obstacles[Randtrack].sepdist){
					b = false;
					break;
				}
			}

			if(b==true){

				if(Random.value < mainprobab){
					 
					GameObject temp = CheckPool(Randtrack);

					temp.transform.parent = transform;
                    temp.transform.position = pos;
                    temp.transform.localRotation = Quaternion.identity;
						
                    switch (trans){
						case transenum.X:
							temp.transform.localPosition = new Vector3(0, temp.transform.localPosition.y, temp.transform.localPosition.z);
							break;
						case transenum.Y:
							temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, 0, temp.transform.localPosition.z);
							break;
						case transenum.Z:
							temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, 0);
							break;
						case transenum.None:

							break;
						default:
							break;
					}

					for(int j=0; j<obstacles[Randtrack].procedures.Count;++j){
						doProcedure(obstacles[Randtrack].procedures[j],temp,rot);
					}
	
					previouspoint.Add(temp.transform.position);
					temp.SetActive(true);

				}
			}
		}
	}

	public GameObject CheckPool (int randtrack){
		for(int i=0; i<obstacles[randtrack].poolTrack.Count;++i){
		if(obstacles[randtrack].poolTrack[i].activeInHierarchy==false){
				return obstacles[randtrack].poolTrack[i];
			}
		}
		if(useother==false){
		obstacles[randtrack].poolTrack.Add(Instantiate(obstacles[randtrack].obstacle, Vector3.zero, Quaternion.identity));
		obstacles[randtrack].poolTrack[obstacles[randtrack].poolTrack.Count-1].transform.parent=transform;
		}else {
			poolManager.pObstacle[obstacles[randtrack].otherID].pools.Add(Instantiate(poolManager.pObstacle[obstacles[randtrack].otherID].obstacle, Vector3.zero, Quaternion.identity));
			obstacles[randtrack].poolTrack.Add(poolManager.pObstacle[obstacles[randtrack].otherID].pools[poolManager.pObstacle[obstacles[randtrack].otherID].pools.Count-1]);
			obstacles[randtrack].poolTrack[obstacles[randtrack].poolTrack.Count-1].SetActive(false);
			obstacles[randtrack].poolTrack[obstacles[randtrack].poolTrack.Count-1].transform.parent = transform;
			return obstacles[randtrack].poolTrack[obstacles[randtrack].poolTrack.Count-1];

		}
		return obstacles[randtrack].poolTrack[obstacles[randtrack].poolTrack.Count-1];
	}

	public void doProcedure(Procedures p,GameObject temp,Vector3 rot){
		switch(p.id){
			case 0:
				temp.transform.up=rot;
				break;
			case 1:
				temp.transform.Rotate(p.rot);
				break;
			case 2:
				temp.transform.localPosition+=p.add;
				break;
			case 3:
				float x=Random.Range(p.RandomRotmin.x,p.RandomRotmax.x);
				float y=Random.Range(p.RandomRotmin.y,p.RandomRotmax.y);
				float z=Random.Range(p.RandomRotmin.z,p.RandomRotmax.z);
				temp.transform.Rotate(new Vector3(x,y,z));
				break;
			case 4:
				float x1=Random.Range(p.RandomPosmin.x,p.RandomPosmax.x);
				float y1=Random.Range(p.RandomPosmin.y,p.RandomPosmax.y);
				float z1=Random.Range(p.RandomPosmin.z,p.RandomPosmax.z);
				temp.transform.localPosition+=new Vector3(x1,y1,z1);
				break;
			case 5:
				temp.transform.LookAt(p.rotaxis.position);
				break;
			case 6:
				float x2=Random.Range(p.Randomtransmin.x,p.Randomtransmax.x);
				float y2=Random.Range(p.Randomtransmin.y,p.Randomtransmax.y);
				float z2=Random.Range(p.Randomtransmin.z,p.Randomtransmax.z);
				temp.transform.Translate(x2,y2,z2);
				break;
			case 7:
				temp.transform.Translate(p.TransformVector);
				break;
			case 8:
				temp.transform.rotation=p.rotatevector.rotation;
			
				break;

		}
	}

	//Maths functions section
	int Probability(List<float> probabs, int size){
		float sum = 0f;
		for (int i = 0; i < size; ++i) {
			sum += probabs[i];
		}
		float value = sum * Random.value;
		
		float sum2 = probabs[0];
		for (int i = 0; i < size; ++i){
			if(sum2 > value){
				return 0;
			} else if(size-i == 1){
				return i;
			}
			else if(value>sum2 && value<sum2+probabs[i+1]){
				return i+1;
			}
			sum2 += probabs[i+1];
		}

		return 0;
	}
}


[System.Serializable]
public class Obstacles {
	public List<GameObject> poolTrack;
	public GameObject obstacle;
	public int otherID = 0; 
	public float probability = 1;
	public float sepdist = 2;
	public TrackObstacle.transenum trans = TrackObstacle.transenum.None;
	public List<Procedures> procedures = new List<Procedures>();
	public bool hasNormals = false;
	public List<bool> showProcedures = new List<bool>();
}

[System.Serializable]
public class Procedures {
	public int id = 0;
	public Vector3 rot;
	public Transform rotaxis;
	public Vector3 add;
	public Transform rotatevector;
	public Vector3 Randomtransmin = Vector3.zero;
	public Vector3 Randomtransmax = Vector3.one; 
	public Vector3 RandomRotmin = Vector3.zero;
	public Vector3 RandomRotmax = Vector3.one;
	public Vector3 TransformVector = Vector3.zero;
	public Vector3 RandomPosmin = Vector3.zero;
	public Vector3 RandomPosmax = Vector3.one;

}
