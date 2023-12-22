using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;


/*
    The Taunt class is responsible for implementing the taunt mechanic for a character in the game. 
    When activated, the character attempts to draw the attention of nearby enemies, forcing them to target 
    the taunting character. This script provides functionalities to visualize the range of the taunt, 
    determine which enemies are within this range, and apply the taunt effect to these enemies.
    
    The taunt mechanic is especially useful in tactical situations where the player wants to divert enemy 
    attacks away from vulnerable characters. It will be primarily used by the Warrior class.
*/

public class Taunt : MonoBehaviour
{
    // ----- Section: Variables for Taunt Attack  -----

    public Tilemap attackRangeTilemap;  
    public Tilemap cursorTilemap;       
    public Tile cursorTile;             
    public Tile attackRangeTile;        
    public GameObject character;        
    public CursorController cursorController; 
    public Astar pathfinding;          
    public int tauntRange = 5;
    public ChatboxController chatboxController;

    void Update()
    {
        Vector3Int characterTilePosition = cursorTilemap.WorldToCell(character.transform.position);

        if (Input.GetKeyDown(KeyCode.B))
        {
            ClearAttackRangeTilemap();
            DrawAttackRange(characterTilePosition, tauntRange);
            PerformTaunt(characterTilePosition, tauntRange);
        }
    }


    // ----- Section: Taunt Logic  -----

    // Draws tiles to show the taunt range
    //void DrawAttackRange(Vector3Int centerTilePosition, int range)
    //{
    //    for (int dx = -range; dx <= range; dx++)
    //    {
    //        for (int dy = -range; dy <= range; dy++)
    //        {
    //            Vector3Int tilePosition = centerTilePosition + new Vector3Int(dx, dy, 0);
    //            if (pathfinding.IsWalkable(tilePosition))
    //            {
    //                attackRangeTilemap.SetTile(tilePosition, attackRangeTile);
    //            }
    //        }
    //    }
    //}


    void DrawAttackRange(Vector3Int centerTilePosition, int range)
    {
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int tilePosition = centerTilePosition + new Vector3Int(dx, dy, 0);
                if (Vector3Int.Distance(centerTilePosition, tilePosition) <= range && pathfinding.IsWalkable(tilePosition))
                {
                    attackRangeTilemap.SetTile(tilePosition, attackRangeTile);
                }
            }
        }
    }



    // Clears the taunt range display
    void ClearAttackRangeTilemap()
    {
        attackRangeTilemap.ClearAllTiles();
    }

    // Taunts all enemies within the specified range

    void PerformTaunt(Vector3Int centerTilePosition, int range)
    {
        CharacterStats taunterStats = character.GetComponent<CharacterStats>();
        // Nested loops to cover a square region around the center tile

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int tilePosition = centerTilePosition + new Vector3Int(dx, dy, 0);
                if (pathfinding.IsWalkable(tilePosition))
                {
                    // Check if there's an enemy on the current tile
                    RaycastHit2D hit = Physics2D.Raycast(attackRangeTilemap.GetCellCenterWorld(tilePosition), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            CharacterStats characterStats = hitObject.GetComponent<CharacterStats>();
                            characterStats.isTaunted = true;
                            // Save the taunter
                            characterStats.tauntedBy = taunterStats; 

                            // Log the taunter and tauntee information
                            Debug.Log("Taunter: " + taunterStats.characterName + " | Tauntee: " + characterStats.characterName);
                        }
                    }
                }
            }
        }
    }


}
