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

		private int SubCount;

		public void Reset ()
		{
			Scores.Clear ();
			SubCount = 0;
		}

		public void CreateSubScore ()
		{
			Scores.Add ( 0 );
			SubCount = 0;
			//Debug.Log ( $"created sub score entry {Scores.Count}" );
		}

		public void AddSubScore ( float Score )
		{
			Debug.Assert ( Scores.Count > 0 );
			float SubTotal = Scores [ Scores.Count - 1 ] * SubCount + Score;
			Scores [ Scores.Count - 1 ] = SubTotal / ++SubCount;
			//Debug.Log ( $"add sub score {Score}, now {LastScore}" );
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
	[SerializeField] ParticleSystem InitialExplosionFx;

	[Space] 
	
	[SerializeField] private LevelManager levelManager;

    PlaybackManager playbackManager;
    bool bStarted = false;
    bool bReadyForDisplay = true;
    bool bReplaying = false;
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
		OnDisplayNextLevel(true);
		AkSoundEngine.PostEvent("playLevelStart", gameObject);
	}

    // Update is called once per frame
    void Update() {

        if ((bReadyForDisplay || !bStarted) && !bReplaying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("User has clicked and something should probably happen ((prep=" + bReadyForDisplay + "  play=" + bStarted + " replay=" + bReplaying + ")");
                if (bReadyForDisplay)
                {
                    OnDisplayNextLevel(false);
                } else if (!bStarted)
                {
                    OnStartNextLevel();
                }
            }
        }
        if ( Input.GetKeyDown( KeyCode.Escape ) ) {
            ExitGame();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

	public void OnDisplayNextLevel(bool firstTime)
	{
		Debug.Log("GameController::OnDisplayNextLevel() Called.");
		StarRating.gameObject.SetActive(false);
		ScoreDisplay.Hide();
		snaps.Clear();
		scoreTally.CreateSubScore();
		if(!firstTime)
		{
			AkSoundEngine.PostEvent("playLevelEnd", gameObject);// Wwise Audio Event @ekampa Level begin (after replay, before explosion)
		}
		levelManager.ClearLevel();
		levelManager.SpawnNextLevel();
        bReadyForDisplay = false;
	}

	public void OnStartNextLevel()
    {
        // Triggered when the player starts the level.
        // Trigger the playback manager to start recording.
        Debug.Log( "GameController::OnStartNextLevel() Called." );
		AkSoundEngine.PostEvent("playExplosion", gameObject);
		bStarted = true;
		StartCoroutine(TriggerExplosionAfterSwell());
	}

    public void OnComplete()
    {
        // Triggered when the player completes the level or runs out of time
        // Trigger the playback manager to stop recording.
        Debug.Log( "GameController::OnComplete() Called." );
		TimeSlow.StopSlowDown ();
		playbackManager.Wait();
        StarRating.gameObject.SetActive ( true );
		StarRating.SetRating ( scoreTally.LastScore , true );
		ScoreDisplay.ShowScore ( scoreTally.LastScore * scoreTally.LastScore * 2500000 );
        StartCoroutine( TriggerPlayback() );
        bReplaying = true;
        bStarted = false;
	}

    System.Collections.IEnumerator TriggerPlayback()
    {
        yield return new WaitForSeconds( .5f );
        AkSoundEngine.PostEvent("playReplay", gameObject);// Wwise Audio Event @ekampa Play replay
        playbackManager.StartPlayback();
    }

	System.Collections.IEnumerator TriggerExplosionAfterSwell()
	{
		yield return new WaitForSecondsRealtime(0.25f);
		Instantiate ( InitialExplosionFx ).Play ();
		Exploder.ExplodeAtThisPoint();
		TimeSlow.StartSlowDown();
		GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
		playbackManager.ResetPlayback(pieces);
		playbackManager.StartRecording();
	}


	public void OnStartPlayback()
    {
        // Triggered shortly after OnComplete (x seconds? or just when the player triggeres it manually?)
        // Trigger the playback manager to play back the recording.
        Debug.Log( "GameController::OnStartPlayback() Called." );
        playbackManager.StartPlayback();
    }

    public void OnPlaybackDone()
    {
        Debug.Log("GameController::OnPlaybackDone() Called");
        AkSoundEngine.PostEvent("playReplayEnd", gameObject);// Wwise Audio Event @ekampa End of Replay
        bReadyForDisplay = true;
        bReplaying = false;
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
