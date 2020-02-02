using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    private struct PieceData
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    enum RecordingState {
        WAIT,
        RECORD,
        REPLAY
    }
    private RecordingState state = RecordingState.WAIT;

    public string pieceTag = "Piece";
    public ulong recordingSpeed = 10;
    public ulong playbackSpeed = 2;

    private ulong frame;

    private GameObject[] pieces;
    private Dictionary<GameObject, List<PieceData> > recordings = new Dictionary<GameObject, List<PieceData>>();

    private int maxRecordings = 150;
    private int currentReplayStep = 0;

    // Start is called before the first frame update
    void Start()
    {
        frame = 0;
        if ( pieces == null )
        {
            pieces = GameObject.FindGameObjectsWithTag( pieceTag );
        }

        recordingSpeed = 60 / recordingSpeed;
        playbackSpeed = 60 / playbackSpeed;

        foreach( GameObject piece in pieces )
        {
            recordings.Add( piece, new List<PieceData>( maxRecordings ) );
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch( state )
        {
            case RecordingState.RECORD:
                {
                    if ( frame % recordingSpeed == 0 ) {
                        foreach ( GameObject piece in pieces ) {
                            recordings[piece].Add( new PieceData {
                                pos = piece.transform.position,
                                rot = piece.transform.rotation
                            } );
                        }
                    }
                }
                break;
            case RecordingState.REPLAY:
                {
                    if ( frame % playbackSpeed == 0 ) {
                        foreach ( KeyValuePair< GameObject, List< PieceData > > entry in recordings ) {
                            GameObject piece = entry.Key;
                            PieceData curEntry = entry.Value[ currentReplayStep ];

                            piece.transform.position = curEntry.pos;
                            piece.transform.rotation = curEntry.rot;
                        }

                    }
                }
                break;
            case RecordingState.WAIT:
            default:
                break;
        }

        frame++;
    }

    public void StartRecording()
    {
        state = RecordingState.RECORD;
    }

    public void Wait()
    {
        state = RecordingState.WAIT;
    }

    public void StartPlayback()
    {
        state = RecordingState.REPLAY;
    }
}
