using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public ItemRandomSpawn[] spawners;
    public ReturnZone[] returnZones;
    public ParticleSystem[] victoryParticles;

    bool gameStarting = false;
    bool gameInProgress = false;
    bool justEnded = false;
    float currentGameTime = 0.0f;

    public List<NetworkPlayer> playersReadyForGame;
    // Start is called before the first frame update
    void Start()
    {
        playersReadyForGame = new List<NetworkPlayer>();
        for (int i = 0; i < returnZones.Length; i++)
        {
            returnZones[i].zoneID = i;
            returnZones[i].onTriggerEntered += SubTriggerEntered;
            returnZones[i].onTriggerExited += SubTriggerExited;
        }
    }

    private void Update()
    {
        if (gameInProgress)
        {
            currentGameTime += Time.deltaTime;
            WorldTextInfo.singleton.UpdateGameTimer(currentGameTime.ToString("F0"));
        }
    }


    #region PUN
    public override void OnJoinedRoom()
    {
        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "GameState", 0 } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
            WorldTextInfo.singleton.NewMessage("Walk Into Zone To Start");
            for (int j = 0; j < returnZones.Length; j++)
            {
                returnZones[j].currentText.text = "Walk Into Zone To Start";
            }
        }
        else
        {
            bool inProgress = false;
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState"))
            {
                int gameState = (int)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];
                if (gameState != 0)
                {
                    inProgress = true;
                    WorldTextInfo.singleton.NewMessage("Game In Progress");
                    for (int j = 0; j < returnZones.Length; j++)
                    {
                        returnZones[j].currentText.text = "Game In Progress";
                    }
                }
            }

            if (!inProgress)
            {
                WorldTextInfo.singleton.NewMessage("Walk Into Zone To Start");
                for (int j = 0; j < returnZones.Length; j++)
                {
                    returnZones[j].currentText.text = "Walk Into Zone To Start";
                }
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey("GameState"))
        {
            int state = (int)propertiesThatChanged["GameState"];
            print("GameState: " + propertiesThatChanged["GameState"]);
            gameInProgress = state != 0;
        }
    }
    #endregion

    [ContextMenu("InitializeGame")]
    public void InitializeGame()
    {
        if (photonView.IsMine)
        {
            if (gameStarting || gameInProgress) return;
            gameStarting = true;
            photonView.RPC(nameof(InitializeGame_RPC), RpcTarget.All);

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "GameState", 1 } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }
    }

    [PunRPC]
    void InitializeGame_RPC()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        GameManager.singleton.PlayTickSound();
        NetworkPlayer_THF.LocalPlayer.ableToSpawn = false;
        GameManager.singleton.ResetAllItems();
        for (int i = 3; i > 0; i--)
        {
            for (int j = 0; j < returnZones.Length; j++)
            {
                returnZones[j].currentText.text = "Game Starting: " + i;
            }
            WorldTextInfo.singleton.NewMessage("Game Starting: " + i);
            yield return new WaitForSeconds(1.0f);
            GameManager.singleton.PlayTickSound();
        }

        returnZones[0].itemsRemaining = spawners[0].maxSpawns;
        returnZones[1].itemsRemaining = spawners[1].maxSpawns;
        returnZones[0].ResetZone();
        returnZones[1].ResetZone();

        for (int i = 0; i < returnZones.Length; i++)
        {
            returnZones[i].currentText.text = "Items Remaining: "+ returnZones[i].itemsRemaining;
        }

        GameManager.singleton.PlayClickSound();
        WorldTextInfo.singleton.NewMessage("Game Started!");
        gameInProgress = true;
        gameStarting = false;
        currentGameTime = 0.0f;
        if (photonView.IsMine)
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].SpawnItems();
            }
        }

        yield return new WaitForSeconds(2.0f);
        

        WorldTextInfo.singleton.SwapToGameInfo();
        WorldTextInfo.singleton.UpdateScore(returnZones[0].itemsRemaining, returnZones[1].itemsRemaining);
    }

    void FinishZone(int zoneID)
    {
        returnZones[zoneID].Finished();

        bool gameOver = true;
        for (int i = 0; i < returnZones.Length; i++)
        {
            if (!returnZones[i].finished)
            {
                gameOver = false;
            }
        }

        if (gameOver)
        {
            FinishGame();
        }
    }

    public void FinishGame()
    {
        for (int i = 0; i < returnZones.Length; i++)
        {
            returnZones[i].currentText.text = "You Win!";
        }
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(FinishGame_RPC), RpcTarget.All);
        }
    }

    [PunRPC]
    void FinishGame_RPC()
    {
        StartCoroutine(EndGameRoutine());
    }

    IEnumerator EndGameRoutine()
    {
        WorldTextInfo.singleton.SwapToLobbyInfo();
        WorldTextInfo.singleton.NewMessage("Victory!\nTotal Time: "+currentGameTime.ToString("F0"));
        GameManager.singleton.PlayVictorySound();
        justEnded = true;
        GameManager.singleton.PlayFirework();
        foreach (ParticleSystem p in victoryParticles)
            p.Play();
        yield return new WaitForSeconds(1.0f);
        GameManager.singleton.PlayFirework();
        foreach (ParticleSystem p in victoryParticles)
            p.Play();
        yield return new WaitForSeconds(1.0f);
        GameManager.singleton.PlayFirework();
        foreach (ParticleSystem p in victoryParticles)
            p.Play();

        gameInProgress = false;
        yield return new WaitForSeconds(1.0f);
        currentGameTime = 0.0f;
        GameManager.singleton.ResetAllItems();

        yield return new WaitForSeconds(7.0f);
        justEnded = false;
        NetworkPlayer_THF.LocalPlayer.ableToSpawn = true;
        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "GameState", 0 } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }
        for (int j = 0; j < returnZones.Length; j++)
        {
            returnZones[j].currentText.text = "Walk Into Zone To Start";
        }
    }

    private void SubTriggerEntered(int zoneID, ReturnZone zone, Collider collider)
    {
        if (gameInProgress)
        {
            if (collider.CompareTag("GameItem"))
            {
                GameItem newItem = collider.GetComponentInParent<GameItem>();
                if((!newItem.lightItem && spawners[zoneID].upsideDown) || (newItem.lightItem & !spawners[zoneID].upsideDown))
                {
                    GameManager.singleton.PlayPointSound();
                    zone.itemsRemaining--;
                    zone.currentText.text = "Items Remaining: " + (zone.itemsRemaining);

                    WorldTextInfo.singleton.UpdateScore(returnZones[0].itemsRemaining, returnZones[1].itemsRemaining);

                    if (zone.itemsRemaining <= 0)
                    {
                        //Finish Game
                        WorldTextInfo.singleton.NewMessage("Game Over!");
                        FinishZone(zone.zoneID);
                    }
                }
            }
        }
        else if(!justEnded)
        {
            if (collider.CompareTag("NetworkPlayer"))
            {
                playersReadyForGame.Add(collider.GetComponentInParent<NetworkPlayer>());
                if(photonView.IsMine && playersReadyForGame.Count >= GameManager.singleton.currentPlayers.Count)
                {
                    InitializeGame();
                }
            }
        }
    }

    private void SubTriggerExited(int zoneID, ReturnZone zone, Collider collider)
    {
        if (gameInProgress)
        {
            if (collider.CompareTag("GameItem"))
            {
                GameItem newItem = collider.GetComponentInParent<GameItem>();
                if ((!newItem.lightItem && spawners[zoneID].upsideDown) || (newItem.lightItem & !spawners[zoneID].upsideDown))
                {
                    GameManager.singleton.PlayClickSound();
                    zone.itemsRemaining++;
                    zone.currentText.text = "Items Remaining: " + (zone.itemsRemaining);
                    WorldTextInfo.singleton.UpdateScore(returnZones[0].itemsRemaining, returnZones[1].itemsRemaining);
                }
            }
        }
        else if(!justEnded)
        {
            if (collider.CompareTag("NetworkPlayer"))
            {
                NetworkPlayer p = collider.GetComponentInParent<NetworkPlayer>();
                if (playersReadyForGame.Contains(p))
                    playersReadyForGame.Remove(collider.GetComponentInParent<NetworkPlayer>());
            }
        }
    }
}
