using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "AI/FlankEnemyAction")]

public class FlankEnemyAction : AIAction
{
    public Astar astar;
    private void OnEnable()
    {
        actionType = ActionType.Movement;
        Debug.Log("FlankEnemyAction initialized. ActionType: " + actionType);
    
    }
    public override int GetPriorityLevel(GameObject unit)
    {
        CharacterStats characterStats = unit.GetComponent<CharacterStats>();

        switch (characterStats.characterClass)
        {
            case CharacterClass.Warrior:
                return 3;
            case CharacterClass.Archer:
                return 2;
            default:
                return 1;
        }
    }
    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        GameManager gameManager = situationGrabber.gameManager; 

        float baseUtility = 1.0f;

        // Get the character stats
        CharacterStats characterStats = unit.GetComponent<CharacterStats>();
        float attackRange = characterStats.attackRange;
        float moveRange = characterStats.moveRange;

        // Check if there is an enemy within attack range, within move range, and within double move range.
        float enemyProximityUtility = 0.0f;

        foreach (KeyValuePair<Vector3Int, CharacterStats> occupiedTile in gameManager.occupiedTiles)
        {
            if (occupiedTile.Value.type == CharacterType.Friendly)
            {
                float distance = Vector3.Distance(occupiedTile.Value.characterGameObject.transform.position, unit.transform.position);
                Debug.Log("Distance to enemy " + occupiedTile.Value.characterName + ": " + distance);

                if (distance <= attackRange)
                {
                    enemyProximityUtility = 1000f; // Set very high utility if enemy is within attack range
                    Debug.Log("Enemy within attack range, utility set to " + enemyProximityUtility);
                    break;
                }
                else if (distance <= moveRange)
                {
                    enemyProximityUtility = 500f;  // Set high utility if enemy is within move range
                    Debug.Log("Enemy within move range, utility set to " + enemyProximityUtility);
                }
                else if (distance <= 2 * moveRange)
                {
                    enemyProximityUtility = 100f;  // Set medium utility if enemy is within double move range
                    Debug.Log("Enemy within double move range, utility set to " + enemyProximityUtility);
                }
                else
                {
                    enemyProximityUtility = 10f;   // Set low utility otherwise
                    Debug.Log("Enemy outside double move range, utility set to " + enemyProximityUtility);
                }
            }
        }

        // Bonus utility for flanking: if there's an enemy within move range, give a high bonus
        float flankingUtility = 0.0f;
        if (enemyProximityUtility >= 500f)
        {
            flankingUtility = 500f;
            Debug.Log("Potential for flanking, utility set to " + flankingUtility);
        }

        float totalUtility = baseUtility + enemyProximityUtility + flankingUtility;
        Debug.Log("Total utility for unit " + unit.name + ": " + totalUtility);

        return totalUtility + 1000;
    }


    public Vector3Int GetFlankTile(GameObject target, AISituationGrabber situationGrabber)
    {
        CharacterStats targetStats = target.GetComponent<CharacterStats>();

        Vector3Int preferredTileNextToTarget = Vector3Int.zero;
        switch (targetStats.facingDirection)
        {
            case Direction.Up:
                preferredTileNextToTarget = new Vector3Int(0, 1, -2);  // prefer the tile behind the enemy
                break;
            case Direction.Down:
                preferredTileNextToTarget = new Vector3Int(1, 0, 0);  // prefer the tile to the above of the enemy
                break;
            case Direction.Left:
                preferredTileNextToTarget = new Vector3Int(0, -1, 0);  // prefer the tile to the right of the enemy
                break;
            case Direction.Right:
                preferredTileNextToTarget = new Vector3Int(0, 1, 0);  // prefer the tile left of the enemy
                break;
        }

        preferredTileNextToTarget += situationGrabber.moveRangeTilemap.WorldToCell(target.transform.position);

        return preferredTileNextToTarget;
    }

    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        Astar astar = Astar.Instance; // Get the Astar instance

        // Debugging
        if (unit == null)
        {
            Debug.Log("Unit is null");
            return;
        }
        if (situationGrabber == null)
        {
            Debug.Log("AISituationGrabber is null");
            return;
        }
        if (astar == null)
        {
            Debug.Log("Astar is null");
            return;
        }

        // Get the moveRange from the unit's CharacterStats
        CharacterStats characterStats = unit.GetComponent<CharacterStats>();
        int moveRange = characterStats.moveRange;

        // Get the set of tiles reachable within moveRange from the unit's current position
        Vector3Int currentPos = Vector3Int.FloorToInt(unit.transform.position);
        HashSet<Vector3Int> reachableTiles = astar.GetReachableTiles(currentPos, moveRange);

        // Start the coroutine to calculate the best situation
        //situationGrabber.StartCoroutine(situationGrabber.CalculateBestSituationCoroutineByFacing());

        // Wait until the coroutine has completed
        while (situationGrabber.bestSituation == null)
        {
        }

        // Get the flank tile from the best situation

        EnemyMover enemyMover = unit.GetComponent<EnemyMover>();
        if (enemyMover != null)
           {
                enemyMover.MoveToBestTile();
            }
        }
    }




