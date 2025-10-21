using UnityEngine;
using System.Collections; 

public class TileSpawner : MonoBehaviour
{
    // CÁC BIẾN SẼ ĐƯỢC GÁN TRONG INSPECTOR
    public GameObject tilePrefab; 
    public float[] columnXPositions; 
    public float spawnZPosition = 15f; 
    public float spawnYPosition = 0f; 
    public float spawnInterval = 0.8f; 
    public Material blackMaterial;
    public Material whiteMaterial;
    
    private GameController gameController;

    void Start()
    {
        gameController = FindFirstObjectByType<GameController>(); 
        
        if (gameController == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy GameController. Đảm bảo bạn đã tạo đối tượng và gắn script GameController.cs.");
            return;
        }

        StartCoroutine(SpawnTilesRoutine());
    }

    IEnumerator SpawnTilesRoutine()
    {
        while (true) 
        {
            SpawnNewRow(); 
            // Sử dụng tốc độ hiện tại của GameController để tính toán interval
            float currentInterval = Mathf.Max(0.1f, spawnInterval - (gameController.score * 0.01f));
            yield return new WaitForSeconds(currentInterval); 
        }
    }
    
    private void SpawnNewRow()
    {
        // 1. CHỈ TÍNH VỊ TRÍ CHO Ô ĐEN
        int blackTileIndex = Random.Range(0, columnXPositions.Length); 
        
        // 2. TÍNH TOÁN VỊ TRÍ ĐẦU RA
        Vector3 spawnPosition = new Vector3(
            columnXPositions[blackTileIndex], 
            spawnYPosition,      
            spawnZPosition      
        );

        // 3. TẠO Ô DUY NHẤT
        GameObject newTileObj = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
        
        Tile tileScript = newTileObj.GetComponent<Tile>(); 
        MeshRenderer meshRenderer = newTileObj.GetComponent<MeshRenderer>(); 
        
        if (tileScript == null || meshRenderer == null) return; 

        // 4. THIẾT LẬP LÀ Ô ĐEN (luôn là ô đen)
        tileScript.isBlackTile = true;
        meshRenderer.material = blackMaterial;

        // Gán Tốc độ
        tileScript.speed = gameController.currentSpeed; 
    }
} // <--- DẤU NGOẶC NHỌN ĐÓNG BỊ THIẾU