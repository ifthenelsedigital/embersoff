using UnityEngine;
using TMPro;


public class FPS : MonoBehaviour
{
    float deltaTime = 0.0f;
    public TMP_Text display;
    
    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        display.text = Mathf.RoundToInt(fps).ToString();
    }
}
