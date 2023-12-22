using UnityEngine;

[CreateAssetMenu(menuName = "AI/MovementAction")]
public class MovementAction : AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Movement;
    }
    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        // Calculate the utility based on the situation and other relevant factors
        Vector3Int bestTile = situationGrabber.BestTile;
        Vector3 unitPosition = unit.transform.position;
        Vector3Int unitTile = new Vector3Int((int)unitPosition.x, (int)unitPosition.y, (int)unitPosition.z);
        float distanceToTarget = Vector3Int.Distance(unitTile, bestTile);
        float weightFactor = 0.5f; 
        float utility = weightFactor / distanceToTarget;

        return utility;
    }



    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        // Get the best tile from the situation Grabber
        Vector3Int bestTile = situationGrabber.BestTile;

        // Move the unit to the best tile
        EnemyMover enemymover = unit.GetComponent<EnemyMover>();
        if (enemymover != null)
        {
            enemymover.MoveToBestTile();
        }
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
}