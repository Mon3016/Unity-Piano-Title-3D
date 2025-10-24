using UnityEngine;
using System.Collections; 
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{
    // Tham chiếu (GÁN TRONG INSPECTOR)
    public GameObject tilePrefab; 
    public float[] columnXPositions; // Vị trí X của các làn đường
    public float spawnZPosition = 15f; 
    
    // Tham số BPM Sync
    public float spawnTimeOffset = 0.1f; // Độ trễ ban đầu để căn nhạc

    private GameController gameController;
    private float spawnTimeInterval; // Thời gian sinh ô mới (Tính từ BPM)

    void Start()
    {
        gameController = FindFirstObjectByType<GameController>(); 
        
        if (gameController == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy GameController.");
            return;
        }

        // Tính toán khoảng thời gian sinh ô từ BPM của GameController
        spawnTimeInterval = (60f / gameController.musicBPM); 

        // Bắt đầu quy trình sinh ô
        StartCoroutine(SpawnTilesRoutine());
    }

    IEnumerator SpawnTilesRoutine()
    {
        // Độ trễ ban đầu để đồng bộ hóa với thời điểm bắt đầu của nhạc
        yield return new WaitForSeconds(spawnTimeOffset); 
        
        while (true) 
        {
            SpawnNewRow(); 
            
            // Dùng thời gian tính toán từ BPM
            yield return new WaitForSeconds(spawnTimeInterval); 
        }
    }
    
    private void SpawnNewRow()
    {
        // CHỈ SINH RA MỘT Ô GẠCH DUY NHẤT (Ô ĐEN) TRONG HÀNG

        // 1. Chọn ngẫu nhiên vị trí X của ô đen (từ 0 đến 3)
        int blackTileIndex = Random.Range(0, columnXPositions.Length);
        float blackTileX = columnXPositions[blackTileIndex];

        // 2. Thiết lập vị trí sinh
        Vector3 spawnPosition = new Vector3(blackTileX, 0.05f, spawnZPosition);
        
        // 3. Sinh ô
        GameObject newTileObj = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
        Tile tileScript = newTileObj.GetComponent<Tile>(); 

        // Đặt ô này là ô đen (TileType = 1)
        tileScript.SetTileType(1); 
        
        // Gán Tốc độ CỐ ĐỊNH từ GameController
        tileScript.speed = gameController.currentSpeed; 
    }
}