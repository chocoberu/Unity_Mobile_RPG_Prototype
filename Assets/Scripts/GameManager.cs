using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        switch(GameInstance.Instance.GameType)
        {
            case GameInstance.EGameType.Single:
                {
                    Debug.Log("SinglePlay GameMode");
                    PhotonNetwork.OfflineMode = true;
                }
                break;

            case GameInstance.EGameType.PvE:
                {
                    if(true == PhotonNetwork.IsMasterClient)
                    {
                        Debug.Log("PvE GameMode");
                    }
                }
                break;
            case GameInstance.EGameType.PvP:
                {
                    if (true == PhotonNetwork.IsMasterClient)
                    {
                        Debug.Log("PvP GameMode");
                    }
                }
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
