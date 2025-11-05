using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    // Cài đặt Game Play
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    
    // Cài đặt Âm thanh
    public AudioSource musicSource; // Nhạc nền của game
    public AudioSource errorSFXSource; // SFX lỗi
    
    // Cài đặt BPM Sync
    public float musicBPM = 40f; 
    public float spawnZPosition = 5f; 
    public float hitZPosition = -2f; 
    
    [HideInInspector] public float currentSpeed; 
    
    private int score = 0;

    void Awake()
    {
        float spawnTimeInterval = 60f / musicBPM;
        float distance = spawnZPosition - hitZPosition;
        currentSpeed = distance / spawnTimeInterval; 
        
        Time.timeScale = 1f; // Khởi động game
    }

    void Start()
    {
        UpdateScoreUI(); 
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play(); // Bắt đầu phát nhạc
        }
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI(); 
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
    
    public void GameOver()
    {
        // LOGIC DỪNG NHẠC KHI GAME OVER
        if (musicSource != null)
        {
            musicSource.Stop(); 
        }

        if (errorSFXSource != null)
        {
            errorSFXSource.Play();
        }
        
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}