using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

namespace IfThenElse
{
    public class LoadingController : MonoBehaviour
    {
        public GameObject progressAnim;
        string nextScene;
        AsyncOperation asyncOperation;
        bool isLoadingStarted = false;
        // Start is called before the first frame update
        void Start()
        {
            isLoadingStarted = false;
            Invoke(nameof(LoadCurrentScene), 4f);
        }

        void LoadCurrentScene()
        {
           
            nextScene = PlayerPrefs.GetString("CurrentLoadScene");
            if(nextScene == "MainMenu" && !PlayerPrefs.HasKey("HasCompletedTraining") && !PlayerPrefs.HasKey("QuitFromAct1"))
            {
               
                    nextScene = "Training";
                
            }
            asyncOperation = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
           
            isLoadingStarted = true;
        }
        void Update()
        {
            if (isLoadingStarted)
            {
                float progress = Mathf.RoundToInt(asyncOperation.progress * 100);
                if(progress > 91)
                {
                    progressAnim.SetActive(false);
                }
            }
        }
    }
}
