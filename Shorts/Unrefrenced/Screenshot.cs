using UnityEngine;
using UnityEditor;

public class Screenshot : MonoBehaviour
{
    [Header("PRESS k FOR SCREENSHOT")]
    public string FileName;
    public string Folder;
    const KeyCode keyCode = KeyCode.K;

    public bool Capture = true;
    void Update()
    {
        if(Capture)
        {
            if (Input.GetKeyDown(keyCode))
            {
                ScreenCapture.CaptureScreenshot(Application.dataPath + "/" + Folder + "/" + FileName + ".png", 2);
                Debug.Log("captured screenshot!");
                Capture = false;
            }
        }
        
    }
}
