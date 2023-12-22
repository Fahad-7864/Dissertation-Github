using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "AI/CalculateBestSituationAttackAction")]

public class CalculateBestSituationAttackAction : AIAction
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
                return 2;
            case CharacterClass.Archer:
                return 2;
            default:
                return 1;
        }
    }

    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
 
        return 1;
    }

    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        situationGrabber.StartCalculateAndAttackMoveCoroutine(unit);
    }

}


