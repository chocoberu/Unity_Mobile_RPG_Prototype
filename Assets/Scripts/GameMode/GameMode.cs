using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
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
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MatchState = EMatchState.PreMatch; 
    }

    // Update is called once per frame
    void Update()
    {
        
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
