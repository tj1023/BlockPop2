using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("----------[ UI ]----------")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestScoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverBestScoreText;
    
    private void OnEnable()
    {
        GameEvents.OnScoreChanged += HandleScoreChanged;
        GameEvents.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleScoreChanged;
        GameEvents.OnTimeChanged -= HandleTimeChanged;

    }
    
    private void Start()
    {
        if(!PlayerPrefs.HasKey("BestScore"))
            PlayerPrefs.SetInt("BestScore", 0);
        
        if(bestScoreText != null)
            bestScoreText.text = "Best : " + PlayerPrefs.GetInt("BestScore").ToString();
    }

    private void HandleScoreChanged(int score)
    {
        scoreText.text = $"Score : {score}";
    }
    
    private void HandleTimeChanged(int time)
    {
        timeText.text = time >= 0 ? Mathf.FloorToInt(time).ToString() : "0";
    }

    public void GameOver(int score)
    {
        gameOverScoreText.text = scoreText.text;
        int bestScore = Mathf.Max(score, PlayerPrefs.GetInt("BestScore"));
        PlayerPrefs.SetInt("BestScore", bestScore);
        gameOverBestScoreText.text = $"Best : {bestScore}";
        gameOverUI.SetActive(true);
    }

    public void SetGameOverUIActive(bool active)
    {
        gameOverUI.SetActive(active);
    }
}
