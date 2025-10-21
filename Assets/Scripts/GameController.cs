using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Tham chiếu UI (Cần gắn từ Inspector)
    public TextMeshProUGUI scoreText; 
    public GameObject gameOverPanel;
    
    // Tham chiếu Materials (Cần gắn từ Inspector)
    public Material blackTileMat;
    public Material hitTileMat; // Material màu xám khi chạm đúng

    // Cấu hình Game
    public int score = 0;
    public float initialSpeed = 10f;
    public float speedIncreaseRate = 0.2f; 
    [HideInInspector] public float currentSpeed;

    void Start()
    {
        // Khởi tạo trạng thái game
        currentSpeed = initialSpeed;
        score = 0;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateScoreUI();
        Time.timeScale = 1f; 
    }

    public void AddScore()
    {
        score++;
        currentSpeed += speedIncreaseRate; 
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void GameOver()
    {
        // 1. DỪNG GAME
        Time.timeScale = 0f; 
        
        // 2. KÍCH HOẠT HIỆU ỨNG NHẤP NHÁY TRÊN TẤT CẢ CÁC Ô
        Tile[] activeTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in activeTiles)
        {
            // False: đây là Game Over chung, không phải miss một ô đen cụ thể
            tile.StartFlash(false); 
        }

        // 3. HIỂN THỊ GAME OVER PANEL 
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // Tải lại Scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}