using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(TrackObstacle))]
public class TrackObstacleEditor : Editor {
	
	[MenuItem("GameObject/Create Other/Infinite Runner Tool/Track Obstacle mesh")]
	static void CreatePrefab()
	{
		TrackObstacle n = new TrackObstacle();
		n.createm();
	}


	private SerializedObject trackobstacleobject;
	private TrackObstacle trackobstacle;

	void OnEnable () {
		trackobstacleobject = new SerializedObject(target);
		trackobstacle = (TrackObstacle)target;
	}

	public override void OnInspectorGUI () {
		trackobstacleobject.Update();
		EditorGUILayout.BeginVertical();
		trackobstacle.useother = EditorGUILayout.Toggle("Use Other Pool Manager",trackobstacle.useother);
		trackobstacle.gam = (GameObject)EditorGUILayout.ObjectField("Mesh GameObject",trackobstacle.gam, typeof(GameObject), true);

		if (trackobstacle.useother) {
			if (GameObject.FindWithTag ("PoolManager")) {
				trackobstacle.poolManager = GameObject.FindGameObjectWithTag ("PoolManager").GetComponent<PropPoolManager> ();
			} else {
				GUILayout.Label ("Please Tag the Pool Manager as PoolManager");
			}
		} else {
			EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Pooled Amount ");
				trackobstacle.PooledAmount = EditorGUILayout.IntField (trackobstacle.PooledAmount);
			EditorGUILayout.EndHorizontal ();
		}

		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Obstacle Appearence probability");
			trackobstacle.mainprobab = EditorGUILayout.Slider(trackobstacle.mainprobab,0,1);
		EditorGUILayout.EndHorizontal();


		int i = 0;
		foreach(Obstacles ob in trackobstacle.obstacles){
			trackobstacle.showobject[i] = EditorGUILayout.Foldout(trackobstacle.showobject[i], "Obstacle " + (i+1));

			if (trackobstacle.showobject[i]) {

				if(trackobstacle.useother) {
					ob.obstacle = (GameObject)EditorGUILayout.ObjectField ("Obstacle", ob.obstacle, typeof (GameObject), true);

				} else {
					if (trackobstacle.poolManager) {
						string[] s = new string[trackobstacle.poolManager.Names.Count];
						for (int j = 0; j < trackobstacle.poolManager.Names.Count; ++j) {
							s[j] = trackobstacle.poolManager.Names[j];
						}

						int[] num = new int[trackobstacle.poolManager.Names.Count];
						for (int j = 0; j < trackobstacle.poolManager.Names.Count; ++j) {
							num[j] = j;
						}

						ob.otherID = EditorGUILayout.IntPopup ("Obstacle Object", ob.otherID, s, num);
						
					} else {
						GUILayout.Label ("Please assign a Pool Manager");
					}
				}

				EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Probability");
					ob.probability = EditorGUILayout.Slider(ob.probability, 0, 1);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Seperation Distance");
					ob.sepdist = EditorGUILayout.FloatField(ob.sepdist);
				EditorGUILayout.EndHorizontal();

				ob.trans = (TrackObstacle.transenum)EditorGUILayout.EnumPopup("Object Fixed Transform ", ob.trans);

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Procedures");
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical();
				EditorGUILayout.Space();
				int k=0;
				foreach(Procedures p in ob.procedures){
				//Procedure Section
					string [] str= new string[9] {
						"Align With Mesh Normals",
						"Rotate With Vector",
						"Move With Vector",
						"Rotate With Random Vector",
						"Move With Random Vector",
						"Look At Transform",
						"Translate With Random Vector",
						"Translate With Vector",
						"Rotate With Other Transform"
					};

					ob.showProcedures[k] = EditorGUILayout.Foldout(ob.showProcedures[k], str[p.id]);
					if(ob.showProcedures[k]) {
						int[] pop = new int[9]{0,1,2,3,4,5,6,7,8};
						p.id = EditorGUILayout.IntPopup(p.id,str,pop);

						switch (p.id) {
						case 0:
							GUILayout.Label("Align with mesh Normals");
							break;
						case 1:
							p.rot = EditorGUILayout.Vector3Field("Rotation Vector", p.rot);
							break;
						case 2:
							p.add = EditorGUILayout.Vector3Field("Move Vector ", p.add);
							break;
						case 3:
							GUILayout.Label("Random Rotation Range");
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							p.RandomRotmin = EditorGUILayout.Vector3Field("Min Value",p.RandomRotmin);
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							p.RandomRotmax=EditorGUILayout.Vector3Field("Max Value",p.RandomRotmax);
							EditorGUILayout.EndHorizontal();
							break;
						case 4:
							GUILayout.Label("Random Vector Range");
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							p.RandomPosmin = EditorGUILayout.Vector3Field("Min Value",p.RandomPosmin);
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							p.RandomPosmax = EditorGUILayout.Vector3Field("Max Value",p.RandomPosmax);
							EditorGUILayout.EndHorizontal();
							break;
						case 5:
							p.rotaxis = (Transform)EditorGUILayout.ObjectField("Object Look Rotation ",p.rotaxis,typeof(Transform),true);
							break;
						case 6:
							GUILayout.Label("Random Transform Range");
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							p.Randomtransmin = EditorGUILayout.Vector3Field("Min Value",p.Randomtransmin);
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							p.Randomtransmax = EditorGUILayout.Vector3Field("Max Value",p.Randomtransmax);
							EditorGUILayout.EndHorizontal();
							break;
						case 7:
							p.TransformVector = EditorGUILayout.Vector3Field("Translate Vector ",p.TransformVector);
							break;
						case 8:
							p.rotatevector = (Transform)EditorGUILayout.ObjectField("Rotate Transform",p.rotatevector, typeof(Transform), true);
							break;
						}

						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.Space();
						if(GUILayout.Button("Remove Procedure")){
							ob.procedures.RemoveAt(k);
							ob.showProcedures.RemoveAt(k);
							break;
						}
						EditorGUILayout.EndHorizontal();
					}

					k++;
				}
				EditorGUILayout.Space();
				if(GUILayout.Button("Add new Procedure")) {
					trackobstacle.obstacles[i].procedures.Add(new Procedures());
					trackobstacle.obstacles[i].showProcedures.Add(false);
				}


				
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if(GUILayout.Button("Remove Obstacle")){
					trackobstacle.obstacles.RemoveAt(i);

					trackobstacle.showobject.RemoveAt(i);
					break;
				
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}

			i++;
		}

		if (GUILayout.Button("Add new Obstacle")) {
			trackobstacle.obstacles.Add(new Obstacles());
			trackobstacle.showobject.Add(false);
		}
		EditorGUILayout.EndVertical();

		if (GUI.changed){
			EditorUtility.SetDirty(target);
			EditorUtility.SetDirty(trackobstacle);
		}
		
		trackobstacleobject.ApplyModifiedProperties();
	}
}
