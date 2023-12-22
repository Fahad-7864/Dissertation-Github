using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
    The LineOfArrows class manages the logic for a character's ability to cast a line of arrows spell.
    This class handles the visual representation of the line, as well as the mechanics to determine
    valid attack targets and perform the actual attack. 
*/
public class LineOfArrows : MonoBehaviour
{
    // ----- Section: Visual Representation and Line Properties -----
    public Tilemap lineOfArrowsTilemap;
    public Tile lineOfArrowsTile;
    public Tile cursorTile;
    public GameObject character;
    public CursorController cursorController;
    public int spellRange = 4;

    // ----- Section: Pathfinding and Tile Management -----
    public Astar pathfinding;
    private Dictionary<Vector3Int, TileBase> originalTiles;
    private Vector3Int lastHoveredTile;
    private HashSet<Vector3Int> reachableTiles;


    // ----- Section: Attack Confirmation and Targeting -----
        public bool confirmingAttack = false;
    private Vector3Int attackTarget = Vector3Int.one * -1;
    public GameObject arrowAnimationPrefab;
    public Attack attack;
    public ElementType elementType;
    public bool isUpdatingLine = false;


    // ----- Section: Skill Cooldown -----
    public int cooldownCounter = 0;
    public int cooldown = 3;


    // Initialize the original tiles dictionary and set the attack type.
    public void Start()
    {
        originalTiles = new Dictionary<Vector3Int, TileBase>();
        attack.attackType = AttackType.Magical;


    }

    // Handles player input to cast the line of arrows spell or to confirm an attack.
    void Update()
    {
       // UpdateLine();
        // If the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int clickedTilePos = lineOfArrowsTilemap.WorldToCell(cursorController.GetCursorWorldPosition());
            Vector3Int characterPos = lineOfArrowsTilemap.WorldToCell(character.transform.position);
            Vector3Int[] neighboringTiles = pathfinding.GetNeighbors(characterPos);

            // Check if the clicked tile is a neighboring tile and in line of sight
            foreach (Vector3Int tile in neighboringTiles)
            {
                if (tile == clickedTilePos && pathfinding.IsWalkable(clickedTilePos))
                {
                    // If the player is not in the process of confirming an attack, start confirming
                    if (!confirmingAttack && isUpdatingLine)
                    {
                        confirmingAttack = true;
                        attackTarget = clickedTilePos;
                    }
                    // If the player is in the process of confirming an attack and clicked on the same tile again, perform the attack
                    else if (clickedTilePos == attackTarget && isUpdatingLine)
                    {
                        Attack(clickedTilePos);
                        confirmingAttack = false;
                        attackTarget = Vector3Int.one * -1;
                        isUpdatingLine = false;  
                    }
                    // If the player is in the process of confirming an attack but clicked on a different tile, change the target
                    else if (isUpdatingLine)
                    {
                        attackTarget = clickedTilePos;
                    }
                    // End the loop once we found the clicked tile in neighboring tiles
                    break;  
                }
            }
        }

        if (isUpdatingLine)
        {
            UpdateLine();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            bool canUseSkill = CanUseSkillFromCurrentPosition();
            CharacterStats characterStats = character.GetComponent<CharacterStats>();
            Debug.Log("Character: " + characterStats.characterName + " (Type: " + characterStats.type + ") can attack from this position? " + (canUseSkill ? "Yes" : "No"));

            // If the character can use the skill, print the detected targets
            if (canUseSkill)
            {
                List<string> targetsInRange = GetEnemiesInLineOfArrowsRange(); 
                Debug.Log("Detected targets: " + string.Join(", ", targetsInRange));
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            UpdateLine();
        }

    }



    // Displays the line of arrows and handles cursor movement.
    public void UpdateLine()
    {
        // Convert the current cursor position in the world to its corresponding position on the tilemap.
        Vector3Int hoveredTilePos = lineOfArrowsTilemap.WorldToCell(cursorController.GetCursorWorldPosition());

        // If the cursor has moved to a new tile, restore the original tile at the last hovered position
        if (hoveredTilePos != lastHoveredTile && originalTiles.ContainsKey(lastHoveredTile))
        {
            // restore the original tile graphic at the tile position the cursor just left.
            lineOfArrowsTilemap.SetTile(lastHoveredTile, originalTiles[lastHoveredTile]);
            originalTiles.Remove(lastHoveredTile);
        }

        lastHoveredTile = hoveredTilePos;

        lineOfArrowsTilemap.ClearAllTiles();
        Vector3Int startTilePos = lineOfArrowsTilemap.WorldToCell(character.transform.position);

        Vector3Int[] neighboringTiles = pathfinding.GetNeighbors(startTilePos);

        foreach (var tilePos in neighboringTiles)
        {
            if (pathfinding.IsWalkable(tilePos))
            {
                lineOfArrowsTilemap.SetTile(tilePos, cursorTile);
            }
        }

        if (Array.IndexOf(neighboringTiles, hoveredTilePos) > -1)
        {
            // Save the original tile at the current hovered position
            originalTiles[hoveredTilePos] = lineOfArrowsTilemap.GetTile(hoveredTilePos);

            Vector3 directionFloat = ((Vector3)hoveredTilePos - (Vector3)startTilePos).normalized;
            Vector3Int direction = new Vector3Int(Mathf.RoundToInt(directionFloat.x), Mathf.RoundToInt(directionFloat.y), 0);

            for (int i = 1; i <= 4; i++)
            {
                Vector3Int tilePos = hoveredTilePos + new Vector3Int(direction.x * i, direction.y * i, 0);

                if (pathfinding.IsWalkable(tilePos))
                {
                    // Save the original tile at the current position
                    originalTiles[tilePos] = lineOfArrowsTilemap.GetTile(tilePos);

                    lineOfArrowsTilemap.SetTile(tilePos, lineOfArrowsTile);

                }
            }
        }
    }


    // ----- Section: Attack Mechanics -----
    // Executes the attack on valid targets.

    public virtual void Attack(Vector3Int targetTilePos)
    {
        Debug.Log("Attempting to attack at tile position: " + targetTilePos);

        // Cooldown Check
        if (cooldownCounter > 0)
        {
            Debug.Log("LineOfArrows is on cooldown");
            return;
        }
        List<string> enemiesInRange = GetEnemiesInLineOfArrowsRange();

        // If there are no enemies in range, return without attacking
        if (enemiesInRange.Count == 0)
        {
            Debug.Log("No enemies in attack range!");
            return;
        }
        // Log the count of enemies
        Debug.Log("Enemies in range: " + enemiesInRange.Count);

        // Define spellRange.
        int spellRange = 4;

        Vector3Int startTilePos = lineOfArrowsTilemap.WorldToCell(character.transform.position);
        Vector3 directionFloat = ((Vector3)targetTilePos - (Vector3)startTilePos).normalized;
        Vector3Int direction = new Vector3Int(Mathf.RoundToInt(directionFloat.x), Mathf.RoundToInt(directionFloat.y), 0);

        for (int i = 1; i <= spellRange; i++)
        {
            Vector3Int tilePos = startTilePos + direction * i;
            Debug.Log("Checking tile position: " + tilePos + " for potential attack.");

            // Convert tilePos to world position
            Vector3 spawnPosition = lineOfArrowsTilemap.GetCellCenterWorld(tilePos);

            if (pathfinding.IsWalkable(tilePos))
            {
                Debug.Log("Tile position: " + tilePos + " is walkable.");

                CharacterStats characterStats = character.GetComponent<CharacterStats>();
                // Perform a raycast to check if there is an enemy at this tile
                RaycastHit2D hit = Physics2D.Raycast(lineOfArrowsTilemap.GetCellCenterWorld(tilePos), Vector2.zero);
                if (hit.collider != null)
                {
                    Debug.Log("Raycast hit detected at tile position: " + tilePos);

                    GameObject hitObject = hit.collider.gameObject;
                    if (characterStats.type == CharacterType.Friendly && hitObject.CompareTag("Enemy") ||
                                    characterStats.type == CharacterType.Enemy && hitObject.CompareTag("Friendly"))
                    {
                        Debug.Log("Detected a valid target at tile position: " + tilePos);

                        attack.elementType = elementType;
                        attack.IsCalledByLineOfArrows = true;
                        // Check if the attack is successful do 10 extra damage
                        bool attackSuccessful = attack.AttackEnemy(hitObject.name, 10);
                        attack.IsCalledByLineOfArrows = false;
                        if (attackSuccessful)
                        {
                            Debug.Log("Attack successful against enemy at position: " + tilePos);
                            // Instantiate the arrow animation prefab at the calculated position
                            GameObject arrowAnimation = Instantiate(arrowAnimationPrefab, spawnPosition, Quaternion.identity);

                            // Start the DropSprite coroutine for the arrow animation
                            StartCoroutine(DropSprite(arrowAnimation));
                            cooldownCounter = cooldown;
                        }
                        else
                        {
                            Debug.Log("Attack was not successful against enemy at position: " + tilePos);
                        }
                    }
                    else
                    {
                        Debug.Log("Detected an object at tile position: " + tilePos + ", but it's not a valid target.");
                    }
                }
                else
                {
                    Debug.Log("No raycast hit detected at tile position: " + tilePos);
                }
            }
            else
            {
                Debug.Log("Tile position: " + tilePos + " is not walkable.");
            }
        }

        lineOfArrowsTilemap.ClearAllTiles();
        originalTiles.Clear();
    }


    // Reduces the cooldown counter every turn.
    public void ReduceCooldown()
    {
        if (cooldownCounter > 0)
        {
            cooldownCounter--;
        }
    }

    // Checks if the skill can be used from the current position.
    // This is an alternative method to BestSpellCastDirection
    // Currently not implemented in the mechanics but will be used in the future.
    public bool CanUseSkillFromCurrentPosition()
    {
        // Get the character's position
        Vector3Int characterPos = lineOfArrowsTilemap.WorldToCell(character.transform.position);

        // Get the neighboring tiles
        Vector3Int[] neighboringTiles = pathfinding.GetNeighbors(characterPos);

        // Iterate through the neighboring tiles to check if any are valid attack targets
        foreach (Vector3Int tile in neighboringTiles)
        {
            if (pathfinding.IsWalkable(tile))
            {
                List<string> enemiesInRange = GetEnemiesInLineOfArrowsRange();

                // If there are enemies in range, the skill can be used from the current position
                if (enemiesInRange.Count > 0)
                {
                    return true;
                }
            }
        }

        // If no valid attack targets were found, the skill cannot be used from the current position
        return false;
    }


    // ----- Section: Animation and Visual Effects -----
    // Handles the dropping animation for the arrow sprites.
    IEnumerator DropSprite(GameObject sprite)
    {
        // Initial local position
        Vector3 startPosition = sprite.transform.position + new Vector3(0, 1, 0);  
        
        // Target local position (below the enemy)
        Vector3 targetPosition = startPosition - Vector3.up;
        Debug.Log("Starting drop from local position: " + startPosition + " to local position: " + targetPosition);

        float dropDuration = 1.5f; 
        float dropProgress = 0.0f; 

        while (dropProgress < dropDuration)
        {
            dropProgress += Time.deltaTime; // Update progress
            float t = dropProgress / dropDuration; 

            // Update the sprite's local position
            sprite.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            // Debug.Log("Updated local position to: " + sprite.transform.localPosition);

            yield return null; 
        }
        Debug.Log("Finished dropping sprite. Final local position: " + sprite.transform.localPosition);

        Destroy(sprite);
    }


    // Identifies and returns the names of enemies in the range of the line of arrows spell.
    public List<string> GetEnemiesInLineOfArrowsRange()
    {
        List<string> enemiesInLineRange = new List<string>();
        CharacterStats characterStats = character.GetComponent<CharacterStats>();

        Vector3Int startTilePos = lineOfArrowsTilemap.WorldToCell(character.transform.position);
        Vector3Int[] neighboringTiles = pathfinding.GetNeighbors(startTilePos);

        foreach (Vector3Int tilePos in neighboringTiles)
        {
            Vector3Int direction = tilePos - startTilePos;
            for (int i = 1; i <= spellRange; i++)
            {
                Vector3Int checkTilePos = startTilePos + direction * i;

                // Perform a raycast to check if there is an enemy at this tile
                RaycastHit2D hit = Physics2D.Raycast(lineOfArrowsTilemap.GetCellCenterWorld(checkTilePos), Vector2.zero);
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    // Check the tag based on the character type from CharacterStats
                    if (characterStats.type == CharacterType.Friendly && hitObject.CompareTag("Enemy") ||
                        characterStats.type == CharacterType.Enemy && hitObject.CompareTag("Friendly"))
                    {
                        string enemyName = hitObject.name;
                        if (!enemiesInLineRange.Contains(enemyName))
                        {
                            enemiesInLineRange.Add(enemyName);
                        }
                    }
                }
            }
        }

        return enemiesInLineRange;
    }






    // ----- Section: AI and Scoring Mechanisms -----
    // Determines the best direction to cast the spell based on current unit position.
    public Vector3Int GetBestSpellCastDirection(GameObject unit)
    {

        // Initialize the best direction to the unit's current forward direction
        Vector3Int bestDirection = Vector3Int.right;  
        float bestScore = float.MinValue;

        // Get the neighboring tiles which represent the four directions
        Vector3Int startTilePos = lineOfArrowsTilemap.WorldToCell(unit.transform.position);
        Vector3Int[] neighboringTiles = pathfinding.GetNeighbors(startTilePos);

        // For each direction
        foreach (Vector3Int tilePos in neighboringTiles)
        {
            Vector3Int direction = tilePos - startTilePos;

            float score = ScoreSpellCastDirection(unit, direction, spellRange);

          //  Debug.Log("Evaluated spell cast direction: " + direction + " Score: " + score);

            // If this direction's score is better than the best score we've seen, update the best direction and best score
            if (score > bestScore)
            {
                Debug.Log("New best spell cast direction found: " + direction + " Score: " + score);
                bestScore = score;
                bestDirection = direction;
            }
        }

        return bestDirection;
    }


    // Scores each potential spell cast direction based on potential targets and their health.
    public float ScoreSpellCastDirection(GameObject unit, Vector3Int direction, int spellRange)
    {
        // Initialize the score to 0
        float score = 0;

        Vector3Int startTilePos = lineOfArrowsTilemap.WorldToCell(unit.transform.position);
        CharacterStats characterStats = character.GetComponent<CharacterStats>();

        // For each tile in the line of the spell's range...
        for (int i = 1; i <= spellRange; i++)
        {
            Vector3Int tilePos = startTilePos + direction * i;

            // If this tile is occupied by an enemy unit...
            RaycastHit2D hit = Physics2D.Raycast(lineOfArrowsTilemap.GetCellCenterWorld(tilePos), Vector2.zero);
            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;
                if (characterStats.type == CharacterType.Friendly && hitObject.CompareTag("Enemy") ||
                                        characterStats.type == CharacterType.Enemy && hitObject.CompareTag("Friendly"))
                {
                    // Increase the score by some amount (e.g. the enemy's health)
                    CharacterStats characterOnTile = hitObject.GetComponent<CharacterStats>();
                    if (characterOnTile != null)
                    {
                        score += characterOnTile.hp;
                        Debug.Log("Adding score for enemy at position: " + tilePos + " Score: " + score);
                    }
                }
            }
        }

        return score;
    }


}

