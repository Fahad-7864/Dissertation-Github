using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    This class plays a pivotal role in managing and maintaining the characters
    and their statistics, and providing an organized interface for 
    other parts of the game to access and manipulate these data.

    The EveryonesStats class is a singleton that manages the statistics of all 
    characters in a game. It tracks, categorizes (friendly or enemy), and 
    provides access to these statistics. 

*/
[System.Serializable]

public class EveryonesStats : MonoBehaviour
{
    public static EveryonesStats Instance { get; private set; }

    // Lists to hold different types of characters stats
    public List<CharacterStats> allCharacterStats;
    public List<CharacterStats> friendlyCharacterStats = new List<CharacterStats>();
    public List<CharacterStats> enemyCharacterStats = new List<CharacterStats>();

    void Awake()
    {
        // Ensure only one instance of this class exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("More than one instance of EveryonesStats detected. Destroying this one.");
            Destroy(this);
        }
    }

    // Register a new character and divide them by their type
    public void RegisterCharacterStats(CharacterStats characterStats)
    {
        if (!allCharacterStats.Exists(x => x.characterName == characterStats.characterName))
        {
            allCharacterStats.Add(characterStats);
            DivideCharactersByType();
        }
        else
        {
            Debug.LogWarning("Character " + characterStats.characterName + " has already been registered.");
        }
    }

    // Sort characters into friendly and enemy lists

    private void DivideCharactersByType()
    {
        foreach (CharacterStats characterStats in allCharacterStats)
        {
            if (characterStats.type == CharacterType.Friendly && !friendlyCharacterStats.Exists(x => x.characterName == characterStats.characterName))
            {
                friendlyCharacterStats.Add(characterStats);
            }
            else if (characterStats.type == CharacterType.Enemy && !enemyCharacterStats.Exists(x => x.characterName == characterStats.characterName))
            {
                enemyCharacterStats.Add(characterStats);
            }
        }
    }



    // Return the total count of all characters

    public int GetCharacterCount()
    {
        return allCharacterStats.Count;
    }


    // Add a new character and divide them by their type
    public void AddCharacterStats(CharacterStats newCharacterStats)
    {
        allCharacterStats.Add(newCharacterStats);
        DivideCharactersByType();

    }


    // Get the stats of a specific character by their name
    public CharacterStats GetCharacterStats(string characterName)
    {
        foreach (CharacterStats stats in allCharacterStats)
        {
            if (stats.characterName == characterName)
            {
                return stats;
            }
        }

        return null;
    }

    // Not using this function right now
    // Link each character's stats to their corresponding game object
    private void InitializeCharacters()
    {
        foreach (CharacterStats characterStats in allCharacterStats)
        {
            GameObject characterGameObject = GameObject.Find(characterStats.characterName);
            if (characterGameObject != null)
            {
                characterStats.characterGameObject = characterGameObject;
            }
            else
            {
                Debug.LogWarning("No GameObject found with name: " + characterStats.characterName);
            }
        }
    }



}

