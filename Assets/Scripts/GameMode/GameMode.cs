using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Player
    protected GameObject playerObject;
    protected int TeamNumber = 0;
    protected CinemachineVirtualCamera blueFollowCamera;
    protected CinemachineVirtualCamera redFollowCamera;

    // GameTime
    [SerializeField]
    protected float respawnTime = 3.0f;
    protected float MaxTime = 180.0f;
    protected float startTime;

    [PunRPC]
    protected void ChangeMatchState(int nextMatchState)
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            matchState = (EMatchState)nextMatchState;
        }

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

        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<PlayerState> players = GameObject.FindObjectsOfType<PlayerState>().ToList();
        players.Sort((lhs, rhs) => lhs.photonView.Controller.ActorNumber < rhs.photonView.Controller.ActorNumber ? -1 : 1);
        foreach (var playerState in players)
        {
            Debug.Log($"Player : {playerState.photonView.Controller.NickName} enter");
            //PlayerState playerState = player.GetComponent<PlayerState>();
            if(null == playerState || false == playerState.enabled)
            {
                continue;
            }

            AddPlayerState(playerState);

            // Player Restart 처리는 서버에서 처리
            if (true == PhotonNetwork.IsMasterClient)
            {
                playerState.OnDeath -= RestartPlayer;
                playerState.OnDeath += RestartPlayer;
            }
        }
    }

    public void AddPlayerState(PlayerState playerState)
    {
        if(false == playerList.Contains(playerState))
        {
            playerList.Add(playerState);
        }
    }

    public void RemovePlayerState(PlayerState playerState)
    {
        if(true == playerList.Contains(playerState))
        {
            playerList.Remove(playerState);
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

    protected virtual void RestartPlayer(GameObject player)
    {
        StartCoroutine(CoRestartPlayer(player));
    }

    protected virtual IEnumerator CoRestartPlayer(GameObject player)
    {
        yield return new WaitForSeconds(respawnTime);

        if (null != player)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            PlayerState playerState = player.GetComponent<PlayerState>();

            movement.photonView.RPC("RestartPlayer", RpcTarget.AllViaServer, playerState.StartPosition, playerState.StartRotation);
        }
    }

    [PunRPC]
    protected void SetStartTime(float time)
    {
        startTime = time;
    }

    protected void SetFollowCamera()
    {
        switch (TeamNumber)
        {
            case 0:
                blueFollowCamera.Follow = playerObject.transform;
                blueFollowCamera.LookAt = playerObject.transform;
                break;
            case 1:
                redFollowCamera.Follow = playerObject.transform;
                redFollowCamera.LookAt = playerObject.transform;
                break;
        }
    }

}
