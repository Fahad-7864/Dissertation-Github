using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

/*
    The AISituationGrabber class is responsible for analyzing the current state of the game 
    and determining the best course of action for the AI. It evaluates the best positions,
    attack strategies, and movement patterns based on the game's situation.
*/

public class AISituationGrabber : MonoBehaviour
{
    // ----- Section: References and Variables -----
    public Attack attackScript;
    public HighlightandMovement movementScript;
    public AI AI;
    public GameManager gameManager;
    public Attack Attackscript;
    public LineOfArrows lineOfArrows;
    public ArcherCircleSpell archerCircleSpell;

    // ----- Section: Tilemap Variables -----
    public Tile bestAttacMarker;
    public Tile bestTileMarker;
    public Tile preferredTileMarker;
    public Tile bestFlankMarker;
    public Tilemap moveRangeTilemap;
    public Tilemap attackRangeTilemap;
    public Tilemap Tilemap; 

    // ----- Section: BestTile Variables -----
    private Vector3Int bestTilePosition;
    [SerializeField]
    public Situation bestSituation;
    public bool isMoveToPreferredTilePossible = false;


    // ----- Section: Situation Class -----
    // Represents the best situation for the AI
    [System.Serializable]
    public class Situation
    {
        // Situation information
        public float situationValue;
        public string targetCharacter;
        public Vector3Int targetTile;
        public bool useAttack;

        public Situation(float situationValue, string targetCharacter, Vector3Int targetTile, bool useAttack)
        {
            this.situationValue = situationValue;
            this.targetCharacter = targetCharacter;
            this.targetTile = targetTile;
            this.useAttack = useAttack;
        }

        // Default constructor
        public Situation()
        {
            this.situationValue = -100000;
            this.targetCharacter = null;
            this.targetTile = new Vector3Int();
            this.useAttack = false;
        }
    }



    // ----- Section: Initialization -----
    void Start()
    {
        // Find the game manager
        gameManager = FindObjectOfType<GameManager>();
        //StartCoroutine(CalculateBestsituationCoroutineForAttackArcher());
        //StartCoroutine(CalculateBestSituationCoroutineByFacing());
        // start the coroutine at the beginning of the game
        //StartCoroutine(CalculateBestSituationCoroutine());
        // StartCoroutine(CalculateBestSituationCoroutineForAttack());
        //  StartCoroutine(CalculateBestSituationCoroutineByFacing());
    }


    // ----- Section: AI Logic -----
    // Calculate the best situation for AI movement based on enemy positions.
    public IEnumerator CalculateBestSituationCoroutine()
    {
        EveryonesStats statsManager = FindObjectOfType<EveryonesStats>();
        CharacterStats thisCharacterStats = statsManager.GetCharacterStats(this.gameObject.name);

        // Get the character stats from the stats manager
        string targetName = FindLowestDefenceFriendly();

        // Check if the character is taunted
        if (thisCharacterStats.isTaunted)
        {
            Debug.Log("Character " + this.gameObject.name + " is taunted by " + thisCharacterStats.tauntedBy.characterName);
            // If the character is taunted, target the caster of the taunt
            targetName = thisCharacterStats.tauntedBy.characterName;
        }
        else
        {
            Debug.Log("Character " + this.gameObject.name + " is not taunted");
        }

        // Get the target character name from the FindLowestDefenceFriendly method
        CharacterStats targetStats = statsManager.GetCharacterStats(targetName);

        // If the stats or game object doesn't exist, don't run the coroutine
        if (targetStats == null || targetStats.characterGameObject == null)
        {
            Debug.Log("Target '" + targetName + "' not found");
            yield break;
        }


        // Get the position of the game object
        Vector3 targetPosition = targetStats.characterGameObject.transform.position;

        movementScript.ShowMoveRange();
        // Get all the reachable tiles
        List<Vector3Int> tilesInMovementRange = movementScript.highlightedTiles;

        Vector3Int bestTile = new Vector3Int();
        float shortestDistance = float.MaxValue;

        // Loop through all tiles in movement range
        foreach (Vector3Int tile in tilesInMovementRange)
        {
            // Get the enemy's tile position
            Vector3Int enemyTilePosition = moveRangeTilemap.WorldToCell(targetStats.characterGameObject.transform.position);

            // If the tile is occupied or is the tile enemy is standing on, skip it
            if (gameManager.IsTileOccupied(tile) || tile == enemyTilePosition)
                continue;

            float distance = Vector3.Distance(moveRangeTilemap.GetCellCenterWorld(tile), targetPosition);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestTile = tile;
            }

            // We yield return null to avoid freezing if there are many tiles
            yield return null;
        }


        Debug.Log("Best tile calculated by coroutine: " + bestTile);

        // Store the best situation for later retrieval
        this.bestSituation = new Situation(shortestDistance, targetName, bestTile, true);
        HighlightBestTile();
        Debug.Log("CalculateBestSituationCoroutine() has finished execution.");
    }

    // Start the coroutine to calculate and move to the best situation.
    public void StartCalculateAndMoveCoroutine(GameObject unit)
    {
        StartCoroutine(CalculateAndMoveCoroutine(unit));
    }

    // Coroutine that calculates the best situation and then initiates movement.
    private IEnumerator CalculateAndMoveCoroutine(GameObject unit)
    {
        yield return StartCoroutine(CalculateBestSituationCoroutine());

        EnemyMover enemymover = unit.GetComponent<EnemyMover>();
        if (enemymover != null)
        {
            enemymover.MoveToBestTile();
        }
        Debug.Log("Moving unit to position: " + moveRangeTilemap.GetCellCenterWorld(bestSituation.targetTile));
    }

    // Calculate the best situation for AI attack based on enemy positions.
    public IEnumerator CalculateBestSituationCoroutineForAttack()
    {
        EveryonesStats statsManager = FindObjectOfType<EveryonesStats>();
        CharacterStats thisCharacterStats = statsManager.GetCharacterStats(this.gameObject.name);

        // Get the character stats from the stats manager
        string targetName = FindLowestDefenceFriendly();


        // Check if the character is taunted
        if (thisCharacterStats.isTaunted)
        {
            Debug.Log("Character " + this.gameObject.name + " is taunted by " + thisCharacterStats.tauntedBy.characterName);
            targetName = thisCharacterStats.tauntedBy.characterName;
        }
        else
        {
            Debug.Log("Character " + this.gameObject.name + " is not taunted");
        }

        CharacterStats targetStats = statsManager.GetCharacterStats(targetName);

        if (targetStats == null || targetStats.characterGameObject == null)
        {
            Debug.Log("Target '" + targetName + "' not found");
            yield break;
        }

        Vector3 targetPosition = targetStats.characterGameObject.transform.position;

        movementScript.ShowMoveRange();

        // Get all the reachable tiles
        List<Vector3Int> tilesInMovementRange = movementScript.highlightedTiles;

        Vector3Int bestTile = new Vector3Int();
        // For Archer
        float longestDistance = float.MinValue;
        // For Warrior
        float shortestDistance = float.MaxValue;
        bool canAttackAfterMoving = false;

        // Loop through all tiles in movement range to find the closest to the enemy
        foreach (Vector3Int tile in tilesInMovementRange)
        {
            // Get the enemy's tile position
            Vector3Int enemyTilePosition = moveRangeTilemap.WorldToCell(targetStats.characterGameObject.transform.position);

            if (gameManager.IsTileOccupied(tile) || tile == enemyTilePosition)
                continue;

            // This simulates the character moving to the tile and then checking the attack range
            HashSet<Vector3Int> reachableAttackTiles = attackScript.ShowAttackRange(tile);

            float distance = Vector3.Distance(moveRangeTilemap.GetCellCenterWorld(tile), targetPosition);
            // Check if the enemy is within attack range after moving to this tile
            bool canAttackFromTile = reachableAttackTiles.Contains(enemyTilePosition);


            // Update the best tile and shortest distance if this tile is closer and allows the character to attack
            if (distance > longestDistance && canAttackFromTile)
            {
                // Shortest distance will be used maybe on the warrior?
                // It could also be used on the Archer depending on the situation
                // if we want the archer to be able to be more aggressive and pull
                // Off more skills that are close range then the use of Shortest distance
                // would be more appropriate.

                // shortestDistance = distance;
                // For the Archer
                longestDistance = distance;

                bestTile = tile;
                // Update this to true because the character can attack after moving to the best tile
                canAttackAfterMoving = true;
            }

            // We yield return null to avoid freezing if there are many tiles
            yield return null;
        }

        // Check if no enemy is within attack range after moving
        if (!canAttackAfterMoving)
        {
            Debug.Log("Using regular situation calculation...");

            // If no enemy is within attack range after moving, use the regular best situation calculation
            yield return StartCoroutine(CalculateBestSituationCoroutine());
        }
        else
        {
            Debug.Log("Using attack situation calculation...");

            // Otherwise, store the best situation for later retrieval
            this.bestSituation = new Situation(shortestDistance, targetName, bestTile, canAttackAfterMoving);
            HighlightAttackBestTile();
        }
    }

    // Start the coroutine to calculate and move to the best attack position.
    public void StartCalculateAndAttackMoveCoroutine(GameObject unit)
    {
        StartCoroutine(CalculateAndMoveAttackCoroutine(unit));
    }

    // Coroutine that calculates the best situation for attack
    private IEnumerator CalculateAndMoveAttackCoroutine(GameObject unit)
    {
        yield return StartCoroutine(CalculateBestSituationCoroutineForAttack());

        EnemyMover enemymover = unit.GetComponent<EnemyMover>();
        if (enemymover != null)
        {
            enemymover.MoveToBestTile();
        }
        Debug.Log("Moving unit to position: " + moveRangeTilemap.GetCellCenterWorld(bestSituation.targetTile));
    }

    // Determines if AI can move to a preferred tile (e.g., for flanking).
    public bool CanMoveToPreferredTile()
    {
        Debug.Log("CanMoveToPreferredTile called");

        // First, check if any friendly can be flanked
        //string targetName = CanFlankFriendly();
        // If no flank-able friendly was found, revert to the lowest defense target
        //if (targetName == null)
        //{
        //    targetName = FindLowestDefenceFriendly();
        //}
        string targetName = FindLowestDefenceFriendly();

        // Get the character stats from the stats manager
        // string targetName = FindLowestDefenceFriendly();
        Debug.Log("Target Name: " + targetName);

        EveryonesStats statsManager = FindObjectOfType<EveryonesStats>();
        CharacterStats targetStats = statsManager.GetCharacterStats(targetName);

        if (targetStats == null || targetStats.characterGameObject == null)
        {
            Debug.Log("Target '" + targetName + "' not found");
            return false;
        }

        Vector3 targetPosition = targetStats.characterGameObject.transform.position;
        movementScript.ShowMoveRange();

        List<Vector3Int> tilesInMovementRange = movementScript.highlightedTiles;

        // Define the preferred direction as a Vector3Int based on the enemy's facing direction
        Vector3Int bestTile = Vector3Int.zero;
        switch (targetStats.facingDirection)
        {
            case Direction.Up:
                bestTile = new Vector3Int(-1, 0, 0);
                break;
            case Direction.Down:
                bestTile = new Vector3Int(1, 0, 0);
                break;
            case Direction.Left:
                bestTile = new Vector3Int(0, -1, 0);
                break;
            case Direction.Right:
                bestTile = new Vector3Int(0, 1, 0);
                break;
        }

        bestTile += moveRangeTilemap.WorldToCell(targetPosition);
        Debug.Log("Calculated Best Tile Position: " + bestTile);
        Debug.Log("Is Best Tile Occupied: " + gameManager.IsTileOccupied(bestTile));
        Debug.Log("Is Best Tile in Movement Range: " + tilesInMovementRange.Contains(bestTile));

        Vector3 currentPos = this.gameObject.transform.position;
        Vector3Int currentTile = moveRangeTilemap.WorldToCell(currentPos);
        if (currentTile == bestTile)
        {
            Debug.Log("Character already at the preferred flank tile.");
            // No need to move
            return false; 
        }

        // Check if preferred tile is in movement range and not occupied
        if (tilesInMovementRange.Contains(bestTile) && !gameManager.IsTileOccupied(bestTile))
        {
            Debug.Log("Best tile is within movement range and is unoccupied.");
            bestSituation = new Situation(0, targetName, bestTile, true);
            HighlighFlankBestTile();
            return true;
        }

        Debug.Log("Best tile isn't in range or is occupied.");
        return false;
    }



    public void StartCalculateAndMoveToPreferredTile(GameObject unit)
    {
        if (CanMoveToPreferredTile())
        {
            EnemyMover enemymover = unit.GetComponent<EnemyMover>();
            if (enemymover != null)
            {
                enemymover.MoveToBestTile();
            }
            Debug.Log("Moving unit to position: " + moveRangeTilemap.GetCellCenterWorld(BestTile));
        }
    }
    // This function Highlights the best flank tile.
    public void HighlighFlankBestTile()
    {
        // Clear the previous best tile
        moveRangeTilemap.SetTile(bestTilePosition, null);

        // Get the best situation and highlight the tile
        if (bestSituation != null && bestSituation.targetTile != null)
        {
            bestTilePosition = bestSituation.targetTile;
            moveRangeTilemap.SetTile(bestTilePosition, bestFlankMarker);
        }
    }

    // This function highlights the best attack tile
    public void HighlightAttackBestTile()
    {
        // Clear the previous best tile
        moveRangeTilemap.SetTile(bestTilePosition, null);

        // Get the best situation and highlight the tile
        if (bestSituation != null && bestSituation.targetTile != null)
        {
            bestTilePosition = bestSituation.targetTile;
            moveRangeTilemap.SetTile(bestTilePosition, bestAttacMarker);
        }
    }


    void Update()
    {
        // HighlightCharacterTiles();
        // If the "L" key is pressed
        if (Input.GetKeyDown(KeyCode.L))
        {
            EveryonesStats statsManager = FindObjectOfType<EveryonesStats>();
            CharacterStats selfStats = statsManager.GetCharacterStats(gameObject.name);
            if (selfStats.hp <= 10f)
            {
                // Start the retreat coroutine
                StartCoroutine(RetreatIfHealthLowCoroutine());
            }
            else
            {
                // Otherwise, start the coroutine to calculate the best situation
                // StartCoroutine(CalculateBestSituationCoroutine());
            }
            // Call the coroutine to calculate the best situation
            //StartCoroutine(CalculateBestSituationCoroutine());
        }
        // If the "P" key is pressed
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("A key pressed.");
            // Call the method to print occupied tiles
            // AI.ChooseBestAction1(this, gameObject);
             AI.ChooseBestUtilityAction(this, gameObject);
       
            //      StartCoroutine(CalculateBestSituationCoroutineByFacing());
            // AI.ChooseBestMovementAction(this, gameObject);
            //if (AI.bestAction != null)
            //{
            //    Debug.Log("Chosen action: " + AI.bestAction.GetType().Name);
            //}

            //if (AI.bestAction == null)
            //{
            //    Debug.Log("Action is null");
            //}
        }


        // If the "P" key is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed.");
            StartCoroutine(CalculateBestSituationCoroutineForAttack());

            // Call the method to print occupied tiles
            //  PrintOccupiedTiles();
        }
    }

    public void HighlightBestTile()
    {
        // Clear the previous best tile
        moveRangeTilemap.SetTile(bestTilePosition, null);

        // Get the best situation and highlight the tile
        if (bestSituation != null && bestSituation.targetTile != null)
        {
            bestTilePosition = bestSituation.targetTile;
            moveRangeTilemap.SetTile(bestTilePosition, bestTileMarker);
        }
    }

    // This function finds vector position of the best tile
    public Vector3Int BestTile
    {
        get
        {
            if (bestSituation != null)
                return bestSituation.targetTile;
            else
                // Return an empty Vector3Int if no best situation has been calculated
                return new Vector3Int();  
        }
    }

    // Finds the lowest defence of a friendly unit
    public string FindLowestDefenceFriendly()
    {
        // Get the stats manager to access the stats of all characters
        EveryonesStats statsManager = FindObjectOfType<EveryonesStats>();

        string weakestFriendly = null;

        float lowestDefence = float.MaxValue;

        GameObject[] friendlyCharacters = GameObject.FindGameObjectsWithTag("Friendly");

        List<Vector3Int> tilesInMovementRange = movementScript.highlightedTiles;

        // For each friendly character
        foreach (GameObject character in friendlyCharacters)
        {
            // Convert the friendly's position to a tile position
            Vector3Int tilePosition = moveRangeTilemap.WorldToCell(character.transform.position);

            if (tilesInMovementRange.Contains(tilePosition))
            {
                CharacterStats characterStats = statsManager.GetCharacterStats(character.name);

                // If the character's defense is lower than the lowest found so far
                if (characterStats.defence < lowestDefence)
                {
                    // This character has the lowest defense found so far
                    lowestDefence = characterStats.defence;

                    // Store the name of this character
                    weakestFriendly = character.name;
                }
            }
        }

        // If no character was found within move range, just return the weakest overall
        if (weakestFriendly == null)
        {
            foreach (GameObject character in friendlyCharacters)
            {
                CharacterStats characterStats = statsManager.GetCharacterStats(character.name);
                if (characterStats.defence < lowestDefence)
                {
                    weakestFriendly = character.name;
                }
            }
        }

        // Return the name of the friendly character with the lowest defense
        return weakestFriendly;
    }


    public IEnumerator RetreatIfHealthLowCoroutine()
    {
        // Get the character stats from the stats manager
        EveryonesStats statsManager = FindObjectOfType<EveryonesStats>();
        CharacterStats selfStats = statsManager.GetCharacterStats(gameObject.name);

        string enemyName = FindLowestDefenceFriendly(); 
        CharacterStats enemyStats = statsManager.GetCharacterStats(enemyName);

        // If the enemy stats or game object doesn't exist, don't run the coroutine
        if (enemyStats == null || enemyStats.characterGameObject == null)
        {
            Debug.Log("Enemy '" + enemyName + "' not found");
            yield break;
        }

        Vector3 enemyPosition = enemyStats.characterGameObject.transform.position;

        movementScript.ShowMoveRange();

        List<Vector3Int> tilesInMovementRange = movementScript.highlightedTiles;

        // Store the distances and tiles in a list of tuples
        List<(float distance, Vector3Int tile)> distancesAndTiles = new List<(float distance, Vector3Int tile)>();

        // Loop through all tiles in movement range to find the furthest from the enemy
        foreach (Vector3Int tile in tilesInMovementRange)
        {
            // If the tile is occupied, skip it
            if (gameManager.IsTileOccupied(tile))
                continue;

            float distance = Vector3.Distance(moveRangeTilemap.GetCellCenterWorld(tile), enemyPosition);
            distancesAndTiles.Add((distance, tile));

            // We yield return null to avoid freezing if there are many tiles
            yield return null;
        }

        // Sort the list by distance in descending order, then take the top N
        int N = 2; 

        var topTiles = distancesAndTiles.OrderByDescending(tuple => tuple.distance).Take(N).ToList();

        // Randomly select one of the top tiles
        var selectedTile = topTiles[Random.Range(0, topTiles.Count)];

        // Store the best retreat situation for later retrieval
        this.bestSituation = new Situation(selectedTile.distance, enemyName, selectedTile.tile, false);  
        HighlightBestTile();
    }


    public void StartCalculateRetreatMovement(GameObject unit)
    {
        StartCoroutine(StartCalculateRetreatMovementCoroutine(unit));
    }

    private IEnumerator StartCalculateRetreatMovementCoroutine(GameObject unit)
    {
        yield return StartCoroutine(RetreatIfHealthLowCoroutine());

        EnemyMover enemymover = unit.GetComponent<EnemyMover>();
        if (enemymover != null)
        {
            enemymover.MoveToBestTile();
        }
        Debug.Log("Moving unit to position: " + moveRangeTilemap.GetCellCenterWorld(bestSituation.targetTile));
    }


}