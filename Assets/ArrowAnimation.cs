using System.Collections;
using UnityEngine;

public class ArrowAnimation : MonoBehaviour
{
    public float animationDuration = 1.0f; // The duration of the animation in seconds

    private void Start()
    {
        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator DestroyAfterAnimation()
    {
        // Wait for the duration of the animation
        yield return new WaitForSeconds(animationDuration);

        // Destroy the arrow game object
        Destroy(gameObject);
    }
}
