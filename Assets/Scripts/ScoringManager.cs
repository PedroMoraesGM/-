using DG.Tweening;
using System.IO;
using TMPro;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    private int _score = 0;
    private int _comboMultiplier = 1;
    private int _consecutiveMatches = 0;

    [SerializeField] private int matchScore = 10;
    [SerializeField] private int mismatchPenalty = 2;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _highscoreMenuText;
    [SerializeField] private TextMeshProUGUI _highscoreGameText;

    private string saveFilePath;
    private const string HighScoreKey = "HighScore";

    private void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "ScoreData.json");

        GameEventsManager.Instance.OnCardCompared += HandleCardCompared;
        GameEventsManager.Instance.OnGameover += HandleGameover;
        GameEventsManager.Instance.OnGameStarted += HandleGameStart;
        GameEventsManager.Instance.OnGameLoaded += HandleGameLoad;

        LoadHighScore();

        UpdateScoreUI();
    }

    private void HandleCardCompared(bool isMatch)
    {
        if (isMatch)
        {
            _consecutiveMatches++;
            _comboMultiplier = 1 + (_consecutiveMatches / 2);
            _score += matchScore * _comboMultiplier;

            _scoreText.transform.DOPunchScale(new Vector3(0.15f, 0.15f), 0.3f); // Scale effect for increasing score
            _scoreText.DOColor(Color.green, 0.2f).OnComplete(() => _scoreText.DOColor(Color.white, 0.2f)); // Fade to green
        }
        else
        {
            int previousScore = _score;
            _consecutiveMatches = 0;
            _comboMultiplier = 1;
            _score = Mathf.Max(0, _score - mismatchPenalty);

            if (_score < previousScore) // Only play effect if the score decreased
            {
                _scoreText.transform.DOPunchScale(new Vector3(-0.15f, -0.15f), 0.3f);
                _scoreText.DOColor(Color.red, 0.2f).OnComplete(() => _scoreText.DOColor(Color.white, 0.2f)); // Fade to red
            }
        }

        SaveScoreData();
        UpdateScoreUI();
    }

    private void HandleGameover()
    {
        CheckAndSetHighScore();
        ClearScoreData();
    }

    private void HandleGameStart()
    {
        ResetScoreData();
    }

    private void HandleGameLoad()
    {
        LoadScoreData();
    }

    private void UpdateScoreUI()
    {
        _scoreText.text = "Score: " + _score;
        LoadHighScore();
    }

    private void SaveScoreData()
    {
        ScoreData data = new ScoreData(_score, _consecutiveMatches, _comboMultiplier);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    private void LoadScoreData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);

            _score = data.score;
            _consecutiveMatches = data.consecutiveMatches;
            _comboMultiplier = data.comboMultiplier;
        }

        UpdateScoreUI();
    }

    private void ClearScoreData()
    {
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);
    }

    private void ResetScoreData()
    {
        _score = 0;
        _consecutiveMatches = 0;
        _comboMultiplier = 1;
        SaveScoreData();
        UpdateScoreUI();
    }

    private void CheckAndSetHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (_score > currentHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, _score);
            PlayerPrefs.Save(); // Save to ensure data persists

            LoadHighScore();
            _highscoreGameText.transform.DOPunchScale(new Vector3(0.15f, 0.15f), 0.3f); // Play animation to indicate new highscore
        }
    }

    private void LoadHighScore()
    {
        int highscore = PlayerPrefs.GetInt(HighScoreKey, 0);

        _highscoreMenuText.text = "High Score: " + highscore;
        _highscoreGameText.text = _highscoreMenuText.text;
    }

    private void OnDestroy()
    {
        GameEventsManager.Instance.OnCardCompared -= HandleCardCompared;
        GameEventsManager.Instance.OnGameover -= HandleGameover;
        GameEventsManager.Instance.OnGameStarted -= HandleGameStart;
        GameEventsManager.Instance.OnGameLoaded -= HandleGameLoad;
    }
}

[System.Serializable]
public class ScoreData
{
    public int score;
    public int consecutiveMatches;
    public int comboMultiplier;

    public ScoreData(int score, int consecutiveMatches, int comboMultiplier)
    {
        this.score = score;
        this.consecutiveMatches = consecutiveMatches;
        this.comboMultiplier = comboMultiplier;
    }
}
