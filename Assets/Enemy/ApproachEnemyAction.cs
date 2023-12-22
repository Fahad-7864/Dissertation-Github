using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "AI/ApproachEnemyAction")]

public class ApproachEnemyAction: AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Movement;
    }

    public int priorityLevel = 2;
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

        float totalUtility = baseUtility + enemyProximityUtility;
        Debug.Log("Total utility for unit " + unit.name + ": " + totalUtility);

        return totalUtility+10000;
    }


    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        situationGrabber.StartCalculateAndMoveCoroutine(unit);
    }
}