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
    
    // Cài đặt Âm thanh
    public AudioSource musicSource; // Nhạc nền của game
    public AudioSource errorSFXSource; // SFX lỗi
    
    // Cài đặt BPM Sync
    public float musicBPM = 40f; 
    public float spawnZPosition = 5f; 
    public float hitZPosition = -2f; 
    
    // Cài đặt Điểm Bonus (khoảng cách từ HitBar)
    public float perfectRange = 0.2f;  // ±0.2 units từ HitBar
    public float greatRange = 0.5f;    // ±0.5 units từ HitBar
    public float goodRange = 1.0f;     // ±1.0 units từ HitBar
    
    [HideInInspector] public float currentSpeed; 
    
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
        currentSpeed = distance / spawnTimeInterval; 
        
        Time.timeScale = 1f; // Khởi động game
    }

    void Start()
    {
        // Load High Score từ PlayerPrefs
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        
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
        
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play(); // Bắt đầu phát nhạc
        }
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
        
        // Thêm điểm combo bonus (mỗi 10 combo thêm 1 điểm)
        int comboBonus = combo / 10;
        scoreToAdd += comboBonus;
        
        score += scoreToAdd;
        
        UpdateScoreUI();
        UpdateComboUI();
        ShowFeedback(feedback);
    }

    // Phương thức cũ cho tương thích ngược (không có bonus)
    public void AddScore()
    {
        score += normalScore;
        combo++;
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
                comboText.text = "Combo: " + combo.ToString() + "x";
            }
            else
            {
                comboText.text = "";
            }
        }
    }
    
    private void ShowFeedback(string text)
    {
        if (feedbackText != null)
        {
            // Stop các animation cũ
            StopAllCoroutines();
            CancelInvoke("ClearFeedback");
            
            feedbackText.text = text;
            
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