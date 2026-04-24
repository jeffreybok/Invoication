using System.Collections;
using UnityEngine;

public class TitleIntroAnimation : MonoBehaviour
{
    [Header("Delay Before Title Appears")]
    public float startDelay = 2f;

    [Header("Scale Animation")]
    public float animationDuration = 1.2f;
    public Vector3 hiddenScale = Vector3.zero;
    public Vector3 finalScale = Vector3.one;

    private void Start()
    {
        transform.localScale = hiddenScale;
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(startDelay);

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;

            // Smooth easing
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(hiddenScale, finalScale, t);
            yield return null;
        }

        transform.localScale = finalScale;
    }
}
