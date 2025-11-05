using UnityEngine;
using System.Collections.Generic;

public class TileSpawner : MonoBehaviour
{
    public GameObject tilePrefab;
    public List<float> columnXPositions = new List<float>(); 
    
    private float spawnZPosition; 
    public float spawnTimeOffset = 0.1f;

    private GameController gameController;
    private float nextSpawnTime;

    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
        
        if (gameController != null)
        {
            spawnZPosition = gameController.spawnZPosition; 
            nextSpawnTime = Time.time + spawnTimeOffset;
        }
    }

    void Update()
    {
        if (gameController == null || Time.timeScale == 0f) return;

        float spawnInterval = 60f / gameController.musicBPM;

        if (Time.time >= nextSpawnTime)
        {
            SpawnRow();
            nextSpawnTime += spawnInterval;
        }
    }

    void SpawnRow()
    {
        if (columnXPositions.Count == 0) return;

        // 1. Chọn ngẫu nhiên vị trí X của ô đen (1 trong 4 làn)
        int blackXIndex = Random.Range(0, columnXPositions.Count);
        
        // Lấy vị trí X của ô đen
        Vector3 spawnPos = new Vector3(columnXPositions[blackXIndex], 0f, spawnZPosition);
        
        // Sinh ra ô đen duy nhất
        GameObject tileObj = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
        Tile tileScript = tileObj.GetComponent<Tile>();
        
        if (tileScript != null)
        {
            // Set là ô Đen
            tileScript.SetTileType(1); 
        }

        // KHÔNG SINH RA CÁC Ô TRẮNG HOẶC Ô KHÁC
    }
}