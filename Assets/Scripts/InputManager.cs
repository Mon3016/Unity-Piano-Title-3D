using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Time.timeScale > 0 && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Gọi hàm OnHit() của Tile khi chạm vào nó
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.OnHit(); 
                }
            }
        }
    }
}