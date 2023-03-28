using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardController : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> playerNameTexts;
    [SerializeField] List<TextMeshProUGUI> scoreTexts;
    [SerializeField] List<TextMeshProUGUI> killBonusTexts;
    [SerializeField] List<TextMeshProUGUI> finalScoreTexts;
    [SerializeField] GameObject victoryImage;
    [SerializeField] GameObject defeatImage;

    int[] scores = new int[2];
    int[] killBonus = new int[2];

    public void FinishGame()
    {
        //calculate and display final result
        for(var i = 0; i < scores.Length; i++)
        {
            int result = scores[i] + killBonus[i];
            finalScoreTexts[i].text = result.ToString();
        }
    }
    public void SetScore(int playerNumber, int score)
    {
        scoreTexts[playerNumber].text = score.ToString();
        scores[playerNumber] = score;
    }

    public void SetPlayerName(int playerNumber, string name)
    {
        playerNameTexts[playerNumber].text = name;
    }

    public void SetKillBonus(int playerNumber, int bonus)
    {
        killBonusTexts[playerNumber].text = "+" + bonus.ToString();
        killBonus[playerNumber] = bonus;
    }

    public void SetVictoryOrDefeat(int playerNumber)
    {
        int otherPlayerNumber = 1;
        if(playerNumber == 1)
            otherPlayerNumber = 0;

        int thisPlayerScore = scores[playerNumber] + killBonus[playerNumber];
        int otherPlayerScore = scores[otherPlayerNumber] + killBonus[otherPlayerNumber];

        if(thisPlayerScore >= otherPlayerScore)
        {
            victoryImage.SetActive(true);
        }
        else
        {
            defeatImage.SetActive(true);
        }
    }
}
