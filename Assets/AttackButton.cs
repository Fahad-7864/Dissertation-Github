using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{
    public TurnManager turnManager;

    public void OnAttackButtonClick()
    {
        // Get active character stats
        CharacterStats activeCharacterStats = turnManager.GetActiveCharacterStats();

        // Check if there is an active character and if it still has action points
        if (activeCharacterStats != null && activeCharacterStats.isCharacterTurn && activeCharacterStats.energy > 0 && activeCharacterStats.type == CharacterType.Friendly)
        {
            Attack attack = activeCharacterStats.characterGameObject.GetComponent<Attack>();

            // Call the ShowAttackRange function or any appropriate function for the attack action
            attack.ShowAttackRange();

            //// Decrease action points
            //activeCharacterStats.actionPoints--;
        }
    }
}
