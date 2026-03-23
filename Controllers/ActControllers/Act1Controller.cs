using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

namespace IfThenElse
{
    public class Act1Controller : MonoBehaviour
    {
        #region Setup
        public Transform playerTransform;

        public bool useAbusiveWords = true;
        public bool TestWithoutControl = false;

        public GameObject nearbyPanel;
        public Animator nearbyPanelAnim;

        public GameObject keyHints;

        public TaskManager taskManager;

        public Animator messageTextGO;
        public TMP_Text messageText;
        public PlayableDirector TimeLineDirector;
        public PlayableDirector endScenePlayer;
        public GameObject introObjects;

        public bool ShowSubtitles;

        public Settings settings;

        public Player playerScript;

        public bool isInTestMode = false;

        public AudioSource musicSource;
        public Animator camAnim; //of player

      
        float prevLODBias;

        void Awake()
        {
            GetComponent<AudioSource>().Play();
            isActStarted = false;
            settings.terrains[0].detailObjectDistance = 40;


            if (PlayerPrefs.GetString("DepthOfField") == "On")
            {
                settings.dofToggle.isOn = true;
                settings.volume.profile.TryGetSettings(out DepthOfField x);
                x.active = true;
                settings.volume.enabled = true;
                settings.layer.enabled = true;
                settings.noOfEffects += 1;
            }


            playerScript = playerTransform.GetComponent<Player>();
            playerScript.onPlayerDied += OnPlayerDied;
            settings.GraphicsInfo.gameObject.SetActive(true);
            if (!isInTestMode)
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(false);
                introObjects.SetActive(true);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                TimeLineDirector.Play();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                EnablePlayer();

            }
            if(!Application.isEditor)
            {
                Kills = 0;
            }
            
            if(difficulty == 0)
            {
                maxSoldierCount = 7;
            }
            else if(difficulty == 1)
            {
                maxSoldierCount = 10;
            }
            else if(difficulty == 2)
            {
                maxSoldierCount = 13;
            }
          
        }
        public void OnTimeLineEnd()
        {
            QualitySettings.lodBias = prevLODBias;
            Invoke("EnablePlayer", 9f);
        }
        void EnablePlayer()
        {
            settings.GraphicsInfo.gameObject.SetActive(false);
            playerTransform.gameObject.SetActive(true);
            introObjects.SetActive(false);
            settings.interactables.SetActive(true);
            blackout.gameObject.SetActive(false);

            PostProcessVolume v = settings.volume;

            keyHints.SetActive(true);
            Invoke(nameof(DisableKeyHints), 4.5f);
            v.profile.TryGetSettings(out DepthOfField dof);
            dof.active = false;
            Debug.Log("dof is " + dof.active);

            if (PlayerPrefs.GetString("MotionBlur") == "On")
            {
                settings.motionBlurToggle.isOn = true;
                settings.volume.profile.TryGetSettings(out MotionBlur x);
                x.active = true;
                v.enabled = true;
                settings.noOfEffects += 1;
            }

            if (!TestWithoutControl)
            {
                InvokeRepeating("SpawnRandomSoldier", 3f, 3f);
            }
            messageTextGO.gameObject.SetActive(false);
            messageTextGO.gameObject.SetActive(true);
            messageText.text = "700 HOURS";
            messageTextGO.SetTrigger("Show");
            if(PlayerPrefs.GetString("Grass") == "On")
            {
                settings.grassToggle.isOn = true;
                settings.terrains[0].detailObjectDistance = 40;
            }
            else
            {
                settings.grassToggle.isOn = false;
                settings.terrains[0].detailObjectDistance = 0;
            }
            Invoke("HideMessageText", 5f);
            if(!TestWithoutControl)
            {
                Invoke("StartAct", 7f);
            }
        }

        void HideMessageText()
        {
            messageTextGO.SetTrigger("Hide");
        }
        private void DisableKeyHints()
        {
            keyHints.SetActive(false);
        }


        #endregion

        #region Main Act

        #region MainFight

        public int Kills = 0;
        int streak = 0;
        public int japaneseSoldierCount;
        public GameObject startExplosion;
        public List<GameObject> explosions = new List<GameObject>();
        public Transform japaneseSoldiersHolder;
        public Act1Soldier[] britainSoldiers;
        public Shatter startCabin;
        int difficulty;
        void StartAct()
        {
            taskManager.enabled = true;
            Debug.Log("Act has started");
            startExplosion.SetActive(true);
            startExplosion.GetComponent<Bomb>().Explode();
            startCabin.Invoke("ShatterObject", 1f);
            Invoke("ExplodeABomb", 5f);
            difficulty = PlayerPrefs.GetInt("Difficulty");
            isActStarted = true;
            foreach(Act1Soldier s in britainSoldiers)
            {           
                    s.OnBaseAttacked();
            }
            musicSource.Play();
            //Invoke("StartDialogues", 1.2f);
        }
        void ExplodeABomb()
        {
            GameObject explosion = explosions[Random.Range(0, explosions.Count)];
            explosion.SetActive(true);
            explosion.GetComponent<Bomb>().Explode();
            Invoke("ExplodeABomb", Random.Range(5f, 10f));
        }
        public List<AudioClip> EnglishDialogues = new List<AudioClip>();
       
      
        public Animator warningTextAnim;
        public TMP_Text warningText;
        void StartDialogues()
        {
            warningText.text = "<color=yellow>TASK: </color>PROTECT YOUR SOLDIERS AND THE BASE!";
            warningTextAnim.SetTrigger("Display");
            StartCoroutine(showNextDialogue(0f, 2, 0, "We're under attack! Take Cover", "Arshad", false, arshad));     
            StartCoroutine(showNextDialogue(3.5f, 3, 0, "The japanese maybe, All men standby! We will protect Singapore!", "Sodhi", false, null));     
            StartCoroutine(showNextDialogue(9f, 4, 0, "They are fucking Japanese! No matter, they came here to die.", "Aavir", true, null));
            StartCoroutine(showNextDialogue(15f, 5, 0, "They have set their mortars somewhere I cannot see...", "Barhan", false, null));
            StartCoroutine(showNextDialogue(23f, 6, 0, "All extra ammunition, weapons, and other supplies are in Tents if you need them!", "Arshad", false, arshad));
        }

        public List<Transform> Spawns = new List<Transform>();
    
        public List<GameObject> JapaneseSoldiersPrototype = new List<GameObject>();
        public Image crosshairCenter;
        public Animator crosshair;
        bool isActStarted = false;
        int maxSoldierCount = 0;
        void SpawnRandomSoldier()
        {
           
            if(japaneseSoldierCount < maxSoldierCount)
            {
                Transform spawnPoint = Spawns[Random.Range(0, Spawns.Count)];
                GameObject japaneseSoldier = Instantiate(JapaneseSoldiersPrototype[Random.Range(0, JapaneseSoldiersPrototype.Count)], spawnPoint.position, Quaternion.identity, japaneseSoldiersHolder.GetChild(0));
                Act1Soldier soldierScript = japaneseSoldier.GetComponent<Act1Soldier>();
                japaneseSoldierCount += 1;
                soldierScript.player = playerTransform;
                soldierScript.controller = this;
                soldierScript.fireRate = Random.Range(0.08f, 0.12f);
                soldierScript.britishSoldiers = britainSoldiers;
                soldierScript.crosshairCenter = crosshairCenter;
                soldierScript.crosshair = crosshair;            
                soldierScript.StartAttacking(playerTransform);
                
            }
        }
        public void IncrementKills()
        {
            Kills += 1;

            if(Kills == 1)
            {
                warningText.text = "<color=green>GREAT!</color> YOU CAN SEARCH THE ENEMY'S BAG FOR SUPPLIES";
                warningTextAnim.SetTrigger("Display");
                ShowAppreciation("<color=red>Death</color> Blow", normalAchievementIcon);
            }

            if (Kills == 12)
            {
                messageText.text = "800 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke("HideMessageText", 5f);
            }

            else if(Kills == 15)
            {
                warningText.text = "<color=orange>WARNING:</color> BRITAIN AIRSTRIKE APPROACHING";
                warningTextAnim.SetTrigger("Display");
                Invoke("CallInAirstrike", 3f);
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "820 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke("HideMessageText", 5f);
            }

            else if (Kills == 18)
            {
                warningText.text = "<color=orange>WARNING:</color> BRITAIN AIRSTRIKE APPROACHING";
                warningTextAnim.SetTrigger("Display");
                Debug.Log("Airstrike incoming");
                Invoke("CallInAirstrike", 3f);
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "830 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke("HideMessageText", 5f);
            }

            else if (Kills == 25)
            {
                warningText.text = "<color=yellow>TASK:</color> REACH THE CABIN";
                warningTextAnim.SetTrigger("Display");
                taskManager.NextTask();
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "1000 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke("HideMessageText", 5f);
                StartInterrogationScene();
                Invoke(nameof(ShowTaskSuggestion), 12);
            }

            streak += 1;
            
            if(streak >= 2)
            {
                ShowAppreciation("<color=yellow>" + streak + "X</color> Streak", streakIcon);
            }
        }
        void ShowTaskSuggestion()
        {
            warningText.text = "<color=grey>SUGGESTED:</color> PRESS 'T' TO VIEW TASKS";
            warningTextAnim.SetTrigger("Display");
        }
        public void OnJapaneseSoldierDied(Transform from)
        {
            japaneseSoldierCount --;
            japaneseSoldierCount = Mathf.Clamp(japaneseSoldierCount, 0, maxSoldierCount);
            if(from == playerTransform)
            {
                IncrementKills();
            }
        }
        public GameObject deathPanel;
        public TMP_Text deathReason;
        public void TreatPlayerAsTraitor()
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;
            streak = 0;
            if (ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }
            StopCoroutine("showNextDialogue");
            CancelInvoke("SpawnJapaneseSoldiers");
            settings.deathPanel.SetActive(true);
            settings.deathReason.text = "You were treated as a traitor for killing your own mate.";
            playerScript.Die(false);
        }
        public Transform[] playerSpawns;
        public void OnPlayerDied()
        {
            StopCoroutine("showNextDialogue");
            streak = 0;
            playerScript.playerSpawn = playerSpawns[Random.Range(0, playerSpawns.Length)];
            if(ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }
            japaneseSoldierCount = 0;
            InvokeRepeating("SpawnRandomSoldier", 2f, 2f);
            if(!isActStarted)
            {
                CancelInvoke(nameof(SpawnRandomSoldier));
                Invoke("StartAct", 16f);
            }
            
        }
        
        #endregion

        #region Interrogation Scene

        [Header("Interrogation Scene")]

        public Act1Soldier arshad;
        public ArrowIndicator arrowNav;
        public Transform cabinDoor;
        public TriggerEnter cabinDoorTrigger;
        public Transform doorPosition;
        public GameObject Environment;
        void StartInterrogationScene()
        {
            CancelInvoke(nameof(ShowTaskSuggestion));
            CancelInvoke("SpawnRandomSoldier");
            CancelInvoke("ExplodeABomb");
            playerScript.isInvincible = true;
            StartCoroutine(showNextDialogue(0f, 9, 0, "Lieutenant sir, we've one Japanese soldier, in the cabin. please reach the cabin, to interrogate!", "MSM Veer", false, null));
            warningText.text = "<color=yellow>TASK: </color> REACH THE CABIN.";
            warningTextAnim.GetComponent<Animator>().SetTrigger("Display");
            messageText.text = "Follow the Arrow";
            messageTextGO.SetTrigger("Show");
            cabinDoor.gameObject.SetActive(true);
            cabinDoorTrigger.onObjectTrigger += OnPlayerReachedCabin;
            StartArrowNavigation(doorPosition);
            foreach(Act1Soldier s in britainSoldiers)
            {
                s.PauseFiring();
            }
        }

        void StartArrowNavigation(Transform to)
        {
            arrowNav.target = to;
            arrowNav.enabled = true;
        }
        public Animator blackout;
        void OnPlayerReachedCabin()
        {
            taskManager.NextTask();
            Destroy(cabinDoor.gameObject);
            blackout.gameObject.SetActive(true);
            blackout.SetTrigger("FadeOut");
            settings.interactables.SetActive(false);
            foreach(Transform t in settings.otherPanels.transform)
            {
                if(t.name != "Blackout")
                {
                    t.gameObject.SetActive(false);
                }
            }

            playerScript.MainController.inputs = Vector2.zero;
            playerScript.weaponsAnim.SetBool("isRunning", false);
            playerScript.weaponsAnim.SetBool("isScoped", false);
            playerScript.weaponsAnim.SetFloat("RunSpeed", 1f);
            playerScript.currentWeaponScript.isPaused = true;
            Invoke("EndAct", 1f);
        }




        #endregion

        #region Endscene
        public void EndAct()
        {
            settings.interactables.SetActive(false);
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;
            playerScript.AcceptFireControls = false;
            playerScript.AcceptMovement = false;
            playerScript.MainController.inputs = Vector2.zero;
            playerScript.currentWeaponScript.isPaused = false;
            playerScript.currentWeaponScript.canFire = true;
            playerScript.isCrouching = false;
            playerScript.isJumpCalled = false;
            playerScript.weaponsAnim.SetBool("isRunning", false);
            playerScript.weaponsAnim.SetBool("isScoped", false);
            playerScript.weaponsAnim.SetFloat("RunSpeed", 1f);

            playerScript.weaponsAnim.enabled = false;
            playerScript.currentWeaponScript.transform.parent.localRotation = Quaternion.Euler(Vector3.zero);
            playerScript.currentWeaponScript.transform.parent.localPosition = new Vector3(0f, 0.01f, 0f);

            playerScript.cam.GetComponent<Animator>().SetBool("isScoped", false);
            playerScript.cam.transform.localRotation = Quaternion.Euler(Vector3.zero);
            playerScript.cam.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            playerScript.isProning = false;
            playerScript.MainController.m_IsWalking = false;
            playerScript.isHoldingExplosive = false;
            playerScript.isUsingMelee = false;
            foreach (Transform t in settings.otherPanels.transform)
            {
                if (t.name != "Blackout")
                {
                    t.gameObject.SetActive(false);
                }
            }
            PlayerPrefs.SetInt("ActNumber", 2);
            PlayerPrefs.SetString("SelectedAct", "Act2");
            PlayerPrefs.SetInt("Act1Completed", 1);
            SceneManager.LoadScene("Story");
        }
       

        #endregion

        #region Airstrike
        [Header("Airstrike")]
        public Transform planeSpawn;
        public Transform planeEndPoint;
        public GameObject b17;
        public void CallInAirstrike()
        {
            Plane plane = Instantiate(b17, planeSpawn).GetComponent<Plane>();
            plane.endPoint = planeEndPoint;
            plane.playerScript = playerScript;
            plane.actNumber = 1;
            plane.cam = camAnim;
        }

        #endregion

        #endregion

        #region Dialogues
        [Space(5f)]
        [Header("Dialogues")]
        public GameObject dialogueBox;
        public Transform dialogueContainer;
        public AudioSource dialoguePlayer;
        IEnumerator showNextDialogue(float waitTime, int DialogueClipIndex, int senderType, string message, string senderName, bool isAbusive, Act1Soldier soldier)
        {
            if (!isAbusive)
            {
                yield return new WaitForSeconds(waitTime);
                dialoguePlayer.clip = EnglishDialogues[DialogueClipIndex];
                dialoguePlayer.Play();
                ShowDialogue(senderType, message, senderName);
                if (soldier != null)
                {
                    soldier.ShowNameDisplay();
                }

                StopCoroutine("showNextDialogue");
            }
            else if (useAbusiveWords)
            {
                yield return new WaitForSeconds(waitTime);
                dialoguePlayer.clip = EnglishDialogues[DialogueClipIndex];
                dialoguePlayer.Play();
                ShowDialogue(senderType, message, senderName);
                StopCoroutine("showNextDialogue");
            }
        }

        // Sender type 0 is mate, 1 is Enemy and 2 is unspecified!
        void ShowDialogue(int senderType, string message, string senderName)
        {
            if(senderType == 0)
            {
                GameObject dialogue = Instantiate(dialogueBox, dialogueContainer);
                dialogue.transform.GetChild(2).GetComponent<TMP_Text>().text =
                    "<color=green>" + senderName + ":</color><color=white>" + message;
            }
            else if (senderType == 1)
            {
                GameObject dialogue = Instantiate(dialogueBox, dialogueContainer);
                dialogue.transform.GetChild(2).GetComponent<TMP_Text>().text =
                    "<color=red>" + senderName + ":</color><color=white>" + message;
            }
            else
            {
                GameObject dialogue = Instantiate(dialogueBox, dialogueContainer);
                dialogue.transform.GetChild(2).GetComponent<TMP_Text>().text =
                    "<color=orange>" + senderName + ":</color><color=white>" + message;
            }
        }


        #endregion

        #region Appreciation

        [Header("Appreciation")]
        public Transform ap;
        public GameObject appreciationPop;
        public Sprite normalAchievementIcon;
        public Sprite streakIcon;
        public Sprite oneShotIcon;
        public void ShowAppreciation(string textLine, Sprite icon)
        {
            if (ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }
            GameObject a = Instantiate(appreciationPop, ap);
            a.transform.GetChild(0).GetComponent<Image>().sprite = icon;
            a.transform.GetChild(1).GetComponent<TMP_Text>().text = textLine;
        }


        #endregion

    }
}
