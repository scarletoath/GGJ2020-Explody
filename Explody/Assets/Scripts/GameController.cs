using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    PlaybackManager playbackManager;

    // Start is called before the first frame update
    void Start()
    {
        if ( Instance == null ) {
            Instance = this;
        } else {
            throw new System.Exception( "Cannot make multiple Game Controller Objects." );
        }

        playbackManager = gameObject.AddComponent<PlaybackManager>() as PlaybackManager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStart()
    {
        // Triggered when the player starts the level.
        // Trigger the playback manager to start recording.
        Debug.Log( "GameController::OnStart() Called." );
    }

    public void OnComplete()
    {
        // Triggered when the player completes the level or runs out of time
        // Trigger the playback manager to stop recording.
        Debug.Log( "GameController::OnComplete() Called." );
    }

    public void OnStartPlayback()
    {
        // Triggered shortly after OnComplete (x seconds? or just when the player triggeres it manually?)
        // Trigger the playback manager to play back the recording.
        Debug.Log( "GameController::OnStartPlayback() Called." );
    }

    public void ShowMenu()
    {
        // Triggered by the player
        // Pauses the game
        Debug.Log( "GameController::ShowMenu() Called." );
    }
}
