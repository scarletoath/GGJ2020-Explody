using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    PlaybackManager playbackManager;
    bool bStarted = false;
    bool bReadyForReplay = false;
    float timeLeft = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        if ( Instance == null ) {
            Instance = this;
        } else {
            throw new System.Exception( "Cannot make multiple Game Controller Objects." );
        }

        playbackManager = this.GetComponent<PlaybackManager>();
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
        Eruption boom = GameObject.Find( "EruptionCenter" ).GetComponent<Eruption>();
        boom.StartEruption();
        playbackManager.StartRecording();
    }

    public void OnComplete()
    {
        // Triggered when the player completes the level or runs out of time
        // Trigger the playback manager to stop recording.
        Debug.Log( "GameController::OnComplete() Called." );
        playbackManager.Wait();
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

    //IEnumerator SimpleTimer() {
    //    for ( int i = 20; i > 0; --i ) {
    //        Debug.Log( "Tminus " + i + " seconds..." );
    //        yield return new WaitForSeconds( 1f );
    //    }
    //    Debug.Log( "TIME'S UP!" );
    //    OnComplete();
    //}
}
