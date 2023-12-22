/*
    The CharacterStats class is responsible for managing the individual
    attributes and statistics  of each character in the game. 
    This class acts as a container for all information related to an individual
    character in the game, enabling easy access and manipulation of
    their stats, status, and behavior.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
[System.Serializable]
public class CharacterStats : MonoBehaviour
{
    // ----- Section: Character Identifiers -----
    public string characterName;
    public CharacterType type;

    // ----- Section: Character Attributes -----
    public float hp = 100f;
    public float maximumhp = 100f;
    public float mp = 100f;
    public float attack = 10f;
    public float defence = 10f;
    public float magicAttack = 10f;
    public float magicDefence = 10f;
    public float speed = 5f;
    public float energy = 0f;
    public int actionPoints; // For Movement
    public float accuracy = 100f;
    public float evasion = 10f;
    public float luck = 5f;

    // ----- Section: Equipment & Personality -----
    public List<Armour> currentEquipment;
    public List<Armour> equippedArmour;
    public Personality personality;

    // ----- Section: Game State Variables -----
    public Phase phase;
    public GameObject characterGameObject;
    public Direction facingDirection;
    public int moveRange = 3;
    public int attackRange = 1;
    public int extendedAttackRange;
    [HideInInspector] 
    public Sprite originalSprite;
    public Sprite deadSprite;
    public CharacterClass characterClass;
    public ElementalProperties elementalProperties;
    public WeaponType weaponType;
    public bool isTaunted = false;
    public CharacterStats tauntedBy;
    
    public delegate void CharacterTurnHandler(bool isTurn);
    public event CharacterTurnHandler CharacterTurnChanged;

    public bool isCharacterTurn;

    public int turnsTaken = 0;

    // ----- Section: Skills -----
    public List<Skill> characterSkills = new List<Skill>();
    
    


    // ----- Freeze Mechanics -----
    [SerializeField]
    // Track how many turns the character is frozen
    private int _turnsFrozen = 0;  
    public TileBase frozenTile; 
    public Tilemap characterTilemap;
    public int turnsFrozen
    {
        get { return _turnsFrozen; }
        set
        {
            _turnsFrozen = value;

            // If the turnsFrozen counter reaches zero, unfreeze the character
            if (_turnsFrozen <= 0)
            {
                _turnsFrozen = 0;  // Ensure it doesn't go negative
                UnfreezeCharacter();
            }
        }
    }

    public bool IsFrozen { get; private set; } = false;
    public void FreezeCharacter()
    {
        IsFrozen = true;

        Vector3Int currentPos = characterTilemap.WorldToCell(transform.position);
        characterTilemap.SetTile(currentPos, frozenTile);
    }

    public void UnfreezeCharacter()
    {
        IsFrozen = false;
        Vector3Int currentPos = characterTilemap.WorldToCell(transform.position);
        characterTilemap.SetTile(currentPos, null);
    }


    // ----- HP Management and Death Mechanic -----

    public delegate void HPChangeHandler();
    public event HPChangeHandler OnHPChanged;

    [SerializeField]
    public bool _isDead;


    public bool IsDead
    {
        get { return _isDead; }
        private set { _isDead = value; }
    }

    public void ChangeHP(float amount)
    {
        Debug.Log(characterName + " HP Changed: " + amount);

        hp += amount;
        if (hp > maximumhp) hp = maximumhp;
        if (hp <= 0)
        {
            hp = 0;
            IsDead = true;
            // PlayDeadAnimation();
            // DisplayDeathMessage();
        }
        else
        {
            IsDead = false;
        }
        // Notify subscribers 
        OnHPChanged?.Invoke();  
    }



    public void SetCharacterTurn(bool isTurn)
    {
        isCharacterTurn = isTurn;
        CharacterTurnChanged?.Invoke(isTurn);
    }



    void Awake()
    {
        SpriteRenderer spriteRenderer = characterGameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        characterName = gameObject.name;
        // Register this character's stats with the manager
        EveryonesStats.Instance.RegisterCharacterStats(this);
        elementalProperties = new ElementalProperties();
        InitializeElementalProperties(); 
    }

    public void AddDefence()
    {
        defence += 50;
        Debug.Log(characterName + "'s defence increased to: " + defence);
    }  



    void InitializeElementalProperties()
    {
        switch (characterClass)
        {
            case CharacterClass.Warrior:
                elementalProperties.SetAffinity(ElementType.Fire, 100);
                elementalProperties.SetAffinity(ElementType.Water, -100);
                break;
            case CharacterClass.Archer:
                elementalProperties.SetAffinity(ElementType.Air, 100);
                elementalProperties.SetAffinity(ElementType.Earth, -100);
                break;
        }
    }

    public void ResetEnergy()
    {
        energy = 0f;
    }



    public void IncreaseMoveRange()
    {
        moveRange++;
    }

    public void DecreaseMoveRange()
    {
        if (moveRange > 1) 
        {
            moveRange--;
        }
        else
        {
            Debug.Log(characterName + " move range is already at minimum!");
        }
    }


    public void FacingDirectionUp()
    {
        moveRange++;
    }

}

// ----- Section: Enums -----
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum CharacterType
{
    Friendly,
    Enemy
}

public enum Personality
{
    Aggressive,
    Defensive,
    Neutral
}

public enum Phase
{
    Movement,
    Action,
    End
}

public enum CharacterClass
{
    Warrior,
    Archer,
    Mage,
 }

public enum ElementType
{
    Fire,
    Water,
    Earth,
    Air,
    Light,
    Dark
}

public enum WeaponType
{
    Sword,
    Bow,
    Axe,
    Spear
}



