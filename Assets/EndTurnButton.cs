using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public TurnManager turnManager; 

    public void OnEndTurnButtonClick()
    {
        // Get active character stats
        CharacterStats activeCharacterStats = turnManager.GetActiveCharacterStats();

        // Check if there is an active character
        if (activeCharacterStats != null && activeCharacterStats.isCharacterTurn)
        {
            // End the current character's turn
            Debug.Log(activeCharacterStats.characterName + "'s turn ended!");

            // Reset the active character's energy
            activeCharacterStats.ResetEnergy();

            activeCharacterStats.isCharacterTurn = false;
        }
    }
}
