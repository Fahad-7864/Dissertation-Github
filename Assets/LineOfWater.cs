using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LineOfWater : MonoBehaviour
{
    public Tilemap lineOfWaterTilemap;
    public Tile lineOfWaterTile;
    public Tile cursorTile;
    public GameObject character;
    public CursorController cursorController;
    public Astar pathfinding;

    public Animator waterAnimator; 
    public int spellRange = 4;

    private Vector3Int lastHoveredTile;
    private Vector3Int attackTarget = Vector3Int.one * -1;

    private bool confirmingAttack = false;
    private float animationTime = 2f; 

    public Attack attack;
    public ElementType elementType;

    private Dictionary<Vector3Int, TileBase> originalTiles;  

    void Start()
    {
        originalTiles = new Dictionary<Vector3Int, TileBase>();
    }
    void Update()
    {
        UpdateLine();
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int clickedTilePos = lineOfWaterTilemap.WorldToCell(cursorController.GetCursorWorldPosition());

            if (Array.IndexOf(pathfinding.GetNeighbors(lineOfWaterTilemap.WorldToCell(character.transform.position)), clickedTilePos) > -1)
            {
                if (!confirmingAttack)
                {
                    confirmingAttack = true;
                    attackTarget = clickedTilePos;
                }
                else if (clickedTilePos == attackTarget)
                {
                    StartCoroutine(Attack(clickedTilePos));
                    confirmingAttack = false;
                    attackTarget = Vector3Int.one * -1;
                }
                else
                {
                    attackTarget = clickedTilePos;
                }
            }
        }
    }

    void UpdateLine()
    {
        Vector3Int hoveredTilePos = lineOfWaterTilemap.WorldToCell(cursorController.GetCursorWorldPosition());

        // If the cursor has moved to a new tile, restore the original tile at the last hovered position
        if (hoveredTilePos != lastHoveredTile && originalTiles.ContainsKey(lastHoveredTile))
        {
            lineOfWaterTilemap.SetTile(lastHoveredTile, originalTiles[lastHoveredTile]);
            originalTiles.Remove(lastHoveredTile);
        }

        lastHoveredTile = hoveredTilePos;

        lineOfWaterTilemap.ClearAllTiles();
        Vector3Int startTilePos = lineOfWaterTilemap.WorldToCell(character.transform.position);

        Vector3Int[] neighboringTiles = pathfinding.GetNeighbors(startTilePos);

        foreach (var tilePos in neighboringTiles)
        {
            if (pathfinding.IsWalkable(tilePos))
            {
                lineOfWaterTilemap.SetTile(tilePos, cursorTile);
            }
        }

        if (Array.IndexOf(neighboringTiles, hoveredTilePos) > -1)
        {
            // Save the original tile at the current hovered position
            originalTiles[hoveredTilePos] = lineOfWaterTilemap.GetTile(hoveredTilePos);

            Vector3 directionFloat = ((Vector3)hoveredTilePos - (Vector3)startTilePos).normalized;
            Vector3Int direction = new Vector3Int(Mathf.RoundToInt(directionFloat.x), Mathf.RoundToInt(directionFloat.y), 0);

            for (int i = 1; i <= spellRange; i++)
            {
                Vector3Int tilePos = hoveredTilePos + new Vector3Int(direction.x * i, direction.y * i, 0);

                if (pathfinding.IsWalkable(tilePos))
                {
                    // Save the original tile at the current position
                    originalTiles[tilePos] = lineOfWaterTilemap.GetTile(tilePos);

                    lineOfWaterTilemap.SetTile(tilePos, lineOfWaterTile);
                }
            }
        }
    }

    protected virtual IEnumerator Attack(Vector3Int targetTilePos)
    {
        Vector3Int startTilePos = lineOfWaterTilemap.WorldToCell(character.transform.position);
        Vector3 directionFloat = ((Vector3)targetTilePos - (Vector3)startTilePos).normalized;
        Vector3Int direction = new Vector3Int(Mathf.RoundToInt(directionFloat.x), Mathf.RoundToInt(directionFloat.y), 0);

        // Calculate the delay based on the spell range and the total animation time
        float delay = animationTime / spellRange;

        for (int i = 1; i <= spellRange; i++)
        {
            Vector3Int tilePos = startTilePos + direction * i;
            Vector3 spawnPosition = lineOfWaterTilemap.GetCellCenterWorld(tilePos);

            if (pathfinding.IsWalkable(tilePos))
            {
                RaycastHit2D hit = Physics2D.Raycast(lineOfWaterTilemap.GetCellCenterWorld(tilePos), Vector2.zero);
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.CompareTag("Enemy"))
                    {
                        attack.elementType = elementType;
                        bool attackSuccessful = attack.AttackEnemy(hitObject.name);
                    }
                }
            }

            // Play the water animation at each tile along the path of the ray
            StartCoroutine(PlayWaterAnimationInSequence(startTilePos, direction, i, delay));

            // Wait for the delay before moving to the next tile
            yield return new WaitForSeconds(delay);
        }

        // Reset the position of the animator after reaching max range
        waterAnimator.transform.position = character.transform.position;
        waterAnimator.ResetTrigger("Play");
    }


    protected virtual IEnumerator PlayWaterAnimationInSequence(Vector3Int startTilePos, Vector3Int direction, int tillIndex, float delay)
    {
        for (int i = 1; i <= tillIndex; i++)
        {
            Vector3Int tilePos = startTilePos + direction * i;
            Vector3 spawnPosition = lineOfWaterTilemap.GetCellCenterWorld(tilePos);

            // Move the animator to this tile and play the animation
            waterAnimator.transform.position = spawnPosition;
            waterAnimator.SetTrigger("Play");

            // Wait for the delay before moving to the next tile
            yield return new WaitForSeconds(delay);
        }
    }

}
