using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StarRating : MonoBehaviour
{

	[Serializable]
	private class LimitEntry
	{

		public float FillAmount = 0;
		public Vector2 Range;

	}

	[Serializable]
	private class Star
	{

		[SerializeField] private Image FillImage;
		[SerializeField] private LimitEntry [] LimitEntries;

		public float CalculateFill ( float Value )
		{
			float FillAmount = 0;
			foreach ( var LimitEntry in LimitEntries )
			{
				if ( Value >= LimitEntry.Range.x )
				{
					FillAmount = LimitEntry.FillAmount;
				}
			}
			return FillAmount;
		}

		public void Fill ( float Value )
		{
			FillImage.fillAmount = CalculateFill ( Value );
		}

		public void FillRaw ( float FillAmount )
		{
			FillImage.fillAmount = FillAmount;
		}

	}

	[SerializeField] private float FillSpeed = 1; // stars to fill per second
	[SerializeField] private Star [] Stars;

	private float CurrentRating;
	private Coroutine AnimCoroutine;

	private void Update ()
	{
		if ( Input.GetKey ( KeyCode.UpArrow ) )
		{
			CurrentRating = Mathf.Clamp01 ( CurrentRating + Time.deltaTime / 4 );
			SetRating ( CurrentRating , false );
		}
		else if ( Input.GetKey ( KeyCode.DownArrow ) )
		{
			CurrentRating = Mathf.Clamp01 ( CurrentRating - Time.deltaTime / 4 );
			SetRating ( CurrentRating , false );
		}
	}

	public void SetRating ( float Percentage , bool IsAnimated )
	{
		if ( !IsAnimated )
		{
			foreach ( var Star in Stars )
			{
				Star.Fill ( Percentage );
			}
		}
		else
		{
			if ( AnimCoroutine != null )
			{
				StopCoroutine ( AnimCoroutine );
			}
			AnimCoroutine = StartCoroutine ( SetRatingAnim ( Percentage ) );
		}
	}

	private IEnumerator SetRatingAnim ( float Percentage )
	{
		// Clear
		foreach ( var Star in Stars )
		{
			Star.FillRaw ( 0 );
		}

		// Fill each star one at a time
		foreach ( var Star in Stars )
		{
			float CurrentFill = 0;
			float EndFill = Star.CalculateFill ( Percentage );
			while ( CurrentFill <= EndFill )
			{
				Star.FillRaw ( CurrentFill );
				yield return null;
				CurrentFill += Time.deltaTime * FillSpeed;
			}
			Star.FillRaw ( EndFill );
		}
		AnimCoroutine = null;
	}

#if UNITY_EDITOR
	[CustomEditor ( typeof ( StarRating ) )]
	private class Inspector : Editor
	{

		private float Rating = 0.5f;

		public override bool RequiresConstantRepaint () => Application.isPlaying;

		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			EditorGUILayout.Space ();

			var StarRating = target as StarRating;
			using ( new EditorGUI.DisabledScope ( !Application.isPlaying || StarRating.AnimCoroutine != null ) )
			{
				using ( new EditorGUILayout.HorizontalScope () )
				{
					if ( GUILayout.Button ( "Preview Rating" ) )
					{
						StarRating.SetRating ( Rating , false );
					}
					if ( GUILayout.Button ( "Preview Rating (Animated)" ) )
					{
						StarRating.SetRating ( Rating , true );
					}
				}
				Rating = EditorGUILayout.FloatField ( "Rating" , Rating );
			}
		}

	}
#endif

}