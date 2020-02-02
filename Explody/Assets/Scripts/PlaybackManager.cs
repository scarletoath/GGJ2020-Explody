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

    private string pieceTag = "Piece";
    [SerializeField] public ulong samplesPerSec = 6;
    [SerializeField] public ulong playbackSpeed = 1;

    private ulong frame;

    private GameObject[] pieces;
    private Dictionary<GameObject, List<PieceData> > recordings = new Dictionary<GameObject, List<PieceData>>();

    private int maxRecordings = 150;
    private int totalRecordings = 0;
    private int currentReplayStep = 1;
    private float lerpVal = 0;

    // Start is called before the first frame update
    void Start()
    {
        frame = 0;
        if ( pieces == null )
        {
            pieces = GameObject.FindGameObjectsWithTag( pieceTag );
        }

        samplesPerSec = 60 / samplesPerSec;

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
                    if ( frame % samplesPerSec == 0 ) {
                        foreach ( GameObject piece in pieces ) {
                            recordings[piece].Add( new PieceData {
                                pos = piece.transform.position,
                                rot = piece.transform.rotation
                            } );
                        }
                        totalRecordings++;
                    }
                }
                break;
            case RecordingState.REPLAY:
                {
                    if ( lerpVal > 1 ) {
                        lerpVal = 0f;
                        currentReplayStep++;
                    }
                    if ( currentReplayStep >= totalRecordings ) {
                        Debug.Log( "Replay is done." );
                        state = RecordingState.WAIT;
                        break;
                    }

                    foreach ( KeyValuePair< GameObject, List< PieceData > > entry in recordings ) {
                        GameObject piece = entry.Key;
                        PieceData curEntry = entry.Value[ currentReplayStep ];
                        PieceData prevEntry = entry.Value[ currentReplayStep - 1 ];

                        piece.transform.position = Vector3.Lerp( prevEntry.pos, curEntry.pos, lerpVal );
                        piece.transform.rotation = Quaternion.Lerp( prevEntry.rot, curEntry.rot, lerpVal );
                    }

                    lerpVal += Time.deltaTime * playbackSpeed;
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
        frame = 0;
    }

    public void Wait()
    {
        state = RecordingState.WAIT;
    }

    public void StartPlayback()
    {
        state = RecordingState.REPLAY;
        frame = 0;
    }
}
