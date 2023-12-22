using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
    The SpinAttack class manages the warrior's special ability to perform a spin attack in the game. 
    This attack targets multiple tiles around the warrior. The tiles within the range of the spin 
    attack are visually highlighted on the screen. Any enemies within this range are attacked 
    when the ability is triggered.
*/
public class SpinAttack : MonoBehaviour
{
    // ----- Section: Variables for SpinAttack -----

    public Tilemap attackRangeTilemap;  
    public Tilemap cursorTilemap;       
    public Tile cursorTile;              
    public Tile attackRangeTile;        
    public GameObject character;         
    public CursorController cursorController; 
    public Astar pathfinding;            
    public int spinRange = 1;         
    public Attack attack;
    public ElementType elementType;

    private Vector3Int lastCharacterPosition;

    void Update()
    {
        Vector3Int characterTilePosition = cursorTilemap.WorldToCell(character.transform.position);

        if (Input.GetKeyDown(KeyCode.B))
        {
            ClearAttackRangeTilemap();
            DrawAttackRange(characterTilePosition, spinRange);
            PerformSpinAttack(characterTilePosition, spinRange);
        }
    }

    // ----- Section: Spin Attack Logic -----

    void DrawAttackRange(Vector3Int centerTilePosition, int range)
    {
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int tilePosition = centerTilePosition + new Vector3Int(dx, dy, 0);
                if (pathfinding.IsWalkable(tilePosition))
                {
                    attackRangeTilemap.SetTile(tilePosition, attackRangeTile);
                }
            }
        }
    }

    void ClearAttackRangeTilemap()
    {
        attackRangeTilemap.ClearAllTiles();
    }

    // Executes the spin attack, attacking all enemies within the specified range
    void PerformSpinAttack(Vector3Int centerTilePosition, int range)
    {
        // The outer loop runs through the horizontal range (from left to right) around the center tile.
        for (int dx = -range; dx <= range; dx++)
        {
            // The inner loop runs through the vertical range (from top to bottom) for each horizontal position.
            // This results in checking all tiles in a square area around the center tile.
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int tilePosition = centerTilePosition + new Vector3Int(dx, dy, 0);
                if (pathfinding.IsWalkable(tilePosition))
                {
                    RaycastHit2D hit = Physics2D.Raycast(attackRangeTilemap.GetCellCenterWorld(tilePosition), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            attack.elementType = elementType;
                            attack.AttackEnemy(hitObject.name);
                        }
                    }
                }
            }
        }
    }
}
