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
    
    [SerializeField]
    private EMatchState matchState;
    public EMatchState MatchState
    {
        get
        {
            return matchState;
        }
        protected set
        {
            matchState = value;
            Debug.Log($"Current Match State : {matchState.ToString()}");
            ChangeMatchState((int)matchState);
        }
    }

    [SerializeField]
    protected List<PlayerState> playerList = new List<PlayerState>();

    [PunRPC]
    protected void ChangeMatchState(int nextMatchState)
    {
        switch((EMatchState)nextMatchState)
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

    public virtual void UpdatePlayerList()
    {
        playerList.Clear();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            PlayerState playerState = player.GetComponent<PlayerState>();
            if(null == playerState)
            {
                continue;
            }

            AddPlayerState(playerState);
        }
    }

    public void AddPlayerState(PlayerState playerState)
    {
        if(false == playerList.Contains(playerState))
        {
            playerList.Add(playerState);
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
