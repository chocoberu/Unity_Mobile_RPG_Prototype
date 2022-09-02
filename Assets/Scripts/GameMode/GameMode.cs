using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviourPunCallbacks
{
    public enum EMatchState
    {
        PreMatch = 0,
        InProgress,
        PostMatch
    }
    private EMatchState matchState;
    public EMatchState MatchState
    {
        get
        {
            return matchState;
        }
        set
        {
            matchState = value;
            ChangeMatchState(matchState);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MatchState = EMatchState.PreMatch;
        InitializeMatch();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void ChangeMatchState(EMatchState nextMatchState)
    {
        switch(nextMatchState)
        {
            case EMatchState.PreMatch:
                InitializeMatch();
                break;
            case EMatchState.InProgress:
                StartMatch();
                break;

            case EMatchState.PostMatch:
                EndMatch();
                break;
        }
    }

    public virtual void InitializeMatch()
    {

    }

    public virtual void StartMatch()
    {

    }

    public virtual void EndMatch()
    {

    }
}
