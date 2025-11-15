using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugUIClicks : MonoBehaviour
{
    void Update()
    {
        // Kiểm tra mỗi khi click chuột
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("====== MOUSE CLICKED ======");
            Debug.Log($"Time.timeScale = {Time.timeScale}");
            
            // Kiểm tra EventSystem
            EventSystem es = EventSystem.current;
            if (es == null)
            {
                Debug.LogError("❌ NO EVENTSYSTEM!");
                return;
            }
            Debug.Log($"✅ EventSystem: {es.name}");
            
            // Kiểm tra raycast
            PointerEventData pointerData = new PointerEventData(es);
            pointerData.position = Input.mousePosition;
            
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            Debug.Log($"📊 Raycast hits: {results.Count}");
            
            if (results.Count == 0)
            {
                Debug.LogWarning("⚠️ NO UI ELEMENTS HIT!");
                Debug.LogWarning("→ Check Canvas has GraphicRaycaster");
                Debug.LogWarning("→ Check Canvas Render Camera is set");
            }
            else
            {
                foreach (RaycastResult result in results)
                {
                    Debug.Log($"  → Hit: {result.gameObject.name}");
                    
                    Button btn = result.gameObject.GetComponent<Button>();
                    if (btn != null)
                    {
                        Debug.Log($"    ✅ Is Button! Interactable: {btn.interactable}");
                        
                        // Kiểm tra persistent events (từ Inspector)
                        int persistentCount = btn.onClick.GetPersistentEventCount();
                        
                        // KHÔNG log error nữa vì:
                        // - Button có thể được wire bằng code runtime (không có persistent events)
                        // - MenuManager tự động wire buttons trong Start()
                        // - Các button được tạo động (như Song_0) đã có onClick được set trong code
                        if (persistentCount > 0)
                        {
                            Debug.Log($"    ✅ Button '{btn.name}' has {persistentCount} persistent onClick events");
                        }
                        else
                        {
                            // Chỉ log info, không log error
                            Debug.Log($"    ℹ️ Button '{btn.name}' - No persistent events (likely wired by code at runtime)");
                        }
                    }
                }
            }
        }
    }
}
