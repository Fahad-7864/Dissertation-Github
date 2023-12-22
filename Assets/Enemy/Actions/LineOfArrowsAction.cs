using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/LineOfArrowAction")]

public class LineOfArrowsAction : AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Utility;
    }

    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        LineOfArrows lineOfArrows = unit.GetComponent<LineOfArrows>();


        // Check if the skill is on cooldown
        if (lineOfArrows.cooldownCounter > 0)
        {
            return 0f; 
        }

        // Get the best direction to cast the spell
        Vector3Int bestDirection = lineOfArrows.GetBestSpellCastDirection(unit);

        // Score the spell cast direction
        float score = lineOfArrows.ScoreSpellCastDirection(unit, bestDirection,4);


        if (score > 10)
        {
            Debug.Log("Utility score is high enough to cast the spell. Score: " + score);
            return 1f;
        }
        else
        {
            Debug.Log("Utility score is not high enough to cast the spell. Score: " + score);
            return 0f;
        }

    }


    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        LineOfArrows lineOfArrows = unit.GetComponent<LineOfArrows>();

        if (lineOfArrows != null)
        {
            Debug.Log("LineOfArrows component found on unit");

           // lineOfArrows.isUpdatingLine = true;

    

            CharacterStats characterStats = unit.GetComponent<CharacterStats>();
            if (characterStats != null)
            {
                Debug.Log("CharacterStats component found on unit");

                if (characterStats.characterClass == CharacterClass.Archer)
                {
                    lineOfArrows.isUpdatingLine = true;

                    Debug.Log("Unit is an Archer");

                    // Calculate the best direction to cast the spell
                    Vector3Int spellCastDirection = lineOfArrows.GetBestSpellCastDirection(unit);

                    // Calculate the target position of the attack
                    Vector3Int targetTilePos = lineOfArrows.lineOfArrowsTilemap.WorldToCell(unit.transform.position) + spellCastDirection;

                    // Perform the attack
                    lineOfArrows.Attack(targetTilePos);

                    // After the attack, set confirmingAttack to false and stop updating the line
                    lineOfArrows.confirmingAttack = false;
                    lineOfArrows.isUpdatingLine = false;
                }
                else
                {
                    Debug.Log("Unit is not an Archer");
                }
            }
            else
            {
                Debug.Log("CharacterStats component not found on unit");
            }
        }
        else
        {
            Debug.Log("LineOfArrows component not found on unit");
        }
    }
    public override int GetPriorityLevel(GameObject unit)
    {
        CharacterStats characterStats = unit.GetComponent<CharacterStats>();

        // For an archer, using an AOE spell is a high priority action when it can hit multiple enemies
        if (characterStats.characterClass == CharacterClass.Archer)
        {
            return 2;
        }

        return 2;
    }
}
