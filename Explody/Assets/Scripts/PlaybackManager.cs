using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    private struct PieceData
    {
        public Vector3 pos;
        public Quaternion rot;
        public bool bActive;
    }

    enum RecordingState
    {
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
    private Dictionary<GameObject, List<PieceData>> recordings;

    private ulong framesPerSample;
    private int maxRecordings = 500;
    private int totalRecordings = 0;
    private int currentReplayStep = 1;
    private float lerpVal = 0f;

    // Start is called before the first frame update
    void Start()
    {
        framesPerSample = 60 / samplesPerSec;
    }

    public void ResetPlayback( GameObject[] newPieces )
    {
        totalRecordings = 0;
        currentReplayStep = 1;
        frame = 0;
        lerpVal = 0f;
        state = RecordingState.WAIT;
        recordings = new Dictionary<GameObject, List<PieceData>>();
        pieces = newPieces;
        foreach ( GameObject piece in pieces ) {
            recordings.Add( piece, new List<PieceData>( maxRecordings ) );
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch ( state ) {
            case RecordingState.RECORD: {
                    if ( frame % framesPerSample == 0 ) {
                        foreach ( GameObject piece in pieces ) {
                            recordings[ piece ].Add( new PieceData {
                                pos = piece.transform.position,
                                rot = piece.transform.rotation,
                                bActive = piece.activeSelf
                            } );
                        }
                        totalRecordings++;
                    }
                }
                break;
            case RecordingState.REPLAY: {
                    if ( currentReplayStep >= totalRecordings ) {
                        Debug.Log( "Replay is done." );
                        state = RecordingState.WAIT;

                        foreach ( KeyValuePair<GameObject, List<PieceData>> entry in recordings ) {
                            GameObject piece = entry.Key;
                            SnapToLocation loc = piece.GetComponent<SnapToLocation>();
                            if( !loc.GetPositionIsFixed() ) {
                                piece.SetActive( false );
                            }
                        }

                        GameController.Instance.OnPlaybackDone();
                        break;
                    }

                    foreach ( KeyValuePair<GameObject, List<PieceData>> entry in recordings ) {
                        GameObject piece = entry.Key;
                        PieceData curEntry = entry.Value[ currentReplayStep ];
                        PieceData prevEntry = entry.Value[ currentReplayStep - 1 ];

                        if ( !curEntry.bActive ) {
                            piece.SetActive( false );
                            continue;
                        }

                        piece.transform.position = Vector3.Lerp( prevEntry.pos, curEntry.pos, lerpVal );
                        piece.transform.rotation = Quaternion.Lerp( prevEntry.rot, curEntry.rot, lerpVal );
                    }
                    float nextLerpVal = lerpVal + Time.deltaTime * playbackSpeed;
                    if ( lerpVal < 1 ) {
                        lerpVal = Mathf.Min( nextLerpVal, 1.0f );
                    } else {
                        lerpVal = 0f;
                        currentReplayStep++;
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
        frame = 0;
    }

    public void Wait()
    {
        if ( state == RecordingState.RECORD ) {
            // Need to add one final recording point for the end of the level.
            foreach ( GameObject piece in pieces ) {
                recordings[ piece ].Add( new PieceData {
                    pos = piece.transform.position,
                    rot = piece.transform.rotation,
                    bActive = piece.activeSelf
                } );

                if( !piece.activeSelf ) {
                    Debug.Log( "GOTCHA!" );
                }
            }
            totalRecordings++;
        }
        state = RecordingState.WAIT;
    }

    public void StartPlayback()
    {
        foreach ( KeyValuePair<GameObject, List<PieceData>> entry in recordings ) {
            GameObject piece = entry.Key;
            PieceData curEntry = entry.Value[ 0 ];

            piece.GetComponent<SnapToLocation>().lookForSnaps = false;

            piece.transform.position = curEntry.pos;
            piece.transform.rotation = curEntry.rot;
            if( !piece.activeSelf ) {
                piece.SetActive( true );
                //PieceData lastEntry = entry.Value[ entry.Value.Count - 1 ];
                //lastEntry.bActive = false;
            }
        }
        state = RecordingState.REPLAY;
        frame = 0;
    }
}
