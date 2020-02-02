using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Explodable))]
public class ExplodableEditor : Editor
{

	public override void OnInspectorGUI()
	{
		Explodable myTarget = (Explodable)target;
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.allowRuntimeFragmentation ) ) , new GUIContent ( "Allow Runtime Fragmentation" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.shatterType ) ) , new GUIContent ( "Shatter Type" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.extraPoints ) ) , new GUIContent ( "Extra Points" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.fragmentGravity ) ) , new GUIContent ( "Gravity" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.subshatterSteps ) ) , new GUIContent ( "Subshatter Steps" ) );
		if (myTarget.subshatterSteps > 1)
		{
			EditorGUILayout.HelpBox("Use subshatter steps with caution! Too many will break performance!!! Don't recommend more than 1", MessageType.Warning);
		}

		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.fragmentLayer ) ) , new GUIContent ( "Fragment Layer" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.sortingLayerName ) ) , new GUIContent ( "Sorting Layer" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.orderInLayer ) ) , new GUIContent ( "Order In Layer" ) );
		EditorGUILayout.PropertyField ( serializedObject.FindProperty ( nameof ( myTarget.LockedInAchievedPreFab) ) , new GUIContent ( "Locked In Prefab" ) );

		serializedObject.ApplyModifiedProperties ();

		if (myTarget.GetComponent<PolygonCollider2D>() == null && myTarget.GetComponent<BoxCollider2D>() == null)
		{
			EditorGUILayout.HelpBox("You must add a BoxCollider2D or PolygonCollider2D to explode this sprite", MessageType.Warning);
		}
		else
		{
			using ( new EditorGUILayout.VerticalScope ( GUI.skin.box ) )
			{
				EditorGUILayout.LabelField ( "Controls" , EditorStyles.boldLabel );
				EditorGUILayout.LabelField ( "Num Fragments" , myTarget.fragments.Count.ToString () );

				EditorGUILayout.Space ();
				if ( GUILayout.Button ( "Generate Fragments" ) )
				{
					myTarget.fragmentInEditor ();
					EditorUtility.SetDirty ( myTarget );
				}
				if ( GUILayout.Button ( "Generate Fragments (Mesh Saved)" ) )
				{
					myTarget.fragmentInEditor_meshSave ();
					EditorUtility.SetDirty ( myTarget );
				}
				using ( new EditorGUI.DisabledScope ( myTarget.fragments.Count == 0 ) )
					if ( GUILayout.Button ( "Destroy Fragments" ) )
					{
						myTarget.deleteFragments ();
						EditorUtility.SetDirty ( myTarget );
					}
				using ( new EditorGUI.DisabledScope ( myTarget.fragments.Count == 0 ) ) if ( GUILayout.Button ( "Reset Fragments" ) )
					{
						myTarget.resetFragments ();
					}
			}

		}

	}
}
