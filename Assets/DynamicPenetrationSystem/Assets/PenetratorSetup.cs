#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PenetratorSetup : ScriptableWizard {

	public GameObject penetrator;
	public GameObject penetratorModel;
	public Vector3 penetratorBase;
	public Vector3 penetratorTip;
	public Material processingMaterial;
	private Material penetratorMaterial;
	private float cachedCurvature;
	private float cachedRecurvature;
	private float length;
	bool prepared = false;

	[MenuItem("Tools/Raliv/Penetrator Setup")]
	static void Setup() {
		ScriptableWizard.DisplayWizard<PenetratorSetup>("Penetrator Setup", "Go", "Cancel");
	}
	
	void OnWizardUpdate() {
	}

	void TranslateMesh(Mesh mesh, Vector3 translation) {
		Vector3[] vertices = mesh.vertices;

		for (int i=0;i<vertices.Length;i++) {
			vertices[i] += translation;
		}

		mesh.vertices = vertices;
	}

	void RotateMesh(Mesh mesh, Quaternion rotation) {
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		Vector3 tempTangent;

		for (int i=0;i<vertices.Length;i++) {
			vertices[i] = rotation * vertices[i];
			normals[i] = rotation * normals[i];
			tempTangent = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
			tempTangent = rotation * tempTangent;
			tangents[i] = new Vector4(tempTangent.x, tempTangent.y, tempTangent.z, tangents[i].w);
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
	}

	void ScaleMesh(Mesh mesh, float ScaleFactor) {
		Vector3[] vertices = mesh.vertices;

		for (int i=0;i<vertices.Length;i++) {
			vertices[i] *= ScaleFactor;
		}

		mesh.vertices = vertices;
	}

	void FuckUpModel(GameObject penetratorModel) {
		Mesh sharedMesh = null;
		if (penetratorModel.GetComponent<SkinnedMeshRenderer>()!=null) 
			sharedMesh = penetratorModel.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		if (penetratorModel.GetComponent<MeshFilter>()!=null) 
			sharedMesh = penetratorModel.GetComponent<MeshFilter>().sharedMesh;
		Mesh mesh = (Mesh)Instantiate(sharedMesh);
		RotateMesh(mesh, Quaternion.Euler(Random.Range(0,3)*90f, Random.Range(0,3)*90f, Random.Range(0,3)*90f));
		TranslateMesh(mesh, Random.insideUnitSphere);
		if (penetratorModel.GetComponent<SkinnedMeshRenderer>()!=null) {
			penetratorModel.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
			EditorUtility.SetDirty(penetratorModel.GetComponent<SkinnedMeshRenderer>().sharedMesh);
		}
		if (penetratorModel.GetComponent<MeshFilter>()!=null) {
			penetratorModel.GetComponent<MeshFilter>().sharedMesh = mesh;
			EditorUtility.SetDirty(penetratorModel.GetComponent<MeshFilter>());
		}
	}

	int SetupPenetratorModel(GameObject penetratorModel) {
		Mesh sharedMesh = null;
		if (penetratorModel.GetComponent<SkinnedMeshRenderer>()!=null) {
			sharedMesh = new Mesh();
			penetratorModel.GetComponent<SkinnedMeshRenderer>().BakeMesh(sharedMesh);
		}
		if (penetratorModel.GetComponent<MeshFilter>()!=null) 
			sharedMesh = penetratorModel.GetComponent<MeshFilter>().sharedMesh;
		Mesh mesh = (Mesh)Instantiate(sharedMesh);

		TranslateMesh(mesh, -penetratorBase);

		Vector3 farthestVert = Vector3.zero;

		RotateMesh(mesh, Quaternion.Inverse(Quaternion.LookRotation(penetratorTip - penetratorBase, Vector3.up)));
		ScaleMesh(mesh, penetratorModel.transform.localScale.x);
		penetratorModel.transform.localScale=Vector3.one;

		for (int i=0;i<mesh.vertices.Length;i++) {
			float dist = mesh.vertices[i].magnitude;
			if (dist>farthestVert.magnitude) {
				farthestVert=mesh.vertices[i];
			}
		}

		length = farthestVert.magnitude;

		/*if (Mathf.Abs(farthestVert.z)>Mathf.Abs(farthestVert.x) && Mathf.Abs(farthestVert.z)>Mathf.Abs(farthestVert.y)) {
			if (farthestVert.z<0f) RotateMesh(mesh, Quaternion.Euler(0f, 180f, 0f));
		}
		if (Mathf.Abs(farthestVert.x)>Mathf.Abs(farthestVert.y) && Mathf.Abs(farthestVert.x)>Mathf.Abs(farthestVert.z)) {
			if (farthestVert.x>0f) RotateMesh(mesh, Quaternion.Euler(0f, -90f, 0f));
				else RotateMesh(mesh, Quaternion.Euler(0f, 90f, 0f));
		}
		if (Mathf.Abs(farthestVert.y)>Mathf.Abs(farthestVert.x) && Mathf.Abs(farthestVert.y)>Mathf.Abs(farthestVert.z)) {
			if (farthestVert.y>0f) RotateMesh(mesh, Quaternion.Euler(90f, 0f, 0f));
				else RotateMesh(mesh, Quaternion.Euler(-90f, 0f, 0f));
		}*/

		Bounds bigBounds=mesh.bounds;
		bigBounds.center=Vector3.zero;
		bigBounds.extents = new Vector3(length*2f, length*2f, length*2f);
		mesh.bounds=bigBounds;

		AssetDatabase.CreateAsset(mesh, "Assets/DynamicPenetrationSystem/MyPenetrators/"+penetratorModel.name+".asset");
		AssetDatabase.SaveAssets();

		if (penetratorModel.GetComponent<SkinnedMeshRenderer>()!=null) {
			penetratorModel.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
			penetratorModel.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen=false;
			bigBounds = penetratorModel.GetComponent<SkinnedMeshRenderer>().localBounds;
			bigBounds.center=Vector3.zero;
			bigBounds.extents = new Vector3(length, length, length);
			penetratorModel.GetComponent<SkinnedMeshRenderer>().localBounds=bigBounds;
			EditorUtility.SetDirty(penetratorModel.GetComponent<SkinnedMeshRenderer>().sharedMesh);
		}
		if (penetratorModel.GetComponent<MeshFilter>()!=null) {
			penetratorModel.GetComponent<MeshFilter>().sharedMesh = mesh;
			EditorUtility.SetDirty(penetratorModel.GetComponent<MeshFilter>());
		}
		return 0;
	}

	void OnEnable() {
			SceneView.onSceneGUIDelegate += OnSceneGUI;
	}
 
	void OnDisable() {
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView) {
		if (prepared) {
			Tools.current = Tool.View;
			EditorGUI.BeginChangeCheck();
			Quaternion handleRotation = Quaternion.identity;
			if (penetratorModel.transform.parent!=null) handleRotation = penetratorModel.transform.parent.rotation;
			Vector3 newPenetratorBase = Handles.PositionHandle(penetratorModel.transform.TransformPoint(penetratorBase), handleRotation);
			Vector3 newPenetratorTip = Handles.PositionHandle(penetratorModel.transform.TransformPoint(penetratorTip), handleRotation);
			if (EditorGUI.EndChangeCheck())	{
				Undo.RecordObject(this, "Changed Penetrator Base");
				penetratorBase = penetratorModel.transform.InverseTransformPoint(newPenetratorBase);
				penetratorTip = penetratorModel.transform.InverseTransformPoint(newPenetratorTip);
			}
			GUIStyle style = new GUIStyle();
			style.normal.textColor=Color.white;
			Handles.color=Color.white;
			Handles.DrawSolidDisc(penetratorModel.transform.TransformPoint(penetratorBase), penetratorModel.transform.right, 0.01f);
			Handles.DrawSolidDisc(penetratorModel.transform.TransformPoint(penetratorBase), penetratorModel.transform.up, 0.01f);
			Handles.DrawSolidDisc(penetratorModel.transform.TransformPoint(penetratorBase), penetratorModel.transform.forward, 0.01f);
			Handles.Label(penetratorModel.transform.TransformPoint(penetratorBase), "BASE");
			Handles.color=Color.blue;
			Handles.DrawSolidDisc(penetratorModel.transform.TransformPoint(penetratorTip), penetratorModel.transform.right, 0.01f);
			Handles.DrawSolidDisc(penetratorModel.transform.TransformPoint(penetratorTip), penetratorModel.transform.up, 0.01f);
			Handles.DrawSolidDisc(penetratorModel.transform.TransformPoint(penetratorTip), penetratorModel.transform.forward, 0.01f);
			Handles.Label(penetratorModel.transform.TransformPoint(penetratorTip), "TIP");
		}
	}

	void OnGUI() {
		penetrator = (GameObject)EditorGUILayout.ObjectField("Penetrator", penetrator, typeof(GameObject), true);
		penetratorModel = (GameObject)EditorGUILayout.ObjectField("My Model", penetratorModel, typeof(GameObject), true);
		EditorGUILayout.HelpBox("EXPERIMENTAL! This might work for you though, give it a shot! (Back up your model just in case)", MessageType.Info);
		if (penetrator==null || penetratorModel==null) {
			GUIStyle textStyle = EditorStyles.label;
 			textStyle.wordWrap = true;
			EditorGUILayout.LabelField("Drag the target Penetrator prefab and your custom model into the slot provided",textStyle);
		} else {
			if (penetratorMaterial==null || !penetratorMaterial.HasProperty("_Length")) {
				if (penetratorModel.GetComponent<SkinnedMeshRenderer>()!=null) 
					penetratorMaterial = penetratorModel.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
				if (penetratorModel.GetComponent<MeshRenderer>()!=null) 
					penetratorMaterial = penetratorModel.GetComponent<MeshRenderer>().sharedMaterial;
				if (penetratorMaterial==null) {
					EditorGUILayout.HelpBox("You must choose a model with a MeshFilter or SkinnedMeshRenderer!", MessageType.Error);
				}
				if (!penetratorMaterial.HasProperty("_Length")) {
					EditorGUILayout.HelpBox("No Dynamic Penetration System material detected! Please add a Penetrator material to your model.", MessageType.Error);
				}
			} else {
				if (!prepared) {
					if (GUILayout.Button("Prepare Model")) {
						if (penetratorModel.GetComponent<SkinnedMeshRenderer>()!=null) {
							if (penetratorModel.GetComponent<SkinnedMeshRenderer>().rootBone!=null) {
								GameObject newPenetratorModel = new GameObject(penetratorModel.name);
								newPenetratorModel.transform.parent=penetrator.transform;
								newPenetratorModel.AddComponent<MeshFilter>();
								newPenetratorModel.GetComponent<MeshFilter>().sharedMesh=new Mesh();
								penetratorModel.GetComponent<SkinnedMeshRenderer>().BakeMesh(newPenetratorModel.GetComponent<MeshFilter>().sharedMesh);
								newPenetratorModel.AddComponent<MeshRenderer>();
								newPenetratorModel.GetComponent<MeshRenderer>().sharedMaterial=processingMaterial;
								newPenetratorModel.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
								newPenetratorModel.transform.position=penetratorModel.transform.position;
								newPenetratorModel.transform.localScale = Vector3.one;
								newPenetratorModel.transform.rotation = penetratorModel.transform.rotation;
								EditorUtility.SetDirty(newPenetratorModel);
								penetratorModel.SetActive(false);
								penetratorModel=newPenetratorModel;
							}
						}
						penetratorBase=Vector3.zero;
						penetratorTip = Quaternion.Inverse(penetratorModel.transform.localRotation) * Vector3.forward * 0.3f;
						cachedCurvature = penetratorMaterial.GetFloat("_Curvature");
						cachedRecurvature = penetratorMaterial.GetFloat("_ReCurvature");
						penetratorMaterial.SetFloat("_Curvature", 0f);
						penetratorMaterial.SetFloat("_ReCurvature", 0f);
						penetratorMaterial.SetFloat("_Length", 100f);
						prepared=true;
					}
				} else {
					EditorGUILayout.HelpBox("Move the white position dot onto the center of the base of the penetrator, everything in front of the dot will deform.", MessageType.Info);
					EditorGUILayout.HelpBox("Move the blue dot to the center of the tip of the penetrator", MessageType.Info);
					if (GUILayout.Button("Go!")) {
						int error = SetupPenetratorModel(penetratorModel);
						if (error > 0) {
							switch (error) {
								case 1:
									EditorGUILayout.HelpBox("Something went wrong!", MessageType.Error);
									break;
							}
						} else {
							penetratorModel.GetComponent<MeshRenderer>().sharedMaterial=penetratorMaterial;
							penetratorMaterial.SetFloat("_Curvature", cachedCurvature);
							penetratorMaterial.SetFloat("_ReCurvature", cachedRecurvature);
							penetratorMaterial.SetFloat("_EntranceStiffness", 0.01f);
							penetratorMaterial.SetFloat("_Length", length);
							penetratorModel.transform.localPosition=Vector3.zero;
							penetratorModel.transform.localRotation=Quaternion.identity;
							EditorGUILayout.Space();
							EditorGUILayout.HelpBox("Done!", MessageType.Info);
							Close();
						}
					}
				}
			}
		}
	}


}
#endif
