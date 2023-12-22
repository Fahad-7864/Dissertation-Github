using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/*
    The DeathScript class is responsible for handling the death mechanics of a character within the game environment.
    Upon detecting that a character's health has reached zero or below, it triggers appropriate actions such as playing a death animation and changing the sprite to a "dead" state.
    The class integrates closely with the CharacterStats component, monitoring changes in health and responding accordingly.
    Moreover, it ensures that necessary resources are cleaned up to prevent memory leaks, maintaining optimal game performance.
*/
public class DeathScript : MonoBehaviour
{
    [SerializeField]
    private CharacterStats characterStats;
    private Animator characterAnimator;
    private SpriteRenderer characterSprite;

    // This event is triggered when the character dies.
    public UnityEvent OnCharacterDeath;

    void Start()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        characterAnimator = GetComponent<Animator>();
        characterSprite = GetComponent<SpriteRenderer>();

        // Subscribe to the character's HP changes
        characterStats.OnHPChanged += CheckDeath;
    }

    void CheckDeath()
    {
        Debug.Log("Checking Death");
        if (characterStats.IsDead && OnCharacterDeath != null)
        {
            OnCharacterDeath.Invoke();  // Invoke all subscribed events/methods
            PlayDeathAnimation();
        }
    }

    void PlayDeathAnimation()
    {
        if (characterAnimator != null)
        {
            characterAnimator.Play("Death");
            StartCoroutine(DeactivateSpriteAfterAnimation());
        }
        else
        {
            // If there's no animator, just deactivate the sprite immediately.
            if (characterSprite != null) characterSprite.enabled = false;
        }
    }

    // Assuming the death animation takes around 2 seconds 
    IEnumerator DeactivateSpriteAfterAnimation()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(2.0f);  
        if (characterAnimator != null)
        {
            // Disable the animator
            characterAnimator.enabled = false;  
        }
        if (characterSprite != null)
        {
            // Change to deadSprite after disabling the animator
            characterSprite.sprite = characterStats.deadSprite;
            // Ensure the sprite is visible
            characterSprite.enabled = true;
        }
    }


    // Cleanup to avoid memory leaks
    private void OnDestroy()
    {
        characterStats.OnHPChanged -= CheckDeath;
    }
}
