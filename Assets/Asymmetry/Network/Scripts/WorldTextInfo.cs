using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldTextInfo : MonoBehaviour
{
    public static WorldTextInfo singleton;
    public GameObject[] gameInfo;
    public GameObject[] lobbyInfo;

    public TMP_Text[] dynamicText;
    public TMP_Text[] gameTimerText;
    public TMP_Text[] scoreTextLeft;
    public TMP_Text[] scoreTextRight;

    public void Awake()
    {
        singleton = this;
        SwapToLobbyInfo();
    }

    public void SwapToGameInfo()
    {
        for (int i = 0; i < gameInfo.Length; i++)
        {
            gameInfo[i].SetActive(true);
            lobbyInfo[i].SetActive(false);
        }
    }

    public void SwapToLobbyInfo()
    {
        for (int i = 0; i < gameInfo.Length; i++)
        {
            gameInfo[i].SetActive(false);
            lobbyInfo[i].SetActive(true);
        }
    }
    
    public void NewMessage(string text)
    {
        for (int i = 0; i < dynamicText.Length; i++)
        {
            dynamicText[i].text = text;
        }
    }

    public void UpdateScore(int score1, int score2)
    {
        for (int i = 0; i < scoreTextLeft.Length; i++)
        {
            scoreTextLeft[i].text = score1.ToString();
            scoreTextRight[i].text = score2.ToString();
        }
    }

    public void UpdateGameTimer(string gameTime)
    {
        for (int i = 0; i < gameTimerText.Length; i++)
        {
            gameTimerText[i].text = "Time Elapsed: " + gameTime;
        }
    }
}
