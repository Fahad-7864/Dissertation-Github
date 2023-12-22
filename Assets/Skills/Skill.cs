using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

/*
   Defines a Skill as a ScriptableObject with specific types and effects. 
   Skills modify character stats based on their type and are associated with a specific character class.
*/
public enum SkillType
{
    Attack,
    Defense,
    Heal
}

public enum SkillEffect
{
    Damage,
    BuffDefense,
    Heal
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill")]
public class Skill : ScriptableObject
{

    public SkillType type;
    public SkillEffect effect;
    public string skillName;
    public Sprite skillSprite;
    public string description;
    public int power; 
    public CharacterClass characterClass;
    public void Use(CharacterStats stats)
    {
        if(stats.characterClass != this.characterClass)
    {
        Debug.Log($"{stats.characterName} cannot use {skillName} because it is not a {this.characterClass}.");
        return;
    }
        switch (effect)
        {
            case SkillEffect.Damage:
                stats.hp = Mathf.Max(stats.hp - power, 0);
                Debug.Log($"{stats.characterName} used {skillName}. Dealt {power} damage.");
                break;
            case SkillEffect.BuffDefense:
                stats.defence += power;
                Debug.Log($"{stats.characterName} used {skillName}. Increased Defense by {power}.");
                break;
            case SkillEffect.Heal:
                stats.hp = Mathf.Min(stats.hp + power, stats.maximumhp);
                Debug.Log($"{stats.characterName} used {skillName}. Restored {power} HP.");
                break;
            default:
                Debug.Log($"{stats.characterName} used {skillName}, but it had no effect.");
                break;
        }
    }
}
