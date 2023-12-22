using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "AI/DefendAction")]

public class DefendAction : AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Utility;
    }

    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        EveryonesStats statsManager = Object.FindObjectOfType<EveryonesStats>();
        CharacterStats selfStats = statsManager.GetCharacterStats(unit.name);

        // Check the health percentage of the AI
        float healthPercentage = selfStats.hp / selfStats.maximumhp;

        float baseUtility = healthPercentage < 0.6f ? 10f : 0.1f;

        float personalityModifier = 0;
        switch (selfStats.personality)
        {
            case Personality.Aggressive:
                personalityModifier = -2; // Aggressive units are less likely to defend
                break;
            case Personality.Defensive:
                personalityModifier = 2; // Defensive units are more likely to defend
                break;
            case Personality.Neutral:
                personalityModifier = 0; // Neutral units are neither more or less likely to defend
                break;
        }

        return baseUtility + personalityModifier;
    }


    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        EveryonesStats statsManager = Object.FindObjectOfType<EveryonesStats>();
        CharacterStats selfStats = statsManager.GetCharacterStats(unit.name);

        // Increase defense by 50%
        selfStats.defence *= 1.5f;

        // Log the defend action execution
        Debug.Log(unit.name + " is defending and increased their defense by 50%.");

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
