using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapData : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject player;
    public GameObject enemy;
    public Color friendlyTileColor = Color.blue;
    public Color enemyTileColor = Color.red;

    private Vector3Int playerTilePosition;
    private Vector3Int enemyTilePosition;
    private Dictionary<Vector3Int, int> scores = new Dictionary<Vector3Int, int>();

    void Start()
    {
        UpdatePlayerAndEnemyPosition();
        CalculateScores();
    }

    void Update()
    {
        UpdatePlayerAndEnemyPosition();
    }

    void UpdatePlayerAndEnemyPosition()
    {
        playerTilePosition = tilemap.WorldToCell(player.transform.position);
        enemyTilePosition = tilemap.WorldToCell(enemy.transform.position);
    }

    void CalculateScores()
    {
        scores.Clear();
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                scores[position] = CalculateTileScore(position, playerTilePosition);
            }
        }
    }

    int CalculateTileScore(Vector3Int targetTilePosition, Vector3Int playerTilePosition)
    {
        // Calculate the Manhattan distance between the player and the tile
        int distance = Mathf.Abs(playerTilePosition.x - targetTilePosition.x) + Mathf.Abs(playerTilePosition.y - targetTilePosition.y);
        return distance;
    }


}
