using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Tham chiếu UI và Assets (GÁN TRONG INSPECTOR)
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public AudioSource musicSource; 
    
    // Tham số BPM Sync (CẦN GÁN TRONG INSPECTOR)
    public float musicBPM = 120f;   // Tốc độ nhạc (120 là ví dụ)
    public float spawnZPosition = 15f; // Vị trí Z mà ô gạch được sinh ra
    public float hitZPosition = -3f;   // Vị trí Z mà người chơi cần chạm/Miss
    
    [HideInInspector] public int score = 0;
    [HideInInspector] public float currentSpeed; // Tốc độ di chuyển cố định (Tính toán)

    void Awake()
    {
        // Tính toán tốc độ cần thiết để các ô khớp với nhịp (Distance / SpawnTime)
        float spawnTime = 60f / musicBPM;
        float distance = spawnZPosition - hitZPosition; 
        currentSpeed = distance / spawnTime;

        // Thiết lập ban đầu
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (musicSource != null)
        {
            musicSource.Play(); // Bắt đầu phát nhạc
        }
        UpdateScoreUI();
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI();
    }

    public void GameOver()
    {
        if (musicSource != null)
        {
            musicSource.Stop(); // Dừng nhạc
        }

        Time.timeScale = 0f; // Dừng game

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Hiện Game Over Panel
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}