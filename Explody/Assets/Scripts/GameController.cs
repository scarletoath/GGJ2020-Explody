using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{

	public class ScoreTally
	{

		private readonly List <float> Scores = new List <float> ();

		public float Total => Scores.Sum ();

		public float LastScore => Scores.Count > 0 ? Scores [ Scores.Count - 1 ] : -1;
		public float this [ int Index ] => Index >= 0 && Index < Scores.Count ? Scores [ Index ] : -1;

		public void Reset ()
		{
			Scores.Clear ();
		}

		public void CreateSubScore ()
		{
			Scores.Add ( 0 );
			Debug.Log ( $"created sub score entry {Scores.Count}" );
		}

		public void AddSubScore ( float Score )
		{
			Debug.Assert ( Scores.Count > 0 );
			Scores [ Scores.Count - 1 ] += Score;
			Debug.Log ( $"add sub score {Score}, now {LastScore}" );
		}

	}

	[System.Serializable]
	private class LevelManager : ISerializationCallbackReceiver
	{

		private enum SelectionMode
		{

			None ,

			Specified ,
			RandomFromSelection ,
			RandomFromAll ,

		}

		[System.Serializable]
		private class Level
		{

			public SelectionMode SelectionMode = SelectionMode.RandomFromAll;
			public int ObjectIndex = -1;
			public int [] RandomObjectIndices;

		}

		[SerializeField] private List <GameObject> LevelObjects = new List <GameObject> ();

		[Space]

		[SerializeField] private Level PostFinalLevel = new Level { SelectionMode = SelectionMode.RandomFromAll };
		[SerializeField] private List <Level> Levels = new List <Level> { new Level { SelectionMode = SelectionMode.RandomFromAll } };

		private int [] RandomAllIndices = System.Array.Empty <int> ();

		private GameObject CurrentLevelObject;
		private int LastObjectIndex = -1;

		public int CurrentLevelIndex { get; private set; } = -1;
		public bool IsLevelActive => CurrentLevelObject != null;

		public void ClearLevel ()
		{
			if ( CurrentLevelObject != null )
			{
				DestroyImmediate ( CurrentLevelObject );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="LevelIndex"></param>
		/// <returns>Indicates whether next level spawn succeeded.</returns>
		public bool SpawnNextLevel () => SpawnLevel ( CurrentLevelIndex + 1 );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="LevelIndex"></param>
		/// <returns>Indicates whether level spawn succeeded.</returns>
		public bool SpawnLevel ( int LevelIndex , bool AutoClear = false )
		{
			if ( !AutoClear && CurrentLevelObject != null )
			{
				Debug.LogError ( $"Level {LevelIndex} requested while current level {CurrentLevelIndex} is still active." , CurrentLevelObject );
				return false;
			}
			ClearLevel ();

			GameObject LevelObjectSource = null;
			if ( LevelIndex >= 0 && LevelIndex < Levels.Count )
			{
				CurrentLevelIndex = LevelIndex;
				LevelObjectSource = GetLevelObject ( Levels [ LevelIndex ] );
			}
			else if ( LevelIndex == Levels.Count ) // post final level mode
			{
				LevelObjectSource = GetLevelObject ( PostFinalLevel );
			}
			else // Invalid level index
			{
				Debug.LogError ( $"{LevelIndex} is not a valid level." );
				return false;
			}

			if ( LevelObjectSource == null ) // Valid level index but undefined level object
			{
				Debug.LogError ( $"Found level {LevelIndex} but no object was defined for it or invalid SelectionMode." );
				return false;
			}

			CurrentLevelObject = Instantiate ( LevelObjectSource );
			return true;
		}

		private GameObject GetLevelObject ( Level Level )
		{
			Debug.Assert ( Level != null , "Cannot get object from null Level!" );
			switch ( Level.SelectionMode )
			{
				case SelectionMode.Specified :
					return LevelObjects [ LastObjectIndex = Level.ObjectIndex ];
				case SelectionMode.RandomFromSelection :
					return Level.RandomObjectIndices.Length > 0 ? LevelObjects [ LastObjectIndex = GetRandomFromArray ( Level.RandomObjectIndices ) ] : null;
				case SelectionMode.RandomFromAll :
					return RandomAllIndices.Length > 0 ? LevelObjects [ LastObjectIndex = GetRandomFromArray ( RandomAllIndices ) ] : null;
			}
			return null;

			int GetRandomFromArray ( int [] Values )
			{
				const int MaxAttempts = 10;
				int Value = -1;
				int Attempts = 0;
				while ( Attempts++ < MaxAttempts && ( Value == -1 || Value == LastObjectIndex ) )
					Value = Values [ Random.Range ( 0 , Values.Length ) ];
				return Value == -1 ? 0 : Value; // guaranteed safe to return 0 as array has been pre-checked for at least one element
			}
		}

		#region ISerializationCallbackReceiver Implementation
		public void OnBeforeSerialize () {}
		public void OnAfterDeserialize () => RandomAllIndices = Enumerable.Range ( 0 , Levels.Count ).ToArray ();
		#endregion

	}

    public static GameController Instance { get; private set; }

	[SerializeField] private ExplodeAtPoint Exploder;
	[SerializeField] private TimeSlow TimeSlow;
	[SerializeField] private StarRating StarRating;
	[SerializeField] private ScoreDisplay ScoreDisplay;

	[Space] 
	
	[SerializeField] private LevelManager levelManager;

    PlaybackManager playbackManager;
    bool bStarted = false;
    bool bReadyForReplay = false;
    float timeLeft = 20.0f;

	private readonly HashSet <SnapToLocation> snaps = new HashSet <SnapToLocation> ();
	public ScoreTally scoreTally { get; } = new ScoreTally ();

    // Start is called before the first frame update
    void Start()
    {
        if ( Instance == null ) {
            Instance = this;
        } else {
            throw new System.Exception( "Cannot make multiple Game Controller Objects." );
        }

        playbackManager = this.GetComponent<PlaybackManager>();
		StarRating.gameObject.SetActive ( false );
		ScoreDisplay.Hide ();
	}

    // Update is called once per frame
    void Update() {
        if ( Input.GetKeyDown( KeyCode.Space ) ) {
            if ( !bStarted && !bReadyForReplay ) { // TODO : remove readyForReplay check once states are in
                OnStartNextLevel();
            } else if( bReadyForReplay ) {
                //OnStartPlayback(); // TODO : Uncomment when ready to hook up
				if ( !ScoreDisplay.IsRamping )
					OnStartNextLevel ();
            }
        }
    }

    public void OnStartNextLevel()
    {
        // Triggered when the player starts the level.
        // Trigger the playback manager to start recording.
        Debug.Log( "GameController::OnStartNextLevel() Called." );
		StarRating.gameObject.SetActive ( false );
		ScoreDisplay.Hide ();
		snaps.Clear ();
		scoreTally.CreateSubScore ();

		levelManager.ClearLevel ();
		levelManager.SpawnNextLevel ();
        Exploder.ExplodeAtThisPoint ();
		TimeSlow.StartSlowDown ();

        GameObject[] pieces = GameObject.FindGameObjectsWithTag( "Piece" );

        playbackManager.ResetPlayback( pieces );
        playbackManager.StartRecording();

		bStarted = true;
	}

    public void OnComplete()
    {
        // Triggered when the player completes the level or runs out of time
        // Trigger the playback manager to stop recording.
        Debug.Log( "GameController::OnComplete() Called." );
		TimeSlow.StopSlowDown ();
		playbackManager.Wait();
        StarRating.gameObject.SetActive ( true );
		StarRating.SetRating ( Random.Range ( 0.29f , 1 ) , true ); // TODO : Get score in percent
		ScoreDisplay.ShowScore ( scoreTally.LastScore );
        bReadyForReplay = true;
        StartCoroutine( TriggerPlayback() );
        bStarted = false;
	}

    System.Collections.IEnumerator TriggerPlayback()
    {
        if ( bReadyForReplay ) {
            yield return new WaitForSeconds( .5f );
            playbackManager.StartPlayback();
        }
    }

    public void OnStartPlayback()
    {
        // Triggered shortly after OnComplete (x seconds? or just when the player triggeres it manually?)
        // Trigger the playback manager to play back the recording.
        Debug.Log( "GameController::OnStartPlayback() Called." );
        playbackManager.StartPlayback();
    }

	public void ShowMenu()
    {
        // Triggered by the player
        // Pauses the game
        Debug.Log( "GameController::ShowMenu() Called." );
    }

	public void RegisterSnap ( SnapToLocation Snap )
	{
		if ( !snaps.Contains ( Snap ) )
		{
			snaps.Add ( Snap );
		}
	}

	public void UnregisterSnap ( SnapToLocation Snap )
	{
		if ( snaps.Contains ( Snap ) )
		{
			snaps.Remove ( Snap );
			scoreTally.AddSubScore ( Snap.GetScore () );

			if ( snaps.Count == 0 ) // all snaps destroyed or fit
			{
				OnComplete ();
			}
		}
	}

    //IEnumerator SimpleTimer() {
    //    for ( int i = 20; i > 0; --i ) {
    //        Debug.Log( "Tminus " + i + " seconds..." );
    //        yield return new WaitForSeconds( 1f );
    //    }
    //    Debug.Log( "TIME'S UP!" );
    //    OnComplete();
    //}
}
