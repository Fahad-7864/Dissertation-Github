using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
    The IceSpellAoe class is responsible for managing the behavior of a water-based
    Area of Effect (AoE) spell in the game. The class handles spell casting, effects,
    targeting, and interactions with characters affected by the spell, particularly freezing mechanics.
    It also provides utilities for visualizing the spell's range and effect area, ensuring that 
    players have a clear understanding of the spell's impact.
*/

public class IceSpellAoe : CircleSpellTrial
{

    // ----- Section: Variables & References -----
    [SerializeField]
    public ChatboxController chatboxController;
    private GameManager gameManager; 
    public GameObject animationSpritePrefab; 
    [SerializeField]
    private Vector3Int bestSpellTilePosition;
    public TileBase bestSpellMarker;
    public EveryonesStats everyonesStats;
    public GameObject particleSystemPrefab; 

    // Track cooldown turns for the skill
    public int cooldownCounter = 0;
    // Cooldown in turns
    public int cooldown = 2;

    public int turnsFrozen = 2;

    void Awake()
    {
        gameManager = GameManager.Instance;

    }

    public void ReduceCooldown()
    {
        if (cooldownCounter > 0)
        {
            cooldownCounter--;
        }
    }

    void Start()
    {

        CharacterStats characterStats = character.GetComponent<CharacterStats>();

        if (characterStats != null)
        {
            //  Debug.Log("CharacterStats component found on character");

            if (characterStats.characterClass == CharacterClass.Archer)
            {
                //     Debug.Log("Character is an Archer");


                //     Debug.Log("Arrow damage calculated: " + arrowDamage);
            }
            else
            {
                //        Debug.Log("Character is not an Archer, class: " + characterStats.characterClass);
            }
        }
        else
        {
            Debug.Log("CharacterStats component not found on character");
        }


    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleSpellCasting();

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

    // Toggles the spell casting mode on/off For the skill button
    public void ToggleSpellCasting()
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

    // Automatically casts the spell to the best position based on enemies' positions
    public override void AutoCastSpell(Dictionary<string, List<Vector3Int>> enemyTiles)
    {
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
            {   // Check if AoE hits an enemy
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

    // Returns the AoE radius of the spell
    public int GetAOERadius()
    {
        return radius;
    }

    // Executes the spell attack on the specified tiles within the AoE
    public override void PerformAttack(Vector3Int center)
    {
        //// Cooldown Check
        if (cooldownCounter > 0)
        {
            Debug.Log("IcespellAOE is on cooldown");
            return;
        }
        // Flag to ensure particle effect is played only once
        bool particleEffectPlayed = false; 

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
                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            Debug.Log("PerformAttack: hit.collider is " + hit.collider);

                            attack.AttackEnemy(hitObject.name); 

                            if (!particleEffectPlayed)
                            {
                                // Instantiate the particle system at the enemy's position
                                GameObject particleSystemInstance = Instantiate(particleSystemPrefab, hitObject.transform.position, Quaternion.Euler(-25f, 0f, 0f));
                                Debug.Log("Particle system instantiated at position: " + particleSystemInstance.transform.position);

                                Destroy(particleSystemInstance, particleSystemInstance.GetComponent<ParticleSystem>().main.duration);
                                particleEffectPlayed = true; 
                            }

                            StartCoroutine(FreezeCharactersInAOE(center));

                            chatboxController.AddMessage(character.gameObject.name + " attacked " + hitObject.name);
                        }
                    }
                }
            }
        }
    }


    // Freezes characters within the spell's AoE
    IEnumerator FreezeCharactersInAOE(Vector3Int center)
    {
        List<GameObject> charactersToFreeze = new List<GameObject>();

        // The two nested loops iterate over a square grid centered at the 'center' tile.
        // The size of this grid is determined by the 'radius'. This effectively checks
        // each tile within the AoE to determine if there's a character on that tile to freeze.
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // Calculate the position of the current tile within the grid.
                Vector3Int tilePos = center + new Vector3Int(x, y, 0);

                // Check if the current tile position is within the defined radius of the center.
                // This ensures we are only acting on tiles within the circular AoE and not the entire square grid.
                if (Vector3Int.Distance(tilePos, center) <= radius)
                {
                    RaycastHit2D hit = Physics2D.Raycast(spellTilemap.GetCellCenterWorld(tilePos), Vector2.zero);

                    if (hit.collider != null)
                    {
                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            CharacterStats characterStats = hitObject.GetComponent<CharacterStats>();
                            if (characterStats && !characterStats.IsDead && !characterStats.IsFrozen)
                            {
                                charactersToFreeze.Add(hitObject);
                            }
                        }
                    }
                }
            }
        }

        // Loop through each character in the list and freeze them.
        foreach (var character in charactersToFreeze)
        {
            character.GetComponent<CharacterStats>().FreezeCharacter();
            //character.GetComponent<CharacterStats>().turnsFrozen = 2;  // Freeze for 2 turns
            character.GetComponent<CharacterStats>().turnsFrozen = turnsFrozen;
        }
        // End the coroutine.
        yield break;  
    }




    // Enters the spell casting mode for the character
    public void EnterSpellCastingMode()
    {
        isCastingSpell = true;
        ShowSpellRange();
    }
    // Exits the spell casting mode for the character
    public void ExitSpellCastingMode()
    {
        isCastingSpell = false;
        spellTilemap.ClearAllTiles();
    }


}
