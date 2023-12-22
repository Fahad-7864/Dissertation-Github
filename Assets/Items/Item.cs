using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The Item class represents different types of usable items within the game.
    This class defines various attributes of an item, such as its type, effect,
    name, appearance, and how it impacts a character's statistics.
    Items are defined as ScriptableObjects for easy creation and management in the Unity Editor.
*/


public enum ItemType
{
    Potion,
    Food
}

public enum ItemEffect
{
    None,
    RestoreHP,
    RestoreMP,
    IncreaseAttack,
    IncreaseDefense,
    Revive,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    // ----- Section: Item Attributes -----
    public ItemType type;
    public ItemEffect effect;
    public string itemName;
    public Sprite itemSprite;
    public string description;
    public int effectValue;
    public float effectMultiplier = 1.0f;

    // ----- Section: Item Usage -----
    public void Use(CharacterStats stats)
    {
        int realEffectValue = Mathf.RoundToInt(effectValue * effectMultiplier);

        switch (effect)
        {
            case ItemEffect.RestoreHP:
                stats.hp = Mathf.Min(stats.hp + realEffectValue, 100f);
                Debug.Log($"Used {itemName}. Restored {realEffectValue} HP.");
                break;
            case ItemEffect.RestoreMP:
                stats.mp = Mathf.Min(stats.mp + realEffectValue, 100f);
                Debug.Log($"Used {itemName}. Restored {realEffectValue} MP.");
                break;
            case ItemEffect.IncreaseAttack:
                stats.attack += realEffectValue;
                Debug.Log($"Used {itemName}. Increased Attack by {realEffectValue}.");
                break;
            case ItemEffect.IncreaseDefense:
                stats.defence += realEffectValue;
                Debug.Log($"Used {itemName}. Increased Defense by {realEffectValue}.");
                break;
            case ItemEffect.Revive:
                if (stats.IsDead)
                {
                    stats.ChangeHP(realEffectValue);
                    Debug.Log($"Used {itemName}. Revived and restored {realEffectValue} HP.");
                }
                else
                {
                    Debug.Log($"{itemName} was used, but the character is already alive.");
                }
                break;

            default:
                Debug.Log($"Used {itemName}, but it had no effect.");
                break;

        }
    }

}
