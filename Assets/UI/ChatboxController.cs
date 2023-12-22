using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/*
    The ChatboxController class is responsible for managing the display and storage of chat messages in a game environment.
    It maintains a queue of recent messages, ensuring a maximum limit to maintain performance and clarity.
    The class offers functionality to add new messages to the chatbox, auto-scrolling to the most recent message,
    and even saving the entire chat log to a file for later review or archiving purposes.
*/
public class ChatboxController : MonoBehaviour
{
    // ----- Section: Variables for Chatbox  -----
    [SerializeField]
    private Text chatText;
    [SerializeField]
    private ScrollRect scrollRect;
    private Queue<string> chatMessages = new Queue<string>();
    private int maxMessages = 25;
    public void AddMessage(string message)
    {
        if (chatMessages.Count >= maxMessages)
        {
            chatMessages.Dequeue();
        }

        chatMessages.Enqueue(message);
        chatText.text = string.Join("\n", chatMessages.ToArray());

        // Ensure the scroll view updates the next time it's rendered
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        // Yield return null will make it continue on the next frame, 
        // this ensures the ScrollRect updates its content before scrolling
        yield return null;

        // Scroll to bottom
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void SaveChatLog()
    {
        // Here, we are using the Application.persistentDataPath which is a built-in Unity feature that returns
        // an appropriate directory path for saving data depending on the runtime platform.

        string filePath = Path.Combine(Application.persistentDataPath, $"chatlog_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        // WriteAllLines will create the file if it doesn't exist, and overwrite it if it does.
        File.WriteAllLines(filePath, chatMessages.ToArray());

        Debug.Log($"Chat log saved to {filePath}");
    }
}


