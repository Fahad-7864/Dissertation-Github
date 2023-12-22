using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "AI/AiFlankSituationGrabber")]

public class AiFlankSituationGrabber : AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Movement;
    }
    public override int GetPriorityLevel(GameObject unit)
    {
        CharacterStats characterStats = unit.GetComponent<CharacterStats>();

        switch (characterStats.characterClass) 
        {
            case CharacterClass.Warrior:
                return 1;
            case CharacterClass.Archer:
                return 2;
            default:
                return 1;
        }
    }

    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        if (situationGrabber.CanMoveToPreferredTile())
        {
            return 1f;
        }
        else
        {
            return 0f;
        }
    }

    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        situationGrabber.StartCalculateAndMoveToPreferredTile(unit);
    }
}

