using System;
using TMPro;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
    public static ScoreScript Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;

    private int _score;
    public int Score
    {
        get => _score;
        set
        {
            if (_score == value) return;
            _score = value;
            scoreText.SetText($"Score = {_score}");
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
