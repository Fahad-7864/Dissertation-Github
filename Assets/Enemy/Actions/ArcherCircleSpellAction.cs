using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/ArcherCircleSpellAction")]
public class ArcherCircleSpellAction : AIAction
{
    private void OnEnable()
    {
        actionType = ActionType.Utility;
    }

    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        ArcherCircleSpell archerCircleSpell = unit.GetComponent<ArcherCircleSpell>();

        if (archerCircleSpell != null)
        {
            Dictionary<string, List<Vector3Int>> enemyTiles = archerCircleSpell.FindEnemiesOnTiles();
            float maxUtility = 0; 

            foreach (var pos in archerCircleSpell.spellTilemap.cellBounds.allPositionsWithin)
            {
                if (archerCircleSpell.spellTilemap.GetTile(pos) == null)
                    continue;

                // Generate AoE based on the position
                List<Vector3Int> aoeTiles = archerCircleSpell.ShowSpellAoE(pos);

                // Check for enemies on each tile in the generated AoE
                float utility = 0;
                foreach (var aoePos in aoeTiles)
                {
                    if (enemyTiles["spellAoETilemap"].Contains(aoePos))  // Check if AoE hits an enemy
                    {
                        utility += 100000f; // Assigning a high utility value for each enemy hit.
                        Debug.Log("Enemy detected at position: " + aoePos);
                    }
                    else
                    {
                        Debug.Log("No enemy detected at position: " + aoePos);
                    }
                }

                // Update maxUtility if this position's utility is higher
                if (utility > maxUtility)
                {
                    maxUtility = utility;
                }
            }

            return maxUtility;
        }
        else
        {
            Debug.LogError("ArcherCircleSpell component not found on unit!");
            return 0f;
        }
    }


    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        ArcherCircleSpell archerCircleSpell = unit.GetComponent<ArcherCircleSpell>();

        if (archerCircleSpell != null)
        {
            archerCircleSpell.EnterSpellCastingMode();
            archerCircleSpell.AutoCastSpell(archerCircleSpell.FindEnemiesOnTiles());
            //archerCircleSpell.ExitSpellCastingMode();
        }
        else
        {
            Debug.LogError("ArcherCircleSpell component not found on unit!");
        }
    }


    public override int GetPriorityLevel(GameObject unit)
    {
        CharacterStats characterStats = unit.GetComponent<CharacterStats>();

        // For an archer, using an AOE spell is a high priority action when it can hit multiple enemies
        if (characterStats.characterClass == CharacterClass.Archer)
        {
            return 1;
        }

        return 1;
    }
}
