using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearPrefs : MonoBehaviour
{
    
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("cleared prefs");
            SceneManager.LoadScene("ITELogo");
        }
        else if (Input.GetKeyUp(KeyCode.Return))
        {
            SceneManager.LoadScene("ITELogo");
        }
    }
}
