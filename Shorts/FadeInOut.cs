using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public float IncreaseRate;
    public AudioSource audioSource;
    public float MinAlpha = 0;
    public float MaxAlpha = 1;
    public Image Source;
    private void OnEnable()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        while(Source.color.a > MinAlpha)
        {
            Source.color = new Color(Source.color.r, Source.color.g, Source.color.b, Source.color.a - IncreaseRate);
            yield return null;
        }
        StopCoroutine(FadeOut());
        StartCoroutine(FadeIn());
    }
    IEnumerator FadeIn()
    {
        while (Source.color.a < MaxAlpha)
        {
            Source.color = new Color(Source.color.r, Source.color.g, Source.color.b, Source.color.a + IncreaseRate);
            yield return null;
        }
        audioSource.Play();
        StopCoroutine(FadeIn());
        StartCoroutine(FadeOut());
    }
}
