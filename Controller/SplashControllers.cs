using UnityEngine;
using UnityEngine.SceneManagement;

namespace IfThenElse
{
    public class SplashControllers : MonoBehaviour
    {
        public bool End = false;
        public bool Exit = false;
        public string NextScene = "Menu";
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void AnimationEnd()
        {
            if(Exit)
            {
                Debug.Log("Game Closed");
                Application.Quit();
                
            }
            else
            {
                Invoke(nameof(GoNextScene), 3);
               
            }
        }
        void GoNextScene()
        {
            SceneManager.LoadScene(NextScene, LoadSceneMode.Single);
        }
        private void Update()
        {
            if(End) //this means it is the 2nd splash screen
            {
                PlayerPrefs.SetString("CurrentLoadScene", NextScene);
                SceneManager.LoadScene("Loading", LoadSceneMode.Single);
            }
        }
    }
}
