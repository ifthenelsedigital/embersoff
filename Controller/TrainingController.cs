using UnityEngine;
using TMPro;

namespace IfThenElse
{
    public class TrainingController : MonoBehaviour
    {
        public Player player;
        public Animator startPanel;
        public AudioClip victoryMusic;

        public TMP_Text warningText;
        public Animator warningOrMsg;

        public GameObject crosshair; 

        Animator anim;
        private void Awake()
        {
            QualitySettings.SetQualityLevel(3);
            Cursor.lockState = CursorLockMode.None;
        }

        void Start()
        {         
            anim = GetComponent<Animator>();
            startPanel.SetTrigger("Open");
            crosshair.SetActive(false);
            player.AcceptMovement = false;
            player.onAmmoOver += OnPlayerAmmoOver;
        }
        public void StartTraining()
        {
            crosshair.SetActive(true);
            player.AcceptFireControls = true;
            player.AcceptMovement = true;
            Cursor.lockState = CursorLockMode.Locked;
            warningText.text = "<color=yellow>TASK: </color>DESTROY ALL THE TARGETS";
            warningOrMsg.SetTrigger("Display");

            isTestStarted = true;
        }
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            isExitPanelOpen = false;
            player.AcceptFireControls = true;
            player.AcceptMovement = true;
        }
        void OnPlayerAmmoOver()
        {
            if(player.currentWeaponScript.Type == Weapon.GunType.AR)
            {
                player.PickUpObject("AR Ammo", 60, "Ammo", false, 0);
            }
            else
            {
                player.PickUpObject("HG Ammo", 60, "Ammo", false, 0);
            }
        }
        public void OnSkipPressed()
        {
            PlayerPrefs.SetInt("HasCompletedTraining", 1);
            PlayerPrefs.SetString("CurrentLoadScene", "MainMenu");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
        }
        public Animator ExitPanel;
        bool isExitPanelOpen = false;
        public void OnQuitBtnClick()
        {
            if(isExitPanelOpen)
            {
                ExitPanel.SetTrigger("Close");
                Cursor.lockState = CursorLockMode.Locked;
                isExitPanelOpen = false;
                player.AcceptFireControls = true;
                player.AcceptMovement = true;
            }
            else
            {
                ExitPanel.SetTrigger("Open");
                Cursor.lockState = CursorLockMode.None;
                isExitPanelOpen = true;
                player.AcceptFireControls = false;
                player.AcceptMovement = false;
            }
        }
        bool isTestStarted = false;
        public void OnYesClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Exit");
        }
   
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && isTestStarted)
            {
                OnQuitBtnClick();
            }
        }
      
        public Animator completionPanel;
        public void BeginJourney()
        {
            PlayerPrefs.SetInt("HasCompletedTraining", 1);
            PlayerPrefs.SetString("CurrentLoadScene", "MainMenu");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
        }
        int targetsDestroyed = 0;
        public void OnTargetShot()
        {
            targetsDestroyed++;
            if(targetsDestroyed == 6)
            {
                OnAllTargetsShot();
            }
        }
        void OnAllTargetsShot()
        {
            player.AcceptFireControls = false;
            player.isFirePressed = false;
            player.weaponsAnim.SetBool("isScoped", false);
            player.currentWeaponScript.cameraObject.SetBool("isScoped", false);
            player.weaponsAnim.SetBool("isShooting", false);
            player.weaponsAnim.SetTrigger("Close");
            anim.SetTrigger("FadeOut");
            Invoke("PlayVictoryMusic", 0.4f);
            Invoke(nameof(DisplayCompletion), 1.1f);
           
        }
        void DisplayCompletion()
        {
            Cursor.lockState = CursorLockMode.None;
            completionPanel.SetTrigger("Open");
            crosshair.SetActive(false);
        }
        void PlayVictoryMusic()
        {
            GetComponent<AudioSource>().clip = victoryMusic;
            GetComponent<AudioSource>().Play();
        }
    }
}
