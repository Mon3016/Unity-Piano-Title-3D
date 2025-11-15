using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    // Cài đặt Game Play
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText; // Text hiển thị High Score
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI feedbackText; // Text hiển thị Perfect/Great/Good
    public GameObject gameOverPanel;
    
    public MenuManager menuManager; // Menu Manager reference
    
    // Cài đặt Âm thanh
    public AudioSource musicSource; // Nhạc nền của game
    public AudioSource errorSFXSource; // SFX lỗi
    
    // Cài đặt BPM Sync
    public float musicBPM = 40f; 
    public float spawnZPosition = 30f; // Tăng từ 5 lên 30 để tiles spawn xa hơn
    public float hitZPosition = -2f; 
    
    // Cài đặt Combo Multiplier
    public bool enableComboMultiplier = true; // Bật/tắt nhân combo
    public float maxComboMultiplier = 4f; // Hệ số nhân tối đa (x4)
    public int comboForMaxMultiplier = 20; // Combo cần để đạt max multiplier
    
    // Cài đặt Speed Increase
    public bool enableSpeedIncrease = true; // Bật/tắt tăng tốc
    public float maxSpeedMultiplier = 2f; // Tốc độ tối đa (x2)
    public int comboForMaxSpeed = 30; // Combo cần để đạt max speed
    
    // Cài đặt Điểm Bonus (khoảng cách từ HitBar)
    public float perfectRange = 0.2f;  // ±0.2 units từ HitBar
    public float greatRange = 0.5f;    // ±0.5 units từ HitBar
    public float goodRange = 1.0f;     // ±1.0 units từ HitBar
    
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float baseSpeed; // Tốc độ gốc (không đổi) 
    
    private int score = 0;
    private int highScore = 0;
    private int combo = 0;
    private int perfectScore = 10;
    private int greatScore = 7;
    private int goodScore = 5;
    private int normalScore = 3;
    
    private const string HIGH_SCORE_KEY = "HighScore"; // Key để lưu vào PlayerPrefs

    void Awake()
    {
        float spawnTimeInterval = 60f / musicBPM;
        float distance = spawnZPosition - hitZPosition;
        baseSpeed = distance / spawnTimeInterval; // Lưu tốc độ gốc
        currentSpeed = baseSpeed; // Khởi tạo tốc độ hiện tại
        
        // KHÔNG khởi động game ngay - để MenuManager điều khiển
        // Time.timeScale sẽ = 0 từ MenuManager khi hiện Main Menu
    }

    void Start()
    {
        // Load High Score từ PlayerPrefs
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        
        // Tìm MenuManager nếu chưa được gán
        if (menuManager == null)
        {
            menuManager = FindFirstObjectByType<MenuManager>();
        }
        
        // Tự động tìm UI Text nếu chưa được gán
        AutoFindUIElements();
        
        // Khởi tạo text ban đầu
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
        
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateComboUI();
        
        // KHÔNG tự động phát nhạc - để MenuManager điều khiển
        // MenuManager sẽ phát nhạc khi người chơi chọn bài và bắt đầu game
    }
    
    // Tự động tìm UI elements nếu chưa được gán trong Inspector
    private void AutoFindUIElements()
    {
        // Tìm ScoreText
        if (scoreText == null)
        {
            string[] possibleScoreNames = { "ScoreText", "ScoreTextUI", "Score", "ScoreUI" };
            foreach (string name in possibleScoreNames)
            {
                GameObject scoreObj = GameObject.Find(name);
                if (scoreObj != null)
                {
                    scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
                    if (scoreText != null) break;
                }
            }
        }
        
        // Setup ScoreText
        if (scoreText != null)
        {
            SetupScoreText(scoreText);
        }
        
        // Tìm và setup HighScoreText
        if (highScoreText == null)
        {
            string[] possibleNames = { "HighScoreText", "HighScore", "BestScore", "BestScoreText" };
            foreach (string name in possibleNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    highScoreText = obj.GetComponent<TextMeshProUGUI>();
                    if (highScoreText != null) break;
                }
            }
        }
        
        // Setup HighScoreText
        if (highScoreText != null)
        {
            SetupHighScoreText(highScoreText);
        }
        
        // Tìm ComboText
        if (comboText == null)
        {
            string[] possibleNames = { "ComboText", "ComboTextUI", "ComboUI", "Combo" };
            foreach (string name in possibleNames)
            {
                GameObject comboObj = GameObject.Find(name);
                if (comboObj != null)
                {
                    comboText = comboObj.GetComponent<TextMeshProUGUI>();
                    if (comboText != null) break;
                }
            }
        }
        
        // Setup ComboText
        if (comboText != null)
        {
            SetupComboText(comboText);
        }
        
        // Tìm FeedbackText
        if (feedbackText == null)
        {
            string[] possibleNames = { "FeedbackText", "FeedbackUI", "Feedback" };
            foreach (string name in possibleNames)
            {
                GameObject feedbackObj = GameObject.Find(name);
                if (feedbackObj != null)
                {
                    feedbackText = feedbackObj.GetComponent<TextMeshProUGUI>();
                    if (feedbackText != null) break;
                }
            }
        }
        
        // Setup FeedbackText
        if (feedbackText != null)
        {
            SetupFeedbackText(feedbackText);
        }
    }

    // Tính điểm dựa trên độ chính xác khi nhấn
    public void AddScore(float tileZPosition)
    {
        // Debug logs - comment nếu không cần debug
        // Debug.Log("==== [GameController] AddScore(float) Called ====");
        
        float distance = Mathf.Abs(tileZPosition - hitZPosition);
        
        int scoreToAdd = 0;
        string feedback = "";
        
        if (distance <= perfectRange)
        {
            scoreToAdd = perfectScore;
            feedback = "PERFECT!";
            combo++;
        }
        else if (distance <= greatRange)
        {
            scoreToAdd = greatScore;
            feedback = "GREAT!";
            combo++;
        }
        else if (distance <= goodRange)
        {
            scoreToAdd = goodScore;
            feedback = "GOOD";
            combo++;
        }
        else
        {
            scoreToAdd = normalScore;
            feedback = "HIT";
            combo++;
        }
        
        // ===== COMBO MULTIPLIER =====
        float comboMultiplier = 1f;
        if (enableComboMultiplier && combo > 1)
        {
            // Tính hệ số nhân dựa trên combo (từ 1x đến maxComboMultiplier)
            comboMultiplier = 1f + ((maxComboMultiplier - 1f) * Mathf.Min(combo, comboForMaxMultiplier) / comboForMaxMultiplier);
            scoreToAdd = Mathf.RoundToInt(scoreToAdd * comboMultiplier);
        }
        
        score += scoreToAdd;
        
        // ===== SPEED INCREASE =====
        UpdateSpeed();
        
        UpdateScoreUI();
        UpdateComboUI();
        ShowFeedback(feedback, comboMultiplier);
    }

    // Cập nhật tốc độ dựa trên combo
    private void UpdateSpeed()
    {
        if (!enableSpeedIncrease)
        {
            currentSpeed = baseSpeed;
            return;
        }
        
        // Tính hệ số tốc độ dựa trên combo (từ 1x đến maxSpeedMultiplier)
        float speedMultiplier = 1f + ((maxSpeedMultiplier - 1f) * Mathf.Min(combo, comboForMaxSpeed) / comboForMaxSpeed);
        currentSpeed = baseSpeed * speedMultiplier;
    }
    
    // Phương thức cũ cho tương thích ngược (không có bonus)
    public void AddScore()
    {
        score += normalScore;
        combo++;
        UpdateSpeed();
        UpdateScoreUI();
        UpdateComboUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
            
            // Auto-fix Z position nếu cần
            RectTransform rectTransform = scoreText.GetComponent<RectTransform>();
            if (rectTransform != null && Mathf.Abs(rectTransform.localPosition.z) > 0.1f)
            {
                Vector3 pos = rectTransform.localPosition;
                pos.z = 0;
                rectTransform.localPosition = pos;
            }
            
            // Check và update High Score
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                PlayerPrefs.Save();
                UpdateHighScoreUI();
            }
        }
    }
    
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + highScore.ToString();
        }
    }
    
    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                // Tính combo multiplier để hiển thị
                float comboMultiplier = 1f;
                if (enableComboMultiplier && combo > 1)
                {
                    comboMultiplier = 1f + ((maxComboMultiplier - 1f) * Mathf.Min(combo, comboForMaxMultiplier) / comboForMaxMultiplier);
                }
                
                // Hiển thị combo với multiplier
                if (comboMultiplier > 1f)
                {
                    comboText.text = string.Format("Combo: {0}x (×{1:F1})", combo, comboMultiplier);
                }
                else
                {
                    comboText.text = "Combo: " + combo.ToString() + "x";
                }
                
                // Thay đổi màu theo mức combo
                if (combo >= comboForMaxMultiplier)
                {
                    comboText.color = new Color(1f, 0.2f, 0.2f, 1f); // Đỏ - MAX COMBO!
                }
                else if (combo >= comboForMaxMultiplier / 2)
                {
                    comboText.color = new Color(1f, 0.5f, 0f, 1f); // Cam - Combo cao
                }
                else if (combo >= 5)
                {
                    comboText.color = new Color(1f, 0.92f, 0.016f, 1f); // Vàng - Combo trung bình
                }
                else
                {
                    comboText.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Xám - Combo thấp
                }
            }
            else
            {
                comboText.text = "";
            }
        }
    }
    
    private void ShowFeedback(string text, float multiplier = 1f)
    {
        if (feedbackText != null)
        {
            // Stop các animation cũ
            StopAllCoroutines();
            CancelInvoke("ClearFeedback");
            
            // Hiển thị feedback với multiplier nếu > 1
            if (multiplier > 1f)
            {
                feedbackText.text = text + "\n×" + multiplier.ToString("F1");
            }
            else
            {
                feedbackText.text = text;
            }
            
            // Thay đổi màu sắc theo loại feedback
            switch (text)
            {
                case "PERFECT!":
                    feedbackText.color = new Color(0f, 1f, 0.3f, 1f); // Xanh lá neon
                    feedbackText.fontSize = 90;
                    break;
                case "GREAT!":
                    feedbackText.color = new Color(0.2f, 0.8f, 1f, 1f); // Xanh dương sáng
                    feedbackText.fontSize = 80;
                    break;
                case "GOOD":
                    feedbackText.color = new Color(1f, 0.7f, 0f, 1f); // Cam
                    feedbackText.fontSize = 70;
                    break;
                case "HIT":
                    feedbackText.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Xám sáng
                    feedbackText.fontSize = 60;
                    break;
            }
            
            // Bắt đầu hiệu ứng bounce
            StartCoroutine(ScaleBounce());
            
            // Fade out sau 0.6 giây
            Invoke("ClearFeedback", 0.6f);
        }
    }
    
    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            // Fade out animation
            StartCoroutine(FadeOutFeedback());
        }
    }
    
    // Hiệu ứng fade out cho feedback text
    private System.Collections.IEnumerator FadeOutFeedback()
    {
        if (feedbackText == null) yield break;
        
        float fadeDuration = 0.3f;
        float elapsedTime = 0f;
        Color startColor = feedbackText.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        feedbackText.text = "";
        feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, 1f); // Reset alpha
    }
    
    // Hiệu ứng scale bounce cho feedback text
    private System.Collections.IEnumerator ScaleBounce()
    {
        if (feedbackText == null) yield break;
        
        RectTransform rectTransform = feedbackText.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;
        
        Vector3 originalScale = rectTransform.localScale;
        float duration = 0.2f;
        float elapsedTime = 0f;
        
        // Scale up
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(0.8f, 1.2f, elapsedTime / duration);
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Scale back to normal
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1.2f, 1f, elapsedTime / duration);
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
    }
    
    public void ResetCombo()
    {
        combo = 0;
        UpdateSpeed(); // Reset tốc độ về ban đầu
        UpdateComboUI();
    }
    

    
    // Setup ComboText ở góc trên bên trái với màu nổi bật
    private void SetupComboText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Fix Z position = 0
        Vector3 localPos = rectTransform.localPosition;
        localPos.z = 0;
        rectTransform.localPosition = localPos;
        
        // Đặt anchor ở góc trên bên trái
        rectTransform.anchorMin = new Vector2(0, 1); // Top-Left
        rectTransform.anchorMax = new Vector2(0, 1); // Top-Left
        rectTransform.pivot = new Vector2(0, 1);
        
        // Vị trí: cách mép trái 30px, cách mép trên 30px
        rectTransform.anchoredPosition = new Vector2(30, -30);
        
        // Kích thước
        rectTransform.sizeDelta = new Vector2(350, 70);
        
        // Màu sắc nổi bật - Vàng Gold
        textComponent.color = new Color(1f, 0.92f, 0.016f, 1f);
        
        // Font size
        textComponent.fontSize = 48;
        
        // Font style: Bold
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        
        // Alignment: Căn trái-trên
        textComponent.alignment = TMPro.TextAlignmentOptions.TopLeft;
    }
    
    // Setup ScoreText với màu nổi bật
    private void SetupScoreText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Fix Z position = 0
        Vector3 localPos = rectTransform.localPosition;
        localPos.z = 0;
        rectTransform.localPosition = localPos;
        
        // Màu sắc nổi bật - TRẮNG
        textComponent.color = new Color(1f, 1f, 1f, 1f);
        
        // Font size lớn
        textComponent.fontSize = 48;
        
        // Font style: Bold
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        
        // Force enable
        textComponent.enabled = true;
        textComponent.gameObject.SetActive(true);
    }
    
    // Setup HighScoreText
    private void SetupHighScoreText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Fix Z position = 0
        Vector3 localPos = rectTransform.localPosition;
        localPos.z = 0;
        rectTransform.localPosition = localPos;
        
        // Màu sắc - Vàng Gold để phân biệt
        textComponent.color = new Color(1f, 0.84f, 0f, 1f); // Gold
        
        // Font size nhỏ hơn score một chút
        textComponent.fontSize = 36;
        
        // Font style: Bold
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        
        // Force enable
        textComponent.enabled = true;
        textComponent.gameObject.SetActive(true);
    }
    
    // Setup FeedbackText ở giữa màn hình với màu sắc động
    private void SetupFeedbackText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Fix Z position = 0
        Vector3 localPos = rectTransform.localPosition;
        localPos.z = 0;
        rectTransform.localPosition = localPos;
        
        // Đặt anchor ở giữa màn hình
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f); // Center
        rectTransform.pivot = new Vector2(0.5f, 0.5f); // Pivot ở giữa
        
        // Vị trí giữa màn hình, hơi lên trên
        rectTransform.anchoredPosition = new Vector2(0, 180);
        
        // Kích thước lớn
        rectTransform.sizeDelta = new Vector2(700, 140);
        
        // Màu sắc mặc định - Vàng sáng (sẽ thay đổi theo rating)
        textComponent.color = new Color(1f, 0.95f, 0.2f, 1f); // Vàng sáng
        
        // Font size rất lớn
        textComponent.fontSize = 80;
        
        // Font style: Bold
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        
        // Alignment: Căn giữa
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
    }
    
    // Fix vị trí UI cho Screen Space - Camera mode
    private void FixUIPosition(TextMeshProUGUI textComponent, string elementName)
    {
        if (textComponent == null) return;
        
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Fix Z position = 0
        Vector3 localPos = rectTransform.localPosition;
        if (Mathf.Abs(localPos.z) > 0.1f)
        {
            Debug.LogWarning($"[GameController] {elementName} Z was {localPos.z}, fixing to 0");
            localPos.z = 0;
            rectTransform.localPosition = localPos;
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
        
        ResetCombo(); // Reset combo khi game over
        
        Time.timeScale = 0f;
        
        // Sử dụng MenuManager nếu có
        if (menuManager != null)
        {
            menuManager.ShowGameOver(score, highScore);
        }
        else if (gameOverPanel != null)
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