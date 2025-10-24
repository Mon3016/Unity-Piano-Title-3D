using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GameController gameController;

    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
    }

    void Update()
    {
        // Xử lý chạm/Click chuột
        if (Input.GetMouseButtonDown(0)) 
        {
            HandleTap(Input.mousePosition);
        }
    }

    void HandleTap(Vector3 screenPosition)
    {
        if (gameController == null || Time.timeScale == 0f) return;

        // Tạo tia Ray từ vị trí chạm
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // Kiểm tra Ray có chạm vào collider nào không
        if (Physics.Raycast(ray, out hit))
        {
            // Lấy component Tile từ đối tượng bị chạm
            Tile tile = hit.collider.GetComponent<Tile>();

            if (tile != null)
            {
                // Gọi hàm xử lý va chạm của Tile
                tile.OnMouseDown(); 
            }
        }
    }
}