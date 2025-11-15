using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class MenuUICreator : EditorWindow
{
    [MenuItem("Tools/Create Menu UI")]
    public static void ShowWindow()
    {
        GetWindow<MenuUICreator>("Menu UI Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Menu UI Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Complete Menu System", GUILayout.Height(40)))
        {
            CreateCompleteMenuUI();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("This will create:\n- Main Menu Panel\n- Pause Menu Panel\n- Song Selection Panel\n- Game Over Panel\n- EventSystem (for UI interaction)\n\nMake sure you have a Canvas in your scene!", MessageType.Info);
    }

    private static void CreateCompleteMenuUI()
    {
        // Find existing Canvas (DON'T delete it - it has game UI!)
        Canvas gameCanvas = FindFirstObjectByType<Canvas>();
        if (gameCanvas == null)
        {
            Debug.LogError("❌ No Canvas found! Please create a Canvas first with your game UI (ScoreText, ComboText, etc.)");
            return;
        }
        
        // TẠO CANVAS RIÊNG CHO MENU với sortingOrder cao hơn để menu luôn trên cùng
        Canvas menuCanvas = gameCanvas.transform.Find("MenuCanvas")?.GetComponent<Canvas>();
        if (menuCanvas == null)
        {
            // Tìm trong scene
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas c in allCanvases)
            {
                if (c.name == "MenuCanvas")
                {
                    menuCanvas = c;
                    break;
                }
            }
        }
        
        if (menuCanvas == null)
        {
            GameObject menuCanvasObj = new GameObject("MenuCanvas");
            menuCanvas = menuCanvasObj.AddComponent<Canvas>();
            Debug.Log("✅ Created separate MenuCanvas for menu UI");
        }
        
        // Configure Menu Canvas - Screen Space Camera với sortingOrder cao
        menuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        menuCanvas.overrideSorting = true;
        menuCanvas.sortingOrder = 2000; // Cao hơn game Canvas để menu luôn trên cùng
        
        if (Camera.main != null)
        {
            menuCanvas.worldCamera = Camera.main;
            menuCanvas.planeDistance = 10f; // Gần camera hơn game Canvas
        }
        
        CanvasScaler menuScaler = menuCanvas.GetComponent<CanvasScaler>();
        if (menuScaler == null)
        {
            menuScaler = menuCanvas.gameObject.AddComponent<CanvasScaler>();
        }
        menuScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        menuScaler.referenceResolution = new Vector2(1920, 1080);
        menuScaler.matchWidthOrHeight = 0.5f;
        
        if (menuCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            menuCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Giữ game Canvas settings (không thay đổi)
        Debug.Log($"✅ Game Canvas: sortingOrder = {gameCanvas.sortingOrder}");
        Debug.Log($"✅ Menu Canvas: renderMode = Overlay, sortingOrder = {menuCanvas.sortingOrder} (always on top)");
        
        // Sử dụng menuCanvas cho menu panels
        Canvas canvas = menuCanvas;
        
        // Create EventSystem if it doesn't exist (required for UI interaction!)
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✅ EventSystem created for UI interaction!");
        }

        // Delete old MenuManager if exists
        MenuManager oldMenuManager = FindFirstObjectByType<MenuManager>();
        if (oldMenuManager != null)
        {
            Debug.Log("🗑️ Removing old MenuManager...");
            DestroyImmediate(oldMenuManager.gameObject);
        }
        
        // Delete old menu panels if they exist (tìm và xóa TẤT CẢ, kể cả duplicate)
        string[] oldPanels = { "MainMenuPanel", "PauseMenuPanel", "SongSelectionPanel", "GameOverPanel" };
        foreach (string panelName in oldPanels)
        {
            // Tìm tất cả panel có cùng tên (kể cả duplicate)
            List<GameObject> panelsToDelete = new List<GameObject>();
            
            // Tìm trong toàn bộ scene
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == panelName && obj.transform.IsChildOf(canvas.transform))
                {
                    panelsToDelete.Add(obj);
                }
            }
            
            // Xóa tất cả panel tìm được
            foreach (GameObject panel in panelsToDelete)
            {
                Debug.Log($"🗑️ Removing old {panelName}...");
                DestroyImmediate(panel);
            }
            
            if (panelsToDelete.Count > 1)
            {
                Debug.LogWarning($"⚠️ Found {panelsToDelete.Count} duplicate {panelName} panels - all removed!");
            }
        }

        // Create MenuManager GameObject
        GameObject menuManagerObj = new GameObject("MenuManager");
        MenuManager menuManager = menuManagerObj.AddComponent<MenuManager>();

        // Create Main Menu Panel - Alpha vừa phải để vẫn thấy các vạch nốt phía sau
        GameObject mainMenuPanel = CreatePanel(canvas.transform, "MainMenuPanel", new Color(0.1f, 0.1f, 0.15f, 0.3f));
        menuManager.mainMenuPanel = mainMenuPanel;

        CreateTitle(mainMenuPanel.transform, "Piano Tiles 3D", 60, new Vector2(0, 150));
        GameObject playBtn = CreateButton(mainMenuPanel.transform, "PlayButton", "PLAY", new Vector2(0, 0), new Vector2(300, 80));
        GameObject quitBtn = CreateButton(mainMenuPanel.transform, "QuitButton", "QUIT", new Vector2(0, -100), new Vector2(300, 80));

        // Create Pause Menu Panel - Alpha vừa phải để vẫn thấy các vạch nốt phía sau
        GameObject pauseMenuPanel = CreatePanel(canvas.transform, "PauseMenuPanel", new Color(0, 0, 0, 0.3f));
        menuManager.pauseMenuPanel = pauseMenuPanel;
        pauseMenuPanel.SetActive(false);

        CreateTitle(pauseMenuPanel.transform, "PAUSED", 50, new Vector2(0, 150));
        GameObject resumeBtn = CreateButton(pauseMenuPanel.transform, "ResumeButton", "RESUME", new Vector2(0, 50), new Vector2(300, 80));
        GameObject restartBtn = CreateButton(pauseMenuPanel.transform, "RestartButton", "RESTART", new Vector2(0, -50), new Vector2(300, 80));
        GameObject mainMenuBtn1 = CreateButton(pauseMenuPanel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -150), new Vector2(300, 80));

        // Create Song Selection Panel - Alpha vừa phải để vẫn thấy các vạch nốt phía sau
        GameObject songSelectionPanel = CreatePanel(canvas.transform, "SongSelectionPanel", new Color(0.15f, 0.1f, 0.2f, 0.3f));
        menuManager.songSelectionPanel = songSelectionPanel;
        songSelectionPanel.SetActive(false);

        CreateTitle(songSelectionPanel.transform, "SELECT SONG", 50, new Vector2(0, 250));
        
        // Create Scroll View for songs
        GameObject scrollView = CreateScrollView(songSelectionPanel.transform, "SongScrollView", new Vector2(0, 0), new Vector2(800, 400));
        menuManager.songListContent = scrollView.transform.Find("Viewport/Content").GetComponent<RectTransform>();

        GameObject backBtn = CreateButton(songSelectionPanel.transform, "BackButton", "BACK", new Vector2(0, -280), new Vector2(250, 70));

        // Create Game Over Panel - Alpha vừa phải để vẫn thấy các vạch nốt phía sau
        GameObject gameOverPanel = CreatePanel(canvas.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.3f));
        menuManager.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        CreateTitle(gameOverPanel.transform, "GAME OVER", 50, new Vector2(0, 200));
        
        GameObject scoreLabel = CreateText(gameOverPanel.transform, "ScoreLabel", "SCORE:", 30, new Vector2(0, 100));
        GameObject finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText", "0", 60, new Vector2(0, 50));
        menuManager.finalScoreText = finalScoreText.GetComponent<TextMeshProUGUI>();
        
        GameObject highScoreLabel = CreateText(gameOverPanel.transform, "HighScoreLabel", "HIGH SCORE:", 25, new Vector2(0, -30));
        GameObject highScoreText = CreateText(gameOverPanel.transform, "HighScoreText", "0", 40, new Vector2(0, -70));
        menuManager.highScoreText = highScoreText.GetComponent<TextMeshProUGUI>();

        GameObject restartBtn2 = CreateButton(gameOverPanel.transform, "RestartButton2", "PLAY AGAIN", new Vector2(0, -170), new Vector2(300, 80));
        GameObject mainMenuBtn2 = CreateButton(gameOverPanel.transform, "MainMenuButton2", "MAIN MENU", new Vector2(0, -270), new Vector2(300, 80));

        // Wire up button events
        playBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowSongSelection());
        quitBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.QuitGame());
        resumeBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ResumeGame());
        restartBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.RestartGame());
        mainMenuBtn1.GetComponent<Button>().onClick.AddListener(() => menuManager.BackToMainMenu());
        backBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowMainMenu());
        restartBtn2.GetComponent<Button>().onClick.AddListener(() => menuManager.RestartGame());
        mainMenuBtn2.GetComponent<Button>().onClick.AddListener(() => menuManager.BackToMainMenu());

        // Add CanvasGroup to each panel for proper interaction
        AddCanvasGroup(mainMenuPanel, true);
        AddCanvasGroup(pauseMenuPanel, false);
        AddCanvasGroup(songSelectionPanel, false);
        AddCanvasGroup(gameOverPanel, false);

        Debug.Log("✅ Complete Menu UI created successfully!");
        Debug.Log("� Your existing game UI (ScoreText, ComboText, etc.) is preserved!");
        Debug.Log("�📝 Next steps:");
        Debug.Log("1. Select MenuManager in Hierarchy");
        Debug.Log("2. In Inspector, configure 'Available Songs' array");
        Debug.Log("3. Assign MenuManager to GameController's 'Menu Manager' field");
        Debug.Log("4. Press PLAY and test the menu!");
        
        EditorUtility.SetDirty(menuManager);
        Selection.activeGameObject = menuManagerObj;
    }

    private static void AddCanvasGroup(GameObject panel, bool isActive)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
        cg.interactable = true;
        cg.blocksRaycasts = isActive; // Only active panel blocks raycasts
        cg.alpha = 1f;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = color;
        
        return panel;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);
        
        RectTransform rect = button.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = button.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        Button btn = button.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.9f, 1f);
        colors.selectedColor = new Color(0.25f, 0.65f, 1f, 1f);
        btn.colors = colors;
        
        // Add Navigation (to prevent errors)
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        
        // Try to use default TMP font
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont == null)
        {
            // Try alternative path
            defaultFont = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        }
        if (defaultFont != null)
        {
            tmp.font = defaultFont;
        }
        
        return button;
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(800, 100);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        
        // Try to use default TMP font
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont == null)
        {
            defaultFont = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        }
        if (defaultFont != null)
        {
            tmp.font = defaultFont;
        }
        
        return textObj;
    }

    private static GameObject CreateTitle(Transform parent, string name, float fontSize, Vector2 position)
    {
        GameObject title = CreateText(parent, name, name, fontSize, position);
        TextMeshProUGUI tmp = title.GetComponent<TextMeshProUGUI>();
        tmp.color = new Color(1f, 0.9f, 0.3f, 1f); // Gold color
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        
        // Add outline only if material supports it
        if (tmp.fontMaterial != null)
        {
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color(0, 0, 0, 0.5f);
        }
        
        return title;
    }

    private static GameObject CreateScrollView(Transform parent, string name, Vector2 position, Vector2 size)
    {
        // Create Scroll View container
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent, false);
        
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchoredPosition = position;
        scrollRect.sizeDelta = size;
        
        Image scrollImage = scrollView.AddComponent<Image>();
        scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        
        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        // Create Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 600);
        contentRect.anchoredPosition = Vector2.zero;
        
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Wire up ScrollRect
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 20;
        
        return scrollView;
    }
}
