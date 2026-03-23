using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace IfThenElse
{
    public class Menu : MonoBehaviour
    {
        public Animator canvasAnim;
        public AudioSource bgm;
        public Settings settings;
        public TMP_Text versionText;


        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            versionText.text = "v" + Application.version;
            cover.SetActive(false);

        }
    
        private void Start()
        {
            Debug.Log("Heyy! Enjoy Playing Embers Off! This will take you into the epic story of INA and India's struggle!");
            bgm.volume = PlayerPrefs.GetFloat("MusicVolume");
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
            if (!PlayerPrefs.HasKey("Graphics"))
            {
                settings.FirstTime = true;
                settings.SetDefaultSettings();
            }
            else
            {
                QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphics"));
             
            }

            
        }

        public Camera cam;
        public GameObject actsView;
        public ActPreview[] Acts;
        public void SelectActClicked()
        {
            SelectedAct = null;
            if (PlayerPrefs.HasKey("ActNumber"))
            {
                foreach (ActPreview act in Acts)
                {
                    if (PlayerPrefs.HasKey("Act" + act.ActNumber + "Completed"))
                    {
                        act.PlayButton.interactable = true;
                        act.interactiveButton.enabled = true;
                        act.LockPanel.SetActive(false);
                        act.CompletedText.SetActive(true);
                    }
                    else if (PlayerPrefs.GetInt("ActNumber") == act.ActNumber)
                    {
                        act.LockPanel.SetActive(false);
                        act.CompletedText.SetActive(false);
                        act.interactiveButton.enabled = true;
                    }
                    else
                    {
                        act.PlayButton.interactable = false;
                        act.LockPanel.SetActive(true);
                        act.CompletedText.SetActive(false);
                        act.interactiveButton.enabled = false;
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt("ActNumber", 1);
                SelectActClicked();
            }

        }
        public GameObject cover;
        public void ChooseDifficulty(int difficulty)
        {
            PlayerPrefs.SetInt("Difficulty", difficulty);
            bgm.volume /= 2;
            cover.SetActive(true);
    
            if(SelectedAct == null)
            {
                SelectedAct = "Act" + PlayerPrefs.GetInt("ActNumber");
            }


            if (PlayerPrefs.HasKey(SelectedAct + "CompletedStory"))
            {
                PlayerPrefs.SetString("CurrentLoadScene", SelectedAct);
            }
            else
            {
                PlayerPrefs.SetString("CurrentLoadScene", "Story");
                PlayerPrefs.SetString("SelectedAct", SelectedAct);
            }
            SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
        }
        string SelectedAct;
        public void Exit()
        {
            SceneManager.LoadScene("Exit");
        }
        public void PlayAct(string ActName)
        {
            SelectedAct = ActName;

            if (PlayerPrefs.HasKey(ActName + "CompletedStory"))
            {
                PlayerPrefs.SetString("CurrentLoadScene", ActName);
                PlayerPrefs.SetString("SelectedAct", SelectedAct);

            }
            else
            {
                PlayerPrefs.SetString("CurrentLoadScene", "Story");
                PlayerPrefs.SetString("SelectedAct", SelectedAct);
            }

        }
        public GameObject EasyCompletion;
        public GameObject FierceCompletion;
        public GameObject RealityCompletion;
        public TMP_Text EasyDate;
        public TMP_Text FierceDate;
        public TMP_Text RealityDate;
        public void PlayClicked()
        {
            SelectedAct = null;
            if (PlayerPrefs.HasKey("ActNumber"))
            {
                if (PlayerPrefs.GetInt("ActNumber") == 1)
                {
                    canvasAnim.SetTrigger("OpenDifficulty");
                    SelectedAct = "Act1";
                    PlayerPrefs.SetString("CurrentLoadScene", "Story");
                }
                else
                {
                    canvasAnim.SetTrigger("OpenContinuation");
                }
            }
            else
            {
                PlayerPrefs.SetString("CurrentLoadScene", "Story");
                PlayerPrefs.SetInt("ActNumber", 1);
                canvasAnim.SetTrigger("OpenDifficulty");
            }

            if(PlayerPrefs.HasKey("CompletedInEasy"))
            {
                Debug.Log("You've already completed game in easy mode");
                EasyCompletion.SetActive(true);
                EasyDate.text = PlayerPrefs.GetString("CompletedInEasy");
            }
            if (PlayerPrefs.HasKey("CompletedInFierce"))
            {
                Debug.Log("You've already completed game in fierce mode");
                FierceCompletion.SetActive(true);
                FierceDate.text = PlayerPrefs.GetString("CompletedInFierce");
            }
            if (PlayerPrefs.HasKey("CompletedInReality"))
            {
                Debug.Log("You've already completed game in reality mode");
                RealityCompletion.SetActive(true);
                RealityDate.text = PlayerPrefs.GetString("CompletedInReality");
            }
        }
    }
}
