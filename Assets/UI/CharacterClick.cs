using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    The CharacterClick class is responsible for handling mouse click interactions 
    on character game objects within the game. When a character is clicked, it 
    fetches and displays the stats of the character using the DisplayStats class.
*/
public class CharacterClick : MonoBehaviour
{
    public DisplayStats displayStats;

    void Start()
    {
        displayStats = GameObject.FindObjectOfType<DisplayStats>();
    }

    void OnMouseDown()
    {
        displayStats.ShowStatsByGameObject(gameObject);
    }
}
