using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int score = 0;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        instance = this;
        UpdateUI();
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Puntos: " + score;
    }
}
