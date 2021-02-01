using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    public List<GameItem> gameItemList;
    public List<NetworkPlayer> currentPlayers;

    public AudioSource sunWorldAudio;
    public AudioSource moonWorldAudio;
    public AudioSource sunWorldMusic;
    public AudioSource moonWorldMusic;

    public AudioSource oneShotAudio;
    public AudioClip[] extraClips;
    int currentItemCount = 0;
    public void Awake()
    {
        gameItemList = new List<GameItem>();
        singleton = this;
    }

    public void ResetAllItems()
    {
        currentItemCount = 0;
        for (int i = 0; i < gameItemList.Count; i++)
        {
            if(gameItemList[i] != null)
            {
                Destroy(gameItemList[i].gameObject);
            }
        }

        gameItemList.Clear();
    }

    public int GetNewItemID(GameItem newItem)
    {
        gameItemList.Add(newItem);
        return currentItemCount++;
    }

    public void AddPlayer(NetworkPlayer player)
    {
        currentPlayers.Add(player);
    }

    public void RemovePlayer(NetworkPlayer player)
    {
        if(currentPlayers.Contains(player))
            currentPlayers.Remove(player);
    }

    public void SetZone(int zoneID)
    {
        switch (zoneID)
        {
            case 0:
                sunWorldAudio.Play();
                moonWorldAudio.Stop();
                sunWorldMusic.volume = 1.0f;
                moonWorldMusic.volume = 0.0f;
                break;
            case 1:
                sunWorldAudio.Stop();
                moonWorldAudio.Play();
                moonWorldMusic.volume = 1.0f;
                sunWorldMusic.volume = 0.0f;
                break;
        }
    }

    public void PlayClickSound()
    {
        oneShotAudio.PlayOneShot(extraClips[0]);
    }

    public void PlayTickSound()
    {
        oneShotAudio.PlayOneShot(extraClips[1]);
    }

    public void PlayPointSound()
    {
        oneShotAudio.PlayOneShot(extraClips[2]);
    }

    public void PlayVictorySound()
    {
        oneShotAudio.PlayOneShot(extraClips[3]);
    }

    public void PlayFirework()
    {
        oneShotAudio.PlayOneShot(extraClips[4]);
    }
}
