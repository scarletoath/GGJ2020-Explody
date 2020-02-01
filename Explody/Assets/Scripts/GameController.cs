using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    PlaybackManager playbackManager;

    // Start is called before the first frame update
    void Start()
    {
        playbackManager = new PlaybackManager();
        playbackManager.Init();
    }

    // Update is called once per frame
    void Update()
    {
        playbackManager.Update();
    }

    void OnStart()
    {
        // Triggered when the player starts the level.
        // Trigger the playback manager to start recording.
        throw new System.NotImplementedException();
    }

    void OnComplete()
    {
        // Triggered when the player completes the level or runs out of time
        // Trigger the playback manager to stop recording.
        throw new System.NotImplementedException();
    }

    void OnStartPlayback()
    {
        // Triggered shortly after OnComplete (x seconds? or just when the player triggeres it manually?)
        // Trigger the playback manager to play back the recording.
        throw new System.NotImplementedException();
    }

    void ShowMenu()
    {
        // Triggered by the player
        // Pauses the game
        throw new System.NotImplementedException();
    }
}
