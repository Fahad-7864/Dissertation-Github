using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
    The ArcherCircleSpell class is responsible for handling the specific 
    circle-spell functionality of the Archer character type. This includes 
    the management of the spell range, AoE (Area of Effect), and the logic 
    to perform attacks on enemies within the AoE.
*/
public class ArcherCircleSpell : CircleSpellTrial
{
    // ----- Section: Variables & References -----

    [SerializeField]
    private float arrowDamage; // Additional damage dealt by the rain of arrows
    public ChatboxController chatboxController;
    private GameManager gameManager; 
    public GameObject animationSpritePrefab; 
    [SerializeField]
    private Vector3Int bestSpellTilePosition;
    public TileBase bestSpellMarker;
    public EveryonesStats everyonesStats;



    // ----- Section: Initialization Methods -----


    // Initialize game manager instance
    void Awake()
    {
        gameManager = GameManager.Instance; 

    }

    // Set up the archer-specific damage for this spell.
    void Start()
    {

        CharacterStats characterStats = character.GetComponent<CharacterStats>();

        if (characterStats != null)
        {
          //  Debug.Log("CharacterStats component found on character");

            if (characterStats.characterClass == CharacterClass.Archer)
            {
               // Debug.Log("Character is an Archer");

                arrowDamage = characterStats.accuracy / 10;

               // Debug.Log("Arrow damage calculated: " + arrowDamage);
            }
            else
            {
                // Debug.Log("Character is not an Archer, class: " + characterStats.characterClass);
            }
        }
        else
        {
            Debug.Log("CharacterStats component not found on character");
        }


    }


    // ----- Section: Real-time Game Logic -----

    // Handling various inputs and corresponding actions
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
                // Clear AoE Tiles if hovered tile is not within spell range
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

    // ----- Section: Spell Casting Logic -----
    // Decide the best casting position automatically based on enemies' positions
    public override void AutoCastSpell(Dictionary<string, List<Vector3Int>> enemyTiles)
    {
        // Initialize variables
        Vector3Int bestSpellPosition = new Vector3Int();  
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
            {
                if (enemyTiles["spellAoETilemap"].Contains(aoePos))  // Check if AoE hits an enemy
                {
                    enemiesHit++;
                }
            }

            // If this position hits more enemies, update bestSpellPosition and maxEnemiesHit
            if (enemiesHit > maxEnemiesHit)
            {
                // Set this position as the new best position
                bestSpellPosition = pos; 
                maxEnemiesHit = enemiesHit;
                Debug.Log("New best position found at " + pos + ", hitting " + enemiesHit + " enemies.");
            }
        }

        // If we found a best position, cast the spell
        if (maxEnemiesHit > 0)
        {
            Debug.Log("Selected position for attack: " + bestSpellPosition + ", which hits " + maxEnemiesHit + " enemies.");

            // Clear previous tiles
            spellTilemap.ClearAllTiles();
            ClearSpellAoE();  
            // Highlight the best spell position
            spellTilemap.SetTile(bestSpellPosition, bestSpellTile);

            PerformAttack(bestSpellPosition);
            isCastingSpell = false;
        }
    }

    // Return the Area of Effect radius of the spell
    public int GetAOERadius()
    {
        return radius;
    }

    // Perform the spell attack in the given area
    public override void PerformAttack(Vector3Int center)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int tilePos = center + new Vector3Int(x, y, 0);
                Debug.Log("PerformAttack: tilePos is " + tilePos);

                if (Vector3Int.Distance(tilePos, center) <= radius)
                {
                    RaycastHit2D hit = Physics2D.Raycast(spellTilemap.GetCellCenterWorld(tilePos), Vector2.zero);
                    Debug.Log("PerformAttack: hit.collider is " + hit.collider);
                    if (hit.collider != null)
                    {
                        CharacterStats characterStats = character.GetComponent<CharacterStats>();

                        GameObject hitObject = hit.collider.gameObject;

                        if ((characterStats.type == CharacterType.Friendly && hitObject.CompareTag("Enemy")) ||
                            (characterStats.type == CharacterType.Enemy && hitObject.CompareTag("Friendly")))
                        {
                            Debug.Log("PerformAttack: hit.collider is " + hit.collider);

                            attack.AttackEnemy(hitObject.name);
                            GameObject animationSprite = Instantiate(animationSpritePrefab, hitObject.transform.position + Vector3.up, Quaternion.identity);
                            Debug.Log("Animation sprite instantiated at local position: " + animationSprite.transform.localPosition);
                            // Coroutine to drop the sprite on y-axis
                            StartCoroutine(DropSprite(animationSprite));
                            chatboxController.AddMessage(character.gameObject.name + " attacked " + hitObject.name);
                        }
                    }
                }
            }
        }
    }


    // Animate the dropping of the sprite over time
    IEnumerator DropSprite(GameObject sprite)
    {
        // Initial local position
        Vector3 startPosition = sprite.transform.position;
        // Target local position (below the enemy)
        Vector3 targetPosition = startPosition - Vector3.up;
        Debug.Log("Starting drop from local position: " + startPosition + " to local position: " + targetPosition);

        float dropDuration = 1.0f; 
        float dropProgress = 0.0f; 

        // Gradually move the sprite from start to target local position
        while (dropProgress < dropDuration)
        {
            dropProgress += Time.deltaTime; 
            float t = dropProgress / dropDuration; // Calculate normalized progress

            // Update the sprite's local position
            sprite.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            // Debug.Log("Updated local position to: " + sprite.transform.localPosition);
            // Wait for the next frame
            yield return null; 
        }
        Debug.Log("Finished dropping sprite. Final local position: " + sprite.transform.localPosition);

        // After dropping, you can destroy the sprite or make it disappear in some other way
        Destroy(sprite);
    }

    // Enter the mode where the player can cast the spell
    public void EnterSpellCastingMode()
    {
        isCastingSpell = true;
        ShowSpellRange();
    }

    // Exit the spell casting mode and clear any visual indicators
    public void ExitSpellCastingMode()
    {
        isCastingSpell = false;
        spellTilemap.ClearAllTiles();
    }


}






