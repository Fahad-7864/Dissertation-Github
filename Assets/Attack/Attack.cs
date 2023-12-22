/*
    The Attack class is responsible for managing the attack logic and interactions 
    between characters in the game. This class provides functionality to define, 
    execute, and visualize attacks, as well as handling related interactions such 
    as determining attack ranges, and handling damage calculations.
*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using System.Linq;



public enum AttackType
{
    Physical,
    Magical
}

public class Attack : MonoBehaviour
{
    // ----- Section: Tilemaps, UI, and Game Objects -----
    public Tilemap attackRangeTileMap;
    public Tile attackhighlightTile;
    public GameObject character;
    public TextMeshProUGUI damageTextUI;

    // ----- Section: Script References and Variables -----
    public EveryonesStats everyonesStats;
    public GameManager gameManager;
    public DisplayStats displayStats;
    public Astar pathfinding;
    public TurnManager turnManager;
    [SerializeField]
    private ChatboxController chatbox;

    // ----- Section: Attack Logic Variables -----
    [SerializeField]
    public bool isInAttackMode = false;
    private List<Vector3Int> highlightedAttackTiles = new List<Vector3Int>();
    private Vector3Int? currentlySelectedTile = null;
    public Tile confirmedTile;
    [SerializeField]
    private Vector3Int selectedTile;
    public AttackType attackType;
    private bool isAttackRangeVisible = false;
    public ElementType elementType;
    CharacterStats characterStats;


    // Initialize character stats on start.
    void Start()
    {
        characterStats = character.GetComponent<CharacterStats>();
    }


    void Update()
    {
        // Check if we are in attack mode
        if (isInAttackMode)
        {
            //HandleAttackTileSelection();

            // If we are, execute attack on enemy click
            AttackOnEnemyClick();

        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("ShowAttackRange pressed");

            ShowAttackRange();
            isInAttackMode = true;
            GetEnemyCharactersInAttackRange();

        }

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    // Attack the closest enemy
        //    string closestEnemy = GetClosestEnemy1();
        //    if (closestEnemy != null)
        //    {
        //        AttackEnemy(closestEnemy);

        //    }
        //}


        if (Input.GetKeyDown(KeyCode.F))
        {

            //GetFriendlyCharactersInAttackRange();
            //GetWeakestFriendly();

        }
        // Check for right click to clear attack range
        if (Input.GetMouseButtonDown(1) && isAttackRangeVisible) // 1 is for right click
        {
            ClearHighlightedAttackTiles();
        }




    }

    // ----- Section: Attack Range Methods -----

    // Calculate and return attack range tiles based on a hypothetical position.
    // This is meant for the attack tile in AISituationGrabber.
    public HashSet<Vector3Int> ShowAttackRange(Vector3Int hypotheticalPosition)
    {
        HashSet<Vector3Int> reachableTiles = pathfinding.GetReachableTiles(hypotheticalPosition, characterStats.attackRange);

        // This function now oreturns the reachable tiles without highlighting them or altering the game state
        return reachableTiles;
    }

    // Display the attack range for the character.
    public void ShowAttackRange()
    {
        Debug.Log("ShowAttackRange called");
        // Clear any previously highlighted attack tiles
        ClearHighlightedAttackTiles();

        // Enable attack mode.
        isInAttackMode = true;
        isAttackRangeVisible = true;

        Vector3Int currentTilePos = attackRangeTileMap.WorldToCell(character.transform.position);
        // Get all the tiles the character can attack to

        HashSet<Vector3Int> reachableTiles = pathfinding.GetReachableTiles(currentTilePos, characterStats.attackRange);
        // AddExtendedAttackRangeForArchers(reachableTiles);

        // Create a list to keep track of the enemies within attack range
        List<CharacterStats> enemiesInRange = new List<CharacterStats>();

        foreach (Vector3Int tilePos in reachableTiles)
        {
            // Exclude the current tile position
            if (tilePos != currentTilePos)
            {
                attackRangeTileMap.SetTile(tilePos, attackhighlightTile);
                highlightedAttackTiles.Add(tilePos);


                if (gameManager.occupiedTiles.ContainsKey(tilePos))
                {
                    CharacterStats characterOnTile = gameManager.occupiedTiles[tilePos];
                    CharacterStats enemyStats = characterOnTile.GetComponent<CharacterStats>();
                    // Must Have Enemy Type
                    if (enemyStats != null && enemyStats.type == CharacterType.Enemy)
                    {
                        // Add the enemy to the list
                        enemiesInRange.Add(enemyStats);

                        Debug.Log($"Enemy {enemyStats.characterName} is within attack range at tile position {tilePos}");
                    }
                }

                Debug.Log($"Tile position in range: {tilePos}");
            }
        }

        // After looping through all reachable tiles, print out the list of enemies
        Debug.Log($"Enemies in range: {string.Join(", ", enemiesInRange.Select(enemy => enemy.characterName))}");
    }

    // Clear the attack range for the character.
    public void ClearHighlightedAttackTiles()
    {
        // Disable attack mode.
        isInAttackMode = false;
        isAttackRangeVisible = false;

        foreach (Vector3Int tilePos in highlightedAttackTiles)
        {
            attackRangeTileMap.SetTile(tilePos, null);
        }

        highlightedAttackTiles.Clear();
    }




    // ----- Section: Attack Methods -----

    public bool AttackEnemy(string enemyName, int damageBonus = 0)
    {
        GameObject enemy = GameObject.Find(enemyName);
        // If the enemy exists and it's not the current character, attack it.
        if (enemy != null && enemy != character)
        {
            string characterTag = character.tag;
            string enemyTag = enemy.tag;
            // If the character is a friendly and the enemy is an enemy, or vice versa, carry out the attack.
            if ((characterTag == "Friendly" && enemyTag == "Enemy") || (characterTag == "Enemy" && enemyTag == "Friendly"))
            {
                CharacterStats enemyStats = everyonesStats.GetCharacterStats(enemyName);
                if (enemyStats != null)
                {
                    int damage;
                    if (attackType == AttackType.Physical)
                    {
                        damage = CalculateDamage(characterStats, enemyStats);
                        // Debugging statement
                        Debug.Log("Physical attack type used for calculating damage");

                    }

                    else
                    {
                        damage = (int)CalculateDamageForSpells(characterStats, enemyStats);
                        // Debugging statement
                        Debug.Log("Magical attack type used for calculating damage");

                    }
                    damage += damageBonus;

                    // Check if the character is on the opposite tile of the enemy
                    if (IsCharacterOnOppositeTile(enemy))
                    {
                        // Enhance the damage by 10 if the character is on the opposite tile of the enemy
                        damage += 25;
                        // Debugging statement
                        Debug.Log("Character is on the opposite tile of the enemy. Enhanced damage.");
                    }
                    else
                    {
                        // Debugging statement
                        Debug.Log("Character is not on the opposite tile of the enemy.");
                    }
                    // Check if the character is an archer
                    // This will then play the archer attack animation
                    if (characterStats.characterClass == CharacterClass.Archer && !IsCalledByLineOfArrows)
                    {
                        StartCoroutine(ArcherAttackRoutine(enemy, damage));
                    }

                    else
                    {
                        // The ChangeHP method will handle all necessary HP changes and death events
                        enemyStats.ChangeHP(-damage);

                        // enemyStats.hp -= damage;
                        // The damage text will stay visible for 2 seconds and then fade out over 2 seconds
                        StartCoroutine(ShowDamageRoutine(enemy, damage, 2f, 2f));
                        displayStats.ShowStatsByGameObject(enemy);
                        chatbox.AddMessage($"{character.name} attacks {enemy.name} for {damage} damage!");
                        ClearHighlightedAttackTiles();
                    }
                    return true; // attack was successful
                }
            }
        }
        ClearHighlightedAttackTiles(); // Clear the attack range after an attack

        return false; // enemy was not found or the attack didn't happen
    }

    // Gets the tile position opposite to the given enemy based on its facing direction.
    private Vector3Int GetOppositeTile(GameObject enemy)
    {
        CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
        if (enemyStats == null)
        {
            return Vector3Int.zero;
        }

        Vector3Int enemyTilePosition = attackRangeTileMap.WorldToCell(enemy.transform.position);
        Vector3Int oppositeTile;
        // Determine the opposite tile based on the enemy's facing direction.
        switch (enemyStats.facingDirection)
        {
            case Direction.Up:
                oppositeTile = enemyTilePosition + new Vector3Int(-1, 0, 0);
                break;
            case Direction.Down:
                oppositeTile = enemyTilePosition + new Vector3Int(1, 0, 0);
                break;
            case Direction.Left:
                oppositeTile = enemyTilePosition + new Vector3Int(0, -1, 0);
                break;
            case Direction.Right:
                oppositeTile = enemyTilePosition + new Vector3Int(0, 1, 0);
                break;
            default:
                oppositeTile = Vector3Int.zero;
                break;
        }

        return oppositeTile;
    }

    // Checks if the character is on the tile opposite to the given enemy by using a boolean.
    private bool IsCharacterOnOppositeTile(GameObject enemy)
    {
        Vector3Int oppositeTile = GetOppositeTile(enemy);
        Vector3Int characterTilePosition = attackRangeTileMap.WorldToCell(character.transform.position);

        return oppositeTile == characterTilePosition;
    }

    public GameObject arrowPrefab;

    // Casts an arrow towards the specified target.
    public void CastArrowShot(GameObject target)
    {
        Debug.Log("CastArrowShot called");

        // Create arrow at character's position
        GameObject arrowInstance = Instantiate(arrowPrefab, character.transform.position, Quaternion.identity);

        // Adjust arrow's display order
        SpriteRenderer sr = arrowInstance.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        StartCoroutine(MoveArrowTowardsTarget(arrowInstance, target));

        // Debugging statement
        Debug.Log("CastArrowShot Fired");
    }

    // Moves the arrow towards its target until it reaches or target is destroyed.
    IEnumerator MoveArrowTowardsTarget(GameObject Arrow, GameObject target)
    {
        // Get arrow's behavior
        ArrowProjectile arrowProjectile = Arrow.GetComponent<ArrowProjectile>();

        // Set arrow's target
        arrowProjectile.Initialize(target);

        // Wait for arrow to reach target or target to be gone
        while (arrowProjectile.enabled && target != null)
        {
            yield return null;
        }

        // Remove arrow once done
        Destroy(Arrow);
    }


    // This is to ensure that line of arrows and the normal attack animation for the atack 
    // are not played at the same time.
    public bool IsCalledByLineOfArrows { get; set; } = false;

    // Attacks the character that's taunting this character.
    public bool AttackTaunter()
    {
        // If character is taunted and has a taunter
        if (characterStats.isTaunted && characterStats.tauntedBy != null)
        {
            // Get the taunter's GameObject
            GameObject taunter = characterStats.tauntedBy.gameObject; 
            CharacterStats taunterStats = taunter.GetComponent<CharacterStats>();

            // Calculate damage based on attack type
            int damage;
            if (attackType == AttackType.Physical)
            {
                damage = CalculateDamage(characterStats, taunterStats);
            }
            else
            {
                damage = (int)CalculateDamageForSpells(characterStats, taunterStats);
            }
            // Apply damage to taunter
            taunterStats.ChangeHP(-damage);
            // Display damage and update stats
            StartCoroutine(ShowDamageRoutine(taunter, damage, 2f, 2f));
            displayStats.ShowStatsByGameObject(taunter);
            chatbox.AddMessage($"{character.name} attacks {taunter.name} for {damage} damage!");

            // Reset taunted state
            characterStats.isTaunted = false;

            return true; // Attack was successful
        }

        return false; // Character was not taunted or taunter not found
    }



    // Calculate damage based on the type of attack (Physical or Magical) and the stats
    // of the attacker and defender.

    public int CalculateDamage(CharacterStats attacker, CharacterStats defender)
    {
        if (attackType == AttackType.Physical)
        {
            // Base Damage Calculation
            int baseDamage = (int)(attacker.attack - defender.defence);
            // Ensure damage is not negative
            baseDamage = Mathf.Max(baseDamage, 0);
            return baseDamage;
        }
        else if (attackType == AttackType.Magical)
        {
            //Magic attack vs magic defence.
            int baseDamage = (int)(attacker.magicAttack - defender.magicDefence);
            // Ensure damage is not negative
            baseDamage = Mathf.Max(baseDamage, 0);
            return baseDamage;
        }
        else
        {
            Debug.LogError("Unsupported attack type!");
            return 0;
        }
    }



    private float CalculateDamageForSpells(CharacterStats attacker, CharacterStats defender)
    {
        float baseDamage = attacker.magicAttack - defender.magicDefence;

        // Ensure damage is not negative
        baseDamage = Mathf.Max(baseDamage, 0);

        return baseDamage;
    }


    // Retrieves the names of friendly characters within the attack range.
    public List<string> GetFriendlyCharactersInAttackRange()
    {
        List<string> charactersInAttackRange = new List<string>();
        foreach (CharacterStats stats in everyonesStats.allCharacterStats)
        {
            if (stats.type == CharacterType.Friendly)
            {
                CheckCharacterInRange(stats, charactersInAttackRange);
            }
        }
        return charactersInAttackRange;
    }

    // Retrieves the names of enemy characters within the attack range.
    public List<string> GetEnemyCharactersInAttackRange()
    {
        List<string> charactersInAttackRange = new List<string>();
        foreach (CharacterStats stats in everyonesStats.allCharacterStats)
        {
            if (stats.type == CharacterType.Enemy)
            {
                CheckCharacterInRange(stats, charactersInAttackRange);
            }
        }
        return charactersInAttackRange;
    }


    // Check if a character is in attack range and add their name to the list if they are.
    private void CheckCharacterInRange(CharacterStats stats, List<string> charactersInAttackRange)
    {
        // Debugging statement
        Debug.Log($"Checking character {stats.characterName}...");

        GameObject characterGameObject = GameObject.Find(stats.characterName);

        if (characterGameObject != null && characterGameObject.name != character.name)
        {
            Vector3Int characterPos = attackRangeTileMap.WorldToCell(characterGameObject.transform.position);
            Vector3Int currentCharacterPos = attackRangeTileMap.WorldToCell(character.transform.position);

            // Calculate the reachable tiles for the attack
            HashSet<Vector3Int> reachableTiles = pathfinding.GetReachableTiles(currentCharacterPos, characterStats.attackRange);
            // AddExtendedAttackRangeForArchers(reachableTiles);

            // Check if the character's position is within the reachable attack tiles
            if (reachableTiles.Contains(characterPos))
            {
                //Debug.Log($"Character {stats.characterName} is in attack range!");
                charactersInAttackRange.Add(stats.characterName);
            }
            else
            {
                // Debug.Log($"Character {stats.characterName} is NOT in attack range.");
            }
        }
        else
        {
            //    Debug.Log($"Could not find GameObject for character {stats.characterName}.");
        }
    }



    // Identifies the weakest 'Friendly' character within attack range.
    // Used primarily by enemy AI to target the most vulnerable player characters.
    public string GetWeakestFriendly()
    {
        List<string> charactersInRange = GetFriendlyCharactersInAttackRange();
        int lowestHealth = int.MaxValue;
        string weakestEnemy = null;

        Debug.Log($"Found {charactersInRange.Count} characters in attack range.");

        foreach (string characterName in charactersInRange)
        {
            GameObject characterGameObject = GameObject.Find(characterName);
            if (characterGameObject != null)
            {
                if (characterGameObject.CompareTag("Friendly"))
                {
                    int health = GetFriendlyHealth(characterName);
                    Debug.Log($"Character {characterName} found with health: {health}");
                    if (health < lowestHealth)
                    {
                        lowestHealth = health;
                        weakestEnemy = characterName;
                    }
                }
                else
                {
                    Debug.Log($"Character {characterName} found but did not have Friendly tag.");
                }
            }
            else
            {
                Debug.Log($"Character {characterName} not found.");
            }
        }

        if (weakestEnemy != null)
        {
            Debug.Log($"Weakest enemy found: {weakestEnemy} with health: {lowestHealth}.");
        }
        else
        {
            Debug.Log("No weakest enemy found among friendly characters.");
        }

        return weakestEnemy;
    }

    // Retrieves the health of a specified 'Friendly' character by their name. 
    public int GetFriendlyHealth(string enemyName)
    {
        GameObject enemy = GameObject.Find(enemyName);
        if (enemy != null && enemy.CompareTag("Friendly"))
        {
            CharacterStats enemyStats = everyonesStats.GetCharacterStats(enemyName);
            if (enemyStats != null)
            {
                return (int)enemyStats.hp;
            }
            else
            {
                Debug.Log($"Could not find stats for enemy {enemyName}.");
            }
        }

        return int.MaxValue;
    }




    public void AttackOnEnemyClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse button pressed");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = -ray.origin.z / ray.direction.z;
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3Int clickedTile = attackRangeTileMap.WorldToCell(mouseWorldPos);

            if (highlightedAttackTiles.Contains(clickedTile))
            {
                Debug.Log("Clicked tile is in attack range");

                // Check if the clicked tile is the same as the currently selected tile
                if (currentlySelectedTile.HasValue && currentlySelectedTile.Value == clickedTile)
                {
                    // Raycast to detect if there is an enemy on the clicked tile
                    RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
                    if (hit.collider != null)
                    {
                        Debug.Log("Raycast hit a game object");

                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            Debug.Log("Raycast hit an enemy game object");
                            StartCoroutine(SelectedTileRoutine(hitObject));
                            // Set attack type to physical
                            attackType = AttackType.Physical;

                            //  Call AttackEnemy if an enemy is hit
                            AttackEnemy(hitObject.name);

                            // Get active character stats
                            CharacterStats activeCharacterStats = turnManager.GetActiveCharacterStats();

                            // Reduce energy by 100 
                            if (activeCharacterStats != null)
                            {
                                activeCharacterStats.energy = Mathf.Max(0, activeCharacterStats.energy - 100);
                            }

                            // Reset the currently selected tile
                            currentlySelectedTile = null;
                        }
                        else
                        {
                            Debug.Log("Raycast hit a game object that is not tagged as an enemy");
                        }
                    }
                    else
                    {
                        Debug.Log("Raycast did not hit any game objects");
                    }
                }
                else
                {
                    Debug.Log("Clicked tile is not the selected tile");

                    // If there is already a selected tile, change its color back to the highlight tile
                    if (currentlySelectedTile.HasValue)
                    {
                        attackRangeTileMap.SetTile(currentlySelectedTile.Value, attackhighlightTile);
                    }

                    // Change the color of the clicked tile to the confirmed tile and set it as the selected tile
                    attackRangeTileMap.SetTile(clickedTile, confirmedTile);
                    currentlySelectedTile = clickedTile;
                }
            }
            else
            {
                Debug.Log("Clicked tile is not in attack range");
            }
        }
    }


    public int GetAttackRange()
    {
        return characterStats.attackRange;
    }



    // ----- Section: UI-related Methods -----

    // Shows a blackout effect for a brief moment when a tile is selected.
    IEnumerator SelectedTileRoutine(GameObject hitObject)
    {
        // Access the UI Image component and enable it
       // Image blackoutImage = GameObject.Find("BlackoutImage").GetComponent<Image>();
       // blackoutImage.enabled = true;

        // Wait for a certain amount of time
        yield return new WaitForSeconds(2f);

        // Disable the blackout image after attacking
       // blackoutImage.enabled = false;
    }

    // Displays the damage dealt to an enemy.
    IEnumerator ShowDamageRoutine(GameObject enemy, int damage, float displayDuration, float fadeDuration)
    {
        // Convert enemy's position to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up);  // Add Vector3.up to position the text above the enemy

        // Assign screenPos to the position of damageTextUI
        damageTextUI.transform.position = screenPos;

        damageTextUI.text = $"-{damage}";
        damageTextUI.color = Color.black;
        damageTextUI.enabled = true;

        yield return new WaitForSeconds(displayDuration);

        // Store the initial time
        float initialTime = Time.time;

        // Fade phase
        while (Time.time - initialTime < fadeDuration)
        {
            // Calculate how far through the duration we are (0 to 1)
            float t = (Time.time - initialTime) / fadeDuration;

            // Fade out the damage text over time
            damageTextUI.color = Color.Lerp(Color.black, Color.clear, t);

            // Wait until the next frame
            yield return null;
        }

        // Disable the damage text UI object
        damageTextUI.enabled = false;
    }


    // ----- Section: : Archer Attack Logic -----

    private bool arrowHitTarget = false;

    private void OnEnable()
    {
        ArrowProjectile.OnArrowHit += ArrowHit;
    }

    private void OnDisable()
    {
        ArrowProjectile.OnArrowHit -= ArrowHit;
    }

    // Callback when the arrow hits its target.
    private void ArrowHit()
    {
        arrowHitTarget = true;
    }

    // Handles the archer's attack routine, casting the arrow and dealing damage once it hits.
    IEnumerator ArcherAttackRoutine(GameObject enemy, int damage)
    {
        CastArrowShot(enemy);
        // Wait for the arrow to hit the target
        yield return new WaitUntil(() => arrowHitTarget); 
       // Use the ChangeHP method to apply damage
        enemy.GetComponent<CharacterStats>().ChangeHP(-damage);
        // Show the damage dealt
        StartCoroutine(ShowDamageRoutine(enemy, damage, 2f, 2f)); 
        displayStats.ShowStatsByGameObject(enemy);
        chatbox.AddMessage($"{character.name} attacks {enemy.name} for {damage} damage!");
        ClearHighlightedAttackTiles();
    }



}