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

    public static GameController Instance { get; private set; }

	[SerializeField] private ExplodeAtPoint Exploder;
	[SerializeField] private TimeSlow TimeSlow;
	[SerializeField] private StarRating StarRating;

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
	}

    // Update is called once per frame
    void Update() {
        if ( Input.GetKeyDown( KeyCode.Space ) ) {
            if ( !bStarted ) {
                bStarted = true;
                OnStart();
            } else if( bReadyForReplay ) {
                OnStartPlayback();
            }
        }
    }

    public void OnStart()
    {
        // Triggered when the player starts the level.
        // Trigger the playback manager to start recording.
        Debug.Log( "GameController::OnStart() Called." );
		snaps.Clear ();
		scoreTally.CreateSubScore ();
        Exploder.ExplodeAtThisPoint ();
		TimeSlow.StartSlowDown ();
        playbackManager.StartRecording();
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
        bReadyForReplay = true;
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
