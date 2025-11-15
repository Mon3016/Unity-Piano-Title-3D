using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    
    [Header("Song Selection")]
    public GameObject songSelectionPanel;
    public RectTransform songListContent; // Content area của ScrollView
    public SongData[] availableSongs;
    
    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    
    private bool isPaused = false;
    private GameController gameController;
    
    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
        
        // Đảm bảo Canvas có sortingOrder cao để menu hiển thị trước các vạch nốt
        EnsureCanvasOnTop();
        
        // Tự động wire button events
        WireButtonEvents();
        
        // Hiển thị Main Menu và PAUSE game
        if (mainMenuPanel != null)
        {
            ShowMainMenu(); // Sẽ pause game
        }
    }
    
    // Đảm bảo Menu Canvas có sortingOrder cao để menu luôn hiển thị trước các đối tượng 3D
    private void EnsureCanvasOnTop()
    {
        // Tìm MenuCanvas (Canvas riêng cho menu)
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        // Nếu không có MenuCanvas, tìm Canvas đầu tiên (fallback)
        if (menuCanvas == null)
        {
            menuCanvas = FindFirstObjectByType<Canvas>();
        }
        
        if (menuCanvas != null)
        {
            // Đảm bảo MenuCanvas luôn active khi ở menu
            menuCanvas.gameObject.SetActive(true);
            
            // BẮT BUỘC override sorting để sortingOrder có hiệu lực
            menuCanvas.overrideSorting = true;
            
            // Đặt sortingOrder RẤT CAO để menu UI luôn render TRƯỚC tất cả
            menuCanvas.sortingOrder = 32767; // Max sorting order
            Debug.Log($"✅ {menuCanvas.name} overrideSorting = true, sortingOrder = 32767 - Menu will ALWAYS display in front");
            
            // SỬ DỤNG Screen Space Overlay để menu LUÔN render trên cùng
            if (menuCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log($"✅ {menuCanvas.name} renderMode set to ScreenSpaceOverlay - Menu ALWAYS on top");
            }
        }
        
        // Đảm bảo game Canvas có sortingOrder thấp hơn MenuCanvas
        foreach (Canvas c in allCanvases)
        {
            if (c.name != "MenuCanvas" && c.name.Contains("Canvas"))
            {
                // Game Canvas nên có sortingOrder thấp
                if (c.sortingOrder >= 1000)
                {
                    c.sortingOrder = 0;
                    Debug.Log($"✅ Game Canvas ({c.name}) sortingOrder set to 0 - will render below menu");
                }
            }
        }
    }
    
    // Tự động wire tất cả button events
    private void WireButtonEvents()
    {
        // Main Menu buttons
        if (mainMenuPanel != null)
        {
            FindAndWireButton(mainMenuPanel, "PlayButton", ShowSongSelection);
            FindAndWireButton(mainMenuPanel, "QuitButton", QuitGame);
        }
        
        // Pause Menu buttons
        if (pauseMenuPanel != null)
        {
            FindAndWireButton(pauseMenuPanel, "ResumeButton", ResumeGame);
            FindAndWireButton(pauseMenuPanel, "RestartButton", RestartGame);
            FindAndWireButton(pauseMenuPanel, "MainMenuButton", BackToMainMenu);
        }
        
        // Song Selection buttons
        if (songSelectionPanel != null)
        {
            FindAndWireButton(songSelectionPanel, "BackButton", ShowMainMenu);
        }
        
        // Game Over buttons
        if (gameOverPanel != null)
        {
            FindAndWireButton(gameOverPanel, "RestartButton2", RestartGame);
            FindAndWireButton(gameOverPanel, "MainMenuButton2", BackToMainMenu);
        }
        
        Debug.Log("✅ All button events wired successfully!");
    }
    
    // Helper method to find and wire a button (tìm sâu trong hierarchy)
    private void FindAndWireButton(GameObject panel, string buttonName, UnityEngine.Events.UnityAction action)
    {
        if (panel == null) return;
        
        // Tìm button trong panel và tất cả children
        Transform buttonTransform = FindChildRecursive(panel.transform, buttonName);
        if (buttonTransform != null)
        {
            UnityEngine.UI.Button button = buttonTransform.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners(); // Clear old listeners
                button.onClick.AddListener(action);
                Debug.Log($"✅ Wired: {buttonName}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Found '{buttonName}' but no Button component!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Button '{buttonName}' not found in {panel.name}!");
        }
    }
    
    // Tìm child recursively trong hierarchy
    private Transform FindChildRecursive(Transform parent, string name)
    {
        // Kiểm tra trực tiếp
        Transform found = parent.Find(name);
        if (found != null) return found;
        
        // Tìm trong tất cả children
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        
        return null;
    }
    
    void Update()
    {
        // Tìm MenuCanvas và disable nó khi game đang chạy
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        // Đảm bảo menu bị ẩn HOÀN TOÀN khi game đang chạy (Time.timeScale > 0)
        if (Time.timeScale > 0f && !isPaused)
        {
            // DISABLE MenuCanvas hoàn toàn khi game đang chạy
            if (menuCanvas != null && menuCanvas.gameObject.activeSelf)
            {
                menuCanvas.gameObject.SetActive(false);
                Debug.Log("✅ MenuCanvas disabled during gameplay");
            }
            
            // Game đang chạy - ẩn TẤT CẢ menu panels HOÀN TOÀN
            if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            {
                mainMenuPanel.SetActive(false);
                UnityEngine.UI.Image img = mainMenuPanel.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.enabled = false;
            }
            if (songSelectionPanel != null && songSelectionPanel.activeSelf)
            {
                songSelectionPanel.SetActive(false);
                UnityEngine.UI.Image img = songSelectionPanel.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.enabled = false;
            }
            if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            {
                pauseMenuPanel.SetActive(false);
                UnityEngine.UI.Image img = pauseMenuPanel.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.enabled = false;
            }
            // GameOver panel có thể hiển thị khi game over, nên không ẩn ở đây
        }
        else
        {
            // ENABLE MenuCanvas khi ở menu (Time.timeScale = 0)
            if (menuCanvas != null && !menuCanvas.gameObject.activeSelf)
            {
                menuCanvas.gameObject.SetActive(true);
            }
        }
        
        // Nhấn ESC để pause/unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    // ===== MAIN MENU =====
    public void ShowMainMenu()
    {
        // ENABLE MenuCanvas và đảm bảo nó luôn trên cùng
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 32767; // Max sorting order
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // Luôn render trên cùng
            Debug.Log("✅ MenuCanvas enabled - menu ALWAYS on top now");
        }
        
        // Đảm bảo game Canvas có sortingOrder thấp hơn
        foreach (Canvas c in allCanvases)
        {
            if (c.name != "MenuCanvas" && c.name.Contains("Canvas"))
            {
                if (c.sortingOrder >= 1000)
                {
                    c.sortingOrder = 0;
                }
            }
        }
        
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(songSelectionPanel, false);
        Time.timeScale = 0f; // PAUSE game ở main menu
        
        // Dừng nhạc nếu đang phát
        if (gameController != null && gameController.musicSource != null)
        {
            gameController.musicSource.Stop();
        }
    }
    
    public void ShowSongSelection()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(songSelectionPanel, true);
        // VẪN pause khi chọn bài hát
        Time.timeScale = 0f;
        
        // Tạo danh sách bài hát
        PopulateSongList();
        
        // Wire lại BackButton sau khi panel được active
        FindAndWireButton(songSelectionPanel, "BackButton", ShowMainMenu);
    }
    
    private void PopulateSongList()
    {
        if (songListContent == null)
        {
            Debug.LogWarning("⚠️ Song List Content is NULL!");
            return;
        }
        
        // Xóa các song buttons cũ
        foreach (Transform child in songListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Nếu không có bài hát nào, tạo một bài mặc định
        if (availableSongs == null || availableSongs.Length == 0)
        {
            Debug.Log("📝 No songs found, creating default song button...");
            CreateDefaultSongButton();
            return;
        }
        
        // Tạo button cho mỗi bài hát
        for (int i = 0; i < availableSongs.Length; i++)
        {
            int songIndex = i; // Capture for closure
            CreateSongButton(availableSongs[i], songIndex);
        }
    }
    
    private void CreateDefaultSongButton()
    {
        GameObject buttonObj = new GameObject("DefaultSongButton");
        buttonObj.transform.SetParent(songListContent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 80);
        
        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(() => {
            // Bắt đầu game với settings mặc định
            if (gameController != null)
            {
                gameController.musicBPM = 40f;
            }
            StartGame();
        });
        
        // Thêm text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "DEFAULT SONG\nBPM: 40";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        
        Debug.Log("✅ Created default song button");
    }
    
    private void CreateSongButton(SongData song, int index)
    {
        GameObject buttonObj = new GameObject($"Song_{index}");
        buttonObj.transform.SetParent(songListContent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 80);
        
        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(() => SelectSong(index));
        
        // Thêm text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = $"{song.songName}\nBPM: {song.bpm}";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontStyle = TMPro.FontStyles.Bold;
    }
    
    public void StartGame()
    {
        // DISABLE MenuCanvas hoàn toàn khi game bắt đầu
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
            Debug.Log("✅ MenuCanvas disabled - game UI only visible now");
        }
        
        // Đảm bảo game Canvas (với Score, Combo) luôn hiển thị và có sortingOrder thấp
        Canvas gameCanvas = null;
        foreach (Canvas c in allCanvases)
        {
            if (c.name != "MenuCanvas")
            {
                gameCanvas = c;
                break;
            }
        }
        
        if (gameCanvas != null)
        {
            // Đảm bảo game Canvas luôn active và có sortingOrder thấp hơn MenuCanvas
            gameCanvas.gameObject.SetActive(true);
            if (gameCanvas.sortingOrder >= 1000)
            {
                gameCanvas.sortingOrder = 0; // Thấp hơn MenuCanvas (2000)
                Debug.Log($"✅ Game Canvas ({gameCanvas.name}) sortingOrder set to 0 - will render below menu");
            }
        }
        
        // Đảm bảo TẤT CẢ menu panels đều bị ẩn HOÀN TOÀN
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            UnityEngine.UI.Image img1 = mainMenuPanel.GetComponent<UnityEngine.UI.Image>();
            if (img1 != null) img1.enabled = false;
        }
        if (songSelectionPanel != null)
        {
            songSelectionPanel.SetActive(false);
            UnityEngine.UI.Image img2 = songSelectionPanel.GetComponent<UnityEngine.UI.Image>();
            if (img2 != null) img2.enabled = false;
        }
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            UnityEngine.UI.Image img3 = pauseMenuPanel.GetComponent<UnityEngine.UI.Image>();
            if (img3 != null) img3.enabled = false;
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            UnityEngine.UI.Image img4 = gameOverPanel.GetComponent<UnityEngine.UI.Image>();
            if (img4 != null) img4.enabled = false;
        }
        
        isPaused = false;
        Time.timeScale = 1f; // BẮT ĐẦU game khi chọn xong bài hát
        
        // Phát nhạc
        if (gameController != null && gameController.musicSource != null)
        {
            gameController.musicSource.Play();
        }
        
        Debug.Log("✅ Game started - All menus completely hidden");
    }
    
    public void SelectSong(int songIndex)
    {
        if (songIndex < 0 || songIndex >= availableSongs.Length) return;
        
        SongData selectedSong = availableSongs[songIndex];
        
        // Áp dụng thông tin bài hát vào GameController
        if (gameController != null)
        {
            gameController.musicBPM = selectedSong.bpm;
            
            if (gameController.musicSource != null && selectedSong.audioClip != null)
            {
                gameController.musicSource.clip = selectedSong.audioClip;
            }
        }
        
        StartGame();
    }
    
    // ===== PAUSE MENU =====
    public void PauseGame()
    {
        if (Time.timeScale == 0f) return; // Đã pause rồi
        
        // ENABLE MenuCanvas và đảm bảo nó luôn trên cùng
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 32767; // Max sorting order
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // Luôn render trên cùng
            Debug.Log("✅ MenuCanvas enabled - Pause menu ALWAYS on top");
        }
        
        // Đảm bảo game Canvas có sortingOrder thấp hơn
        foreach (Canvas c in allCanvases)
        {
            if (c.name != "MenuCanvas" && c.name.Contains("Canvas"))
            {
                if (c.sortingOrder >= 1000)
                {
                    c.sortingOrder = 0;
                }
            }
        }
        
        isPaused = true;
        Time.timeScale = 0f;
        SetPanelActive(pauseMenuPanel, true);
        
        // Dừng nhạc
        if (gameController != null && gameController.musicSource != null)
        {
            gameController.musicSource.Pause();
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPanelActive(pauseMenuPanel, false);
        
        // Tiếp tục phát nhạc
        if (gameController != null && gameController.musicSource != null)
        {
            gameController.musicSource.UnPause();
        }
    }
    
    // ===== GAME OVER MENU =====
    public void ShowGameOver(int finalScore, int highScore)
    {
        // ENABLE MenuCanvas và đảm bảo nó luôn trên cùng
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 32767; // Max sorting order
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // Luôn render trên cùng
            Debug.Log("✅ MenuCanvas enabled - Game Over menu ALWAYS on top");
        }
        
        // Đảm bảo game Canvas có sortingOrder thấp hơn
        foreach (Canvas c in allCanvases)
        {
            if (c.name != "MenuCanvas" && c.name.Contains("Canvas"))
            {
                if (c.sortingOrder >= 1000)
                {
                    c.sortingOrder = 0;
                }
            }
        }
        
        // Hiển thị Game Over panel và ẩn các panel khác
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(songSelectionPanel, false);
        SetPanelActive(gameOverPanel, true);
        
        // Pause game
        Time.timeScale = 0f;
        
        if (finalScoreText != null)
        {
            finalScoreText.text = "Score: " + finalScore.ToString();
        }
        
        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + highScore.ToString();
        }
        
        Debug.Log("✅ Game Over menu displayed");
    }
    
    public void RestartGame()
    {
        // Ẩn tất cả menu và bắt đầu game lại
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(songSelectionPanel, false);
        SetPanelActive(gameOverPanel, false);
        
        Time.timeScale = 1f;
        
        // Reset game state - reload scene để reset hoàn toàn
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void BackToMainMenu()
    {
        // Hiển thị Main Menu và ẩn các menu khác
        ShowMainMenu();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // ===== HELPER METHODS =====
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            // QUAN TRỌNG: SetActive(false) sẽ ẩn hoàn toàn panel và tất cả children
            panel.SetActive(active);
            
            // Also control CanvasGroup for proper raycasting
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = active;
                cg.interactable = active;
                cg.alpha = active ? 1f : 0f; // Đảm bảo alpha = 0 khi inactive
            }
            
            // Điều chỉnh Image component
            UnityEngine.UI.Image img = panel.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                if (active)
                {
                    img.enabled = true; // Bật Image khi active
                    Color color = img.color;
                    // Điều chỉnh alpha về 0.3 để menu không chói nhưng vẫn thấy vạch nốt
                    if (color.a < 0.2f || color.a > 0.5f)
                    {
                        color.a = 0.3f; // Vừa phải - không chói, vẫn thấy vạch nốt
                        img.color = color;
                    }
                }
                else
                {
                    img.enabled = false; // Tắt Image component khi panel inactive
                }
            }
        }
    }
}

// Class để lưu thông tin bài hát
[System.Serializable]
public class SongData
{
    public string songName;
    public AudioClip audioClip;
    public float bpm = 120f;
    public Sprite coverImage; // Ảnh bìa bài hát (optional)
}
