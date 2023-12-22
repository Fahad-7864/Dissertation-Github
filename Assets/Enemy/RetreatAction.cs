using UnityEngine;

[CreateAssetMenu(menuName = "AI/RetreatAction")]
public class RetreatAction : AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Movement;
    }
    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        EveryonesStats statsManager = Object.FindObjectOfType<EveryonesStats>();
        CharacterStats selfStats = statsManager.GetCharacterStats(unit.name);

        // If the character's health is more than 50%, the utility of retreat is low
        // Otherwise, it's inversely proportional to health
        return selfStats.hp > 50f ? 10f : (100f - selfStats.hp);
    }

    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        situationGrabber.StartCalculateRetreatMovement(unit);
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

