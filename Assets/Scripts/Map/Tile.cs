using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Transforms"), Tooltip("The transforms of the tile, obstacle, and player.")]
    public Transform tilePrefab;
    public Transform[] obstaclePrefabs;
    public Transform playerTransform;

    [Header("Tile Settings"), Tooltip("The tile prefab to spawn.")]
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 30.0f;
    public float moveSpeed;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;
    
    private GameManager gameManager;
    
    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }
    
    void Start()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(i >= noObstaclesInitially);
        }
    }

    void Update()
    {
        if (!gameManager.isGameover)
        {
            moveSpeed = gameManager.stageSpeed;
            MoveTiles();
            if (tiles.Count > 0 && tiles[0].position.z < playerTransform.position.z - 50)
            {
                ReuseTile();
            }
        }
        
    }

    private void MoveTiles()
    {
        foreach (var tile in tiles)
        {
            tile.Translate(-Vector3.forward * (moveSpeed * Time.deltaTime), Space.World);
        }
    }

    private void SpawnTile(bool spawnObstacles)
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab is not assigned.");
            return;
        }

        var newTile = Instantiate(tilePrefab, nextTilePosition, Quaternion.identity, transform);
        if (newTile == null)
        {
            Debug.LogWarning("Failed to instantiate tile prefab.");
            return;
        }

        var tileEndPoint = newTile.GetComponent<TileEndPoint>();
        if (tileEndPoint == null)
        {
            Debug.LogWarning("TileEndPoint component is missing on the tile prefab.");
            return;
        }
    
        if (tileEndPoint.endPoint == null)
        {
            Debug.LogWarning("EndPoint is not assigned in TileEndPoint component.");
            return;
        }

        nextTilePosition = tileEndPoint.endPoint.position;
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            SpawnObstacles(newTile);
        }
    }

    private void SpawnObstacles(Transform tile)
    {
        // 모든 자식 스폰 포인트를 리스트에 추가
        var spawnPoints = new List<Transform>();
        foreach (Transform child in tile)
        {
            if (child.CompareTag("ObstacleSpawnPoint"))
            {
                spawnPoints.Add(child);
            }
        }

        // 장애물을 생성하지 않을 포인트를 무작위로 선택
        var emptyIndex = Random.Range(0, spawnPoints.Count);

        // 선택된 포인트를 제외한 나머지 포인트에 장애물 생성
        for (var i = 0; i < spawnPoints.Count; i++)
        {
            if (i != emptyIndex && obstaclePrefabs.Length > 0) // 비워둘 인덱스가 아니면
            {
                var spawnPoint = spawnPoints[i];
                if (Random.Range(0, 2) == 0) // 50% 확률로 장애물 생성
                {
                    var obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                    Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
                }
            }
        }
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        var endPoint = tiles[^1].GetComponent<TileEndPoint>().endPoint.position;
        tile.position = endPoint;
        tiles.Add(tile);
    }
}

