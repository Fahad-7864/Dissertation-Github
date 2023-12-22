using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveButton : MonoBehaviour
{
    public TurnManager turnManager;

    public void OnMoveButtonClick()
    {
        // Get active character stats
        CharacterStats activeCharacterStats = turnManager.GetActiveCharacterStats();

        // Check if there is an active character and if it still has action points
        if (activeCharacterStats != null && activeCharacterStats.isCharacterTurn && activeCharacterStats.actionPoints > 0 && activeCharacterStats.type == CharacterType.Friendly)
        {
            HighlightandMovement characterHighlightAndMovement = activeCharacterStats.characterGameObject.GetComponent<HighlightandMovement>();

            // Call the ShowMoveRangeButton function
            characterHighlightAndMovement.ShowMoveRangeButton();

            //// Decrease action points
            //activeCharacterStats.actionPoints--;
        }
    }
}
