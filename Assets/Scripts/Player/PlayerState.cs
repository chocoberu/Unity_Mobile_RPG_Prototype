using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviourPun
{
    private int teamNumber = 0;
    public int TeamNumber { get { return teamNumber; } }
    
    public int KillScore { get; set; }
    public int DeathScore { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void SetTeamNumber(int newTeamNumber)
    {

    }
}
