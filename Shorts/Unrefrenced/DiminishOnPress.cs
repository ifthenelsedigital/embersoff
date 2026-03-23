using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DiminishOnPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float desiredTime = 1f;
    bool isScaling = false;
    Vector3 minScaled;
    Vector3 maxScaled;
    public void OnEnable()
    {
        isScaling = false;
        transform.localScale = new Vector3(1, 1, 1);
    }
    public void OnPointerDown(PointerEventData data)
    {
        if(!isScaling)
        {
            isScaling = true;
            StopCoroutine(scaleUp());
            StartCoroutine(scaleDown());
        }
       
    }
    public void OnPointerUp(PointerEventData data)
    {
        if(!isScaling)
        {
            isScaling = true;
            StopCoroutine(scaleDown());
            StartCoroutine(scaleUp());
        } 
    }
    IEnumerator scaleDown()
    {
        float timer = 0f;
        while(timer < desiredTime)
        {
            timer += Time.deltaTime;
            transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f) * Time.deltaTime;
        }
        if(minScaled != null)
        {
            minScaled = transform.localScale;
        }
        if (transform.localScale != minScaled)
        {
            transform.localScale = minScaled;
        }
        isScaling = false;
        yield return null;
    }
    IEnumerator scaleUp()
    {
        float timer = 0f;
        while (timer < desiredTime)
        {
            timer += Time.deltaTime;
            transform.localScale += new Vector3(0.1f, 0.1f, 0.1f) * Time.deltaTime;
        }
        if(maxScaled != null)
        {
            maxScaled = transform.localScale;
        }
        if(transform.localScale != maxScaled)
        {
            transform.localScale = maxScaled;
        }
        isScaling = false;
        yield return null;
    }
    
}
