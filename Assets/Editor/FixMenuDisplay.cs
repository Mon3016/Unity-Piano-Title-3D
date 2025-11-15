using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixMenuDisplay
{
    [MenuItem("Tools/Fix Menu Display")]
    public static void FixMenu()
    {
        Debug.Log("🔍 Checking menu panels...");
        
        // 1. Check Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found!");
            return;
        }
        
        Debug.Log($"✅ Canvas found: {canvas.name}");
        Debug.Log($"   - Render Mode: {canvas.renderMode}");
        Debug.Log($"   - Sorting Order: {canvas.sortingOrder}");
        Debug.Log($"   - Plane Distance: {canvas.planeDistance}");
        
        // 2. Check and fix menu panels
        string[] panelNames = { "MainMenuPanel", "PauseMenuPanel", "SongSelectionPanel", "GameOverPanel" };
        bool foundAny = false;
        
        foreach (string panelName in panelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel == null)
            {
                // Try to find in Canvas children
                Transform canvasTransform = canvas.transform;
                Transform panelTransform = canvasTransform.Find(panelName);
                if (panelTransform != null)
                {
                    panel = panelTransform.gameObject;
                }
            }
            
            if (panel != null)
            {
                foundAny = true;
                Debug.Log($"\n📋 Found panel: {panelName}");
                
                // Check active state
                bool isActive = panel.activeSelf;
                Debug.Log($"   - Active: {isActive}");
                
                // Check Image component
                Image image = panel.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    Debug.Log($"   - Image Color: R={color.r:F2}, G={color.g:F2}, B={color.b:F2}, A={color.a:F2}");
                    
                    // Fix alpha if too low
                    if (color.a < 0.3f)
                    {
                        color.a = 0.4f;
                        image.color = color;
                        Debug.Log($"   ✅ Fixed alpha to 0.4");
                        EditorUtility.SetDirty(panel);
                    }
                }
                else
                {
                    Debug.LogWarning($"   ⚠️ No Image component found!");
                }
                
                // Check CanvasGroup
                CanvasGroup cg = panel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    Debug.Log($"   - CanvasGroup Alpha: {cg.alpha}");
                    Debug.Log($"   - CanvasGroup Interactable: {cg.interactable}");
                    Debug.Log($"   - CanvasGroup BlocksRaycasts: {cg.blocksRaycasts}");
                    
                    // Fix CanvasGroup
                    if (panelName == "MainMenuPanel" && isActive)
                    {
                        cg.alpha = 1f;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                        Debug.Log($"   ✅ Fixed CanvasGroup for MainMenuPanel");
                        EditorUtility.SetDirty(panel);
                    }
                }
                
                // Ensure MainMenuPanel is active
                if (panelName == "MainMenuPanel" && !isActive)
                {
                    panel.SetActive(true);
                    Debug.Log($"   ✅ Activated MainMenuPanel");
                    EditorUtility.SetDirty(panel);
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Panel not found: {panelName}");
            }
        }
        
        if (!foundAny)
        {
            Debug.LogError("❌ No menu panels found! You may need to create them using 'Tools/Create Complete Menu UI'");
            return;
        }
        
        // 3. Check MenuManager
        MenuManager menuManager = Object.FindFirstObjectByType<MenuManager>();
        if (menuManager != null)
        {
            Debug.Log($"\n✅ MenuManager found");
            Debug.Log($"   - MainMenuPanel assigned: {menuManager.mainMenuPanel != null}");
            Debug.Log($"   - PauseMenuPanel assigned: {menuManager.pauseMenuPanel != null}");
            Debug.Log($"   - SongSelectionPanel assigned: {menuManager.songSelectionPanel != null}");
            Debug.Log($"   - GameOverPanel assigned: {menuManager.gameOverPanel != null}");
            
            // Force show main menu
            if (menuManager.mainMenuPanel != null)
            {
                menuManager.mainMenuPanel.SetActive(true);
                Image img = menuManager.mainMenuPanel.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0.4f;
                    img.color = c;
                }
                CanvasGroup cg = menuManager.mainMenuPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                Debug.Log($"   ✅ Forced MainMenuPanel to be visible");
                EditorUtility.SetDirty(menuManager);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ MenuManager not found!");
        }
        
        // 4. Final check - ensure Canvas sorting order và override sorting
        canvas.overrideSorting = true;
        if (canvas.sortingOrder < 100)
        {
            canvas.sortingOrder = 1000;
            Debug.Log($"✅ Set Canvas overrideSorting = true, sortingOrder = 1000");
            EditorUtility.SetDirty(canvas.gameObject);
        }
        
        // 5. Thử đổi sang Screen Space Overlay nếu Screen Space Camera không hoạt động
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Debug.Log("ℹ️ Current mode: Screen Space Camera");
            Debug.Log("   If menu still behind, try changing to Screen Space Overlay in Inspector");
        }
        
        Debug.Log("\n🎉 Menu display fix complete!");
        Debug.Log("→ Check the Game view to see if menu is visible now");
        Debug.Log("→ If still behind, try: Canvas → Render Mode → Screen Space - Overlay");
    }
}

