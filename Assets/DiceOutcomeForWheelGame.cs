using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DiceOutcomeForWheelGame : MonoBehaviour
{
    private int rollCount = 0;
    public ChatboxController chatboxController;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RollDiceButtonClicked(Action onCompleted)
    {
        rollCount = 0;
        StartCoroutine(RollDiceThreeTimes(onCompleted));
    }



    IEnumerator RollDiceThreeTimes(Action onCompleted)
    {
        while (rollCount < 3)
        {
            RollDice();
            yield return new WaitForSeconds(1f);
            rollCount++;
        }
        // Log the Persistent Data score after the three dice rolls
        Debug.Log("Persistent Data Score after three dice rolls: " + PersistentData.WheelGameScore);


        onCompleted?.Invoke(); // Call the callback function when completed
    }

    void RollDice()
    {
        int diceRoll = UnityEngine.Random.Range(1, 7);
        var outcome = diceOutcomes[diceRoll];
        int randomResult = UnityEngine.Random.Range(1, 101);
        int score = 0;
        string outcomeMessage = "";


        if (randomResult <= outcome.Item1)
        {
            outcomeMessage = "Good outcome";
            score = 5;
        }
        else if (randomResult <= outcome.Item1 + outcome.Item2)
        {
            outcomeMessage = "Bad outcome";
            score = 2;
        }
        else
        {
            outcomeMessage = "Perfect outcome";
            score = 10;
        }

        // Add the score to the persistent data class
        PersistentData.WheelGameScore += score;

        chatboxController.AddMessage($"Dice Roll Outcome: {outcomeMessage}. Scored: {score} points.");

    }



    private Dictionary<int, (int, int, int)> diceOutcomes = new Dictionary<int, (int, int, int)>
    {
    //Good,Bad,Perfect
        {1, (90, 35, 5)},
        {2, (80, 10, 10)},
        {3, (70, 10, 20)},
        {4, (60, 30, 10)},
        {5, (60, 35, 5)},
        {6, (60, 5, 40)}
    };


}

