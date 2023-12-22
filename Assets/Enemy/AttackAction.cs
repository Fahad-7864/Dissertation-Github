using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/AttackAction")]
public class AttackAction : AIAction
{

    private void OnEnable()
    {
        actionType = ActionType.Utility;
    }

    public override float EvaluateUtility(GameObject unit, AISituationGrabber situationGrabber)
    {
        EveryonesStats statsManager = Object.FindObjectOfType<EveryonesStats>();
        Attack attackComponent = unit.GetComponent<Attack>();

        if (statsManager == null) Debug.Log("StatsManager is null!");
        if (attackComponent == null) Debug.Log("Attack component not found on the unit!");

        // Set the initial utility to 0
        float utility = 0f;

        if (attackComponent != null)
        {
            Debug.Log("Evaluating utility for unit: " + unit.name); 

              
            // Get the name of the weakest enemy within attack range
            string weakestEnemyName = attackComponent.GetWeakestFriendly();

            if (weakestEnemyName != null)
            {
                Debug.Log("Weakest enemy found: " + weakestEnemyName); 
                utility = 0.9f;
            }
            else
            {
                Debug.Log("No weakest enemy found within attack range.");
            }
        }
        else
        {
            Debug.Log("No attack component found on the unit.");
        }

        // Log the utility value
        Debug.Log("Returning utility value: " + utility); 

        return utility;
    }



    public override void Execute(GameObject unit, AISituationGrabber situationGrabber)
    {
        // Get the attack script from the unit
        Attack attack = unit.GetComponent<Attack>();

        if (attack != null)
        {
            string weakestEnemy = attack.GetWeakestFriendly(); 

            // Perform the attack
            if (weakestEnemy != null)
            {
                attack.isInAttackMode = true;
                // Attack the weakest enemy

                attack.AttackEnemy(weakestEnemy); 

            }
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


