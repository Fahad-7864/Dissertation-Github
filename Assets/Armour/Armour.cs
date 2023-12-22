using UnityEngine;
/*
    The Armour script defines individual armour pieces that can be equipped 
    by characters. This script sets the type, effect, visual representation, 
    and the specific benefits provided by the armour when equipped.
*/

// ----- Section: Enums -----
// Enum to categorize the different types of armour pieces.
public enum ArmourType
{
    Helmet,
    ChestPiece,
    LegPiece,
    Shield
}

// Enum to define the specific benefits or effects provided by the armour.
public enum ArmourEffect
{
    None,
    IncreaseHealth,
    IncreaseDefence,
    IncreaseMagicDefence,
}

[CreateAssetMenu(fileName = "New Armour", menuName = "Inventory/Armour")]
public class Armour : ScriptableObject
{
    // ----- Section: Armour Attributes -----
    public ArmourType type;
    public ArmourEffect effect;
    public string armourName;
    public Sprite armourSprite;
    public string description;
    public int effectValue;

    // ----- Section: Armour Methods -----

    // Method to equip the armour and apply its effect to the character's stats.
    public void Equip(CharacterStats stats)
    {
        switch (effect)
        {
            case ArmourEffect.IncreaseHealth:
                stats.hp += effectValue;
                Debug.Log($"Equipped {armourName} on {stats.characterName}. Increased HP by {effectValue}. New HP: {stats.hp}");
                break;
            case ArmourEffect.IncreaseDefence:
                stats.defence += effectValue;
                Debug.Log($"Equipped {armourName}. Increased Defence by {effectValue}. New Defence: {stats.defence}");
                break;
            case ArmourEffect.IncreaseMagicDefence:
                stats.magicDefence += effectValue;
                Debug.Log($"Equipped {armourName}. Increased Magic Defence by {effectValue}. New Magic Defence: {stats.magicDefence}");
                break;
            default:
                Debug.Log($"Equipped {armourName}, but it had no effect.");
                break;
        }
    }

    // Method to unequip the armour and remove its effect from the character's stats.
    public void Unequip(CharacterStats stats)
    {
        switch (effect)
        {
            case ArmourEffect.IncreaseHealth:
                stats.hp -= effectValue;
                Debug.Log($"Unequipped {armourName}. Decreased HP by {effectValue}. New HP: {stats.hp}");
                break;
            case ArmourEffect.IncreaseDefence:
                stats.defence -= effectValue;
                Debug.Log($"Unequipped {armourName}. Decreased Defence by {effectValue}. New Defence: {stats.defence}");
                break;
            case ArmourEffect.IncreaseMagicDefence:
                stats.magicDefence -= effectValue;
                Debug.Log($"Unequipped {armourName}. Decreased Magic Defence by {effectValue}. New Magic Defence: {stats.magicDefence}");
                break;
            default:
                Debug.Log($"Unequipped {armourName}, but it had no effect.");
                break;
        }
    }
}
