using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


/*
    The CircleSpellTrial class represents the base functionality for circle-spell 
    casting in the game. This class provides the methods for visualizing the spell's 
    range, AoE (Area of Effect), and handling the logic of casting the spell on 
    target tiles and enemies.
*/
public class CircleSpellTrial : MonoBehaviour
{
    // ----- Section: Variables & References -----
    // Tilemaps and Tiles for visualizing spell range and AoE
    public Tilemap spellTilemap;  
    public Tile spellRangeTile; 
    public Tile spellAoETile;  
    public Tile TileForEnemyFound; 
    public Tile bestSpellTile;
    public Tilemap spellAoETilemap;


    // References to game objects and scripts
    public GameObject character;  
    public CursorController cursorController;  
    public Astar pathfinding;
    public Attack attack;


    // Variables for spell range and AoE radius
    public int spellRange;  
    public int radius;  

    // Variables to handle the state of the spell casting
    private Vector3Int lastCenterTile = Vector3Int.one * -1;  
    public bool isCastingSpell = false; 


 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCastingSpell = !isCastingSpell;
            if (isCastingSpell)
            {
                ShowSpellRange();
            }
            else
            {
                spellTilemap.ClearAllTiles();
            }
        }

        if (isCastingSpell)
        {
            Vector3Int hoveredTilePos = spellTilemap.WorldToCell(cursorController.GetCursorWorldPosition());

            // Check if hovered tile is within spell range
            if (spellTilemap.HasTile(hoveredTilePos))
            {
                ShowSpellAoE(hoveredTilePos);
            }
            else
            {
                ClearSpellAoE(); 
            }
            if (Input.GetMouseButtonDown(0) && spellTilemap.GetTile(hoveredTilePos) == spellRangeTile)
            {
                PerformAttack(hoveredTilePos);
                isCastingSpell = false;
                spellTilemap.ClearAllTiles();
                ClearSpellAoE();  

            }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {

            Dictionary<string, List<Vector3Int>> enemyTiles = FindEnemiesOnTiles();

            // Clear any previous enemy visualization
            spellTilemap.ClearAllTiles();
            spellAoETilemap.ClearAllTiles();

            // Visualize enemies found on spellTilemap
            foreach (Vector3Int tile in enemyTiles["spellTilemap"])
            {
                spellTilemap.SetTile(tile, TileForEnemyFound);
            }

            // Visualize enemies found within AoE of spellTilemap
            foreach (Vector3Int tile in enemyTiles["spellAoETilemap"])
            {
                spellAoETilemap.SetTile(tile, TileForEnemyFound);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            AutoCastSpell(FindEnemiesOnTiles());
        }

    }

    // Determines the best casting position automatically based on enemies' positions
    public virtual void AutoCastSpell(Dictionary<string, List<Vector3Int>> enemyTiles)
    {
        Vector3Int bestSpellPosition = Vector3Int.one * -1;  
        int maxEnemiesHit = 0;  

        // Go through each tile in the spellTilemap and simulate casting the spell there
        foreach (var pos in spellTilemap.cellBounds.allPositionsWithin)
        {
            if (spellTilemap.GetTile(pos) == null)
                continue;

            // Generate AoE based on the position
            List<Vector3Int> aoeTiles = ShowSpellAoE(pos);

            // Check for enemies on each tile in the generated AoE
            int enemiesHit = 0;
            foreach (var aoePos in aoeTiles)
            {// Check if AoE hits an enemy
                if (enemyTiles["spellAoETilemap"].Contains(aoePos))  
                {
                    enemiesHit++;
                }
            }

            // If this position hits more enemies, update bestSpellPosition and maxEnemiesHit
            if (enemiesHit > maxEnemiesHit)
            {
                bestSpellPosition = pos;
                maxEnemiesHit = enemiesHit;
            }
        }

        // If we found a best position, cast the spell
        if (bestSpellPosition != Vector3Int.one * -1)
        {
            // Clear previous tiles
            spellTilemap.ClearAllTiles();
            ClearSpellAoE();  // Clear AoE Tiles
            // Highlight the best spell position
            spellTilemap.SetTile(bestSpellPosition, bestSpellTile);

           // PerformAttack(bestSpellPosition);
            isCastingSpell = false;
        
        }
    }



    // Identifies the positions of enemies on both the spell and AoE tilemaps
    public Dictionary<string, List<Vector3Int>> FindEnemiesOnTiles()
    {
        Dictionary<string, List<Vector3Int>> enemyTiles = new Dictionary<string, List<Vector3Int>>
    {
        { "spellTilemap", new List<Vector3Int>() },
        { "spellAoETilemap", new List<Vector3Int>() }
    };

        // Check all tiles in spellTilemap
        foreach (var pos in spellTilemap.cellBounds.allPositionsWithin)
        {
            if (spellTilemap.GetTile(pos) == null)
                continue;

            // Check for enemies directly on the tile
            RaycastHit2D hitDirect = Physics2D.Raycast(spellTilemap.GetCellCenterWorld(pos), Vector2.zero);
            if (hitDirect.collider != null)
            {
                GameObject hitObject = hitDirect.collider.gameObject;
                CharacterStats characterStats = hitObject.GetComponent<CharacterStats>();
                if (characterStats != null && characterStats.type == CharacterType.Enemy)
                {
                    Vector3Int enemyTilePos = spellTilemap.WorldToCell(hitObject.transform.position);
                    Debug.Log("Enemy " + characterStats.characterName + " is on tile position: " + enemyTilePos);
                    enemyTiles["spellTilemap"].Add(pos);
                }
            }

            // Generate AoE based on the position of the character
            List<Vector3Int> aoeTiles = ShowSpellAoE(pos);

            // Check for enemies on each tile in the generated AoE
            foreach (var aoePos in aoeTiles)
            {
                RaycastHit2D hitAoE = Physics2D.Raycast(spellAoETilemap.GetCellCenterWorld(aoePos), Vector2.zero);
                if (hitAoE.collider != null)
                {
                    GameObject hitObject = hitAoE.collider.gameObject;
                    CharacterStats characterStats = hitObject.GetComponent<CharacterStats>();
                    if (characterStats != null && characterStats.type == CharacterType.Enemy)
                    {
                        Vector3Int enemyTilePos = spellAoETilemap.WorldToCell(hitObject.transform.position);
                        Debug.Log("Enemy " + characterStats.characterName + " is within AoE centered at position: " + pos);
                        enemyTiles["spellAoETilemap"].Add(aoePos);
                    }
                }
            }
        }

        return enemyTiles;
    }




    // Visualizes the tiles within the range where the spell can be cast
    public void ShowSpellRange()
    {
        spellTilemap.ClearAllTiles();
        Vector3Int startTilePos = spellTilemap.WorldToCell(character.transform.position);
        for (int x = -spellRange; x <= spellRange; x++)
        {
            for (int y = -spellRange; y <= spellRange; y++)
            {
                Vector3Int tilePos = startTilePos + new Vector3Int(x, y, 0);
                if (Vector3Int.Distance(tilePos, startTilePos) <= spellRange && pathfinding.IsWalkable(tilePos))
                {
                    spellTilemap.SetTile(tilePos, spellRangeTile);
                }
            }
        }
    }


    // Visualizes the Area of Effect (AoE) of the spell based on a center tile
    public List<Vector3Int> ShowSpellAoE(Vector3Int center)
    {
        Vector3Int startTilePos = spellTilemap.WorldToCell(character.transform.position);

        // Clear previous AoE tiles
        if (lastCenterTile != Vector3Int.one * -1)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int tilePos = lastCenterTile + new Vector3Int(x, y, 0);
                    if (spellAoETilemap.GetTile(tilePos) == spellAoETile)
                    {
                        spellAoETilemap.SetTile(tilePos, null);
                    }
                }
            }
        }

        List<Vector3Int> aoeTiles = new List<Vector3Int>(); // List to store AoE tiles

        // Set new AoE tiles
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int tilePos = center + new Vector3Int(x, y, 0);
                if (Vector3Int.Distance(tilePos, center) <= radius && pathfinding.HasLineOfSight(startTilePos, tilePos))
                {
                    spellAoETilemap.SetTile(tilePos, spellAoETile);
                    // Check if AoE hits an enemy
                    aoeTiles.Add(tilePos); 
                }
            }
        }

        lastCenterTile = center;  // UPDATE: storing last AoE center position

        return aoeTiles;  // Return the list of AoE tiles
    }


    // Executes the spell attack on the specified tiles within the AoE
    public virtual void PerformAttack(Vector3Int center)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int tilePos = center + new Vector3Int(x, y, 0);

                if (Vector3Int.Distance(tilePos, center) <= radius)
                {
                    RaycastHit2D hit = Physics2D.Raycast(spellTilemap.GetCellCenterWorld(tilePos), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            attack.AttackEnemy(hitObject.name);
                        }
                    }
                }
            }
        }
    }

    // Clears the tiles showing the Area of Effect (AoE) of the spell
    public void ClearSpellAoE()
    {
        if (lastCenterTile != Vector3Int.one * -1)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int tilePos = lastCenterTile + new Vector3Int(x, y, 0);
                    if (spellAoETilemap.GetTile(tilePos) == spellAoETile)
                    {
                        spellAoETilemap.SetTile(tilePos, null);
                    }
                }
            }
        }
    }



}
