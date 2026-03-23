using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;

namespace IfThenElse
{
    public class Act2Controller : MonoBehaviour
    {
        #region Setup
        public Transform playerTransform;

        public bool useAbusiveWords = true;
        public bool TestWithoutControl = false;

        public GameObject nearbyPanel;
        public Animator nearbyPanelAnim;

        public Animator messageTextGO;
        public TMP_Text messageText;
        public PlayableDirector TimeLineDirector;

        public Settings settings;

        public Player playerScript;

        public bool isInTestMode = false;

        public AudioSource musicSource;

        public Transform[] britainSpawns;

        public TaskManager taskManager;

        public Transform[] britainCoverPoints;
        public TriggerEnter[] inaCoverPoints;

        public GameObject blackout;

      
        float prevLODBias;


        void Awake()
        {
            GetComponent<AudioSource>().Play();
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

            SoldiersInAreas.Add(0, 0);
            SoldiersInAreas.Add(1, 0);
            SoldiersInAreas.Add(2, 0);
            SoldiersInAreas.Add(3, 0);
            playerScript = playerTransform.GetComponent<Player>();
            playerScript.onPlayerDied += OnPlayerDied;
            indianSoldierParent.gameObject.SetActive(false);
            introSoldiers.gameObject.SetActive(true);
            settings.GraphicsInfo.gameObject.SetActive(true);
            if(!isInTestMode)
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(false);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                TimeLineDirector.Play();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(false);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                TimeLineDirector.time = 46;
                TimeLineDirector.Play();
                Cursor.lockState = CursorLockMode.Locked;
            }    
  
          
        }
        public ItemsAndIcons iai;
        public GameObject introSoldiers;
        public void OnTimeLineEnd()
        {
            settings.GraphicsInfo.gameObject.SetActive(false);
            introSoldiers.SetActive(false);
            indianSoldierParent.gameObject.SetActive(true);
            Debug.Log("Game's not gonna be easy");
            foreach(Soldier s in indianSoldiers)
            {
                if(s.SoldierName != "CP PAVAN")
                {
                    s.a2Controller = this;
                    s.IAI = iai;
                    s.anim.SetBool("IsAiming", true);
                }
            }
            Invoke(nameof(GoPavan), 1);
            Invoke(nameof(EnablePlayer), 2);
        }
        public GameObject[] toEnable;
        int Progress = 0;
        void GoPavan()
        {
            foreach(Soldier s in indianSoldiers)
            {
                if(s.SoldierName == "CP PAVAN")
                {
                    NavMeshAgent n = s.nav;
                    n.SetDestination(s.coverPoint.transform.position);
                    n.isStopped = false;
                    n.updatePosition = true;
                    s.anim.SetBool("IsRunning", true);
                    s.coverPoint.onObjectTrigger += s.DisableSelf;
                }
            }
        }
        void EnablePlayer()
        {
            playerTransform.gameObject.SetActive(true);
            settings.interactables.SetActive(true);
            blackout.gameObject.SetActive(false);

            foreach(GameObject g in toEnable)
            {
                g.SetActive(true);
            }

            
            foreach (Transform c in britainCoverPoints)
            {
                Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], c.position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                britSoldier.a2Controller = this;
                britSoldier.Act = 2;
                britSoldier.player = playerTransform;
                britSoldier.IAI = iai;
                TriggerEnter triggerEnter = c.GetComponent<TriggerEnter>();
                int areaCode = triggerEnter.AreaCode;
                britSoldier.AreaSecurityCode = areaCode;
                SoldiersInAreas[areaCode]++;
            
                britSoldier.fightMode = Soldier.FightMode.Defensive;
                britSoldier.indianSoldierParent = indianSoldierParent;
                britishSoldierCount++;
                britSoldier.isReadyToDefend = true;
                britSoldier.target = playerTransform;
            }
            Invoke(nameof(SpawnFirstAttackingSoldier), 2f);
            musicSource.Play();
            QualitySettings.lodBias = prevLODBias;

            PostProcessVolume v = settings.volume;

            v.profile.TryGetSettings(out DepthOfField dof);
            dof.active = false;

            if (PlayerPrefs.GetString("MotionBlur") == "On")
            {
                settings.motionBlurToggle.isOn = true;
                settings.volume.profile.TryGetSettings(out MotionBlur x);
                x.active = true;
                v.enabled = true;
                settings.noOfEffects += 1;
            }

            warningOrMsgText.text = "<color=yellow>TASK:</color> ENTER THE BRITAIN RAILWAY";
            warningOrMsg.SetTrigger("Display");
            taskManager.enabled = true;

            messageTextGO.gameObject.SetActive(false);
            messageTextGO.gameObject.SetActive(true);
            messageText.text = "00 HOURS";
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
            Invoke(nameof(HideMessageText), 5f);
            Invoke(nameof(ShowCoverSuggestion), 15f);
        }

        void HideMessageText()
        {
            messageTextGO.SetTrigger("Hide");
        }
       
        void ShowCoverSuggestion()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=orange>RECOMMEND:</color> TAKE COVER BEHIND SEATS";
            warningOrMsg.SetTrigger("Display");
        }
        void ShowGrenadeSuggestion()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=orange>RECOMMEND:</color> USE GRENADES";
            warningOrMsg.SetTrigger("Display");
        }

        #endregion

        #region MainAct

        [Header("Act")]
        public int britishSoldierCount;
        public int kills;
        public Soldier[] indianSoldiers;
        public GameObject[] BritainSoldierPrototypes;
        public Transform britishSoldierParent;
        public Transform indianSoldierParent;

        public bool ShowSubtitles;

        public Animator warningOrMsg;
        public TMP_Text warningOrMsgText;

        public Transform[] playerSpawns;
        void SpawnFirstAttackingSoldier()
        {
                Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawns[0].position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                britSoldier.fightMode = Soldier.FightMode.Attacking;
                britSoldier.a2Controller = this;
                britSoldier.player = playerTransform;
                britSoldier.IAI = iai;
                britSoldier.indianSoldierParent = indianSoldierParent;
                britSoldier.InitTarget(playerTransform);
                britishSoldierCount++;        
        }
     
        public int SoldiersInA1;
        public int SoldiersInA2;
        public int SoldiersInA3;
        public int SoldiersInA4;
        Dictionary<int, int> SoldiersInAreas = new();
       
        public void SoldierDied(Soldier s, Soldier.Nationality nation, Transform from, int Area)
        {


            if (nation == Soldier.Nationality.British)
            {
                if(s.fightMode == Soldier.FightMode.Defensive)
                {
                    SoldiersInAreas[Area]--;
                    if (SoldiersInAreas[0] == 0 && SoldiersInAreas[1] == 0 && SoldiersInAreas[2] == 0 && SoldiersInAreas[3] == 0)
                    {
                        warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED:</color> CLEARED THE RAILWAY";
                        warningOrMsg.SetTrigger("Display");
                        foreach (Transform t in settings.otherPanels.transform)
                        {
                            t.gameObject.SetActive(false);
                        }
                        blackout.SetActive(true);
                        blackout.GetComponent<Animator>().SetTrigger("FadeOut");
                        Time.timeScale = 0.2f;
                        Time.fixedDeltaTime = 0.01f;
                        Invoke(nameof(EndAct), 1.4f);
                    }
                    else if (SoldiersInAreas[Area] == 0)
                    {
                        warningOrMsgText.text = "<color=yellow>TASK:</color> CLEAR THE NEXT CARRIAGE";
                        warningOrMsg.SetTrigger("Display");
                        taskManager.NextTask();
                        Progress++;
                       
                            Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawns[Progress].position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                            britSoldier.fightMode = Soldier.FightMode.Attacking;
                            britSoldier.a2Controller = this;
                            britSoldier.player = playerTransform;
                            britSoldier.IAI = iai;
                            britSoldier.indianSoldierParent = indianSoldierParent;
                            britSoldier.InitTarget(playerTransform);
                            britishSoldierCount++;
                        
                        playerScript.playerSpawn = playerSpawns[Progress];
                    }

                }
                britishSoldierCount -= 1;
                IncrementKills();
            }
                
        }
        int streak = 0;
        public void OnPlayerDied()
        {
            CancelInvoke(nameof(ShowTaskSuggestion));
            StopCoroutine(nameof(showNextDialogue));
            streak = 0;
            if (ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }

           

        }
        void ShowTaskSuggestion()
        {
            warningOrMsgText.text = "<color=grey>SUGGESTION:</color> PRESS 'T' TO VIEW TASKS";
            warningOrMsg.SetTrigger("Display");
        }
        public void IncrementKills()
        {
            CancelInvoke(nameof(ShowTaskSuggestion));
            Invoke(nameof(ShowTaskSuggestion), 20);
            kills += 1;
            if(kills == 1)
            {
                Invoke(nameof(ShowGrenadeSuggestion), 2.5f);
            }
            if (kills == 10)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "01 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke("HideMessageText", 5f);
            }
            if (kills == 19)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "02 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke("HideMessageText", 5f);
            }
            
            streak += 1;

            if (streak >= 2)
            {
                ShowAppreciation("<color=yellow>" + streak + "X</color> Streak", streakIcon);
            }
        }

        public void TreatPlayerAsTraitor()
        {
            if (!playerScript.isDead)
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
                settings.deathPanel.SetActive(true);
                settings.deathReason.text = "You were treated as a traitor for killing your own mate.";
                playerScript.Die(false);
            }
        }

        #endregion

        #region EndScene
        void EndAct()
        {
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
            settings.interactables.SetActive(false);
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;

            foreach (Transform t in settings.otherPanels.transform)
            {
                if (t.name != "Blackout")
                {
                    t.gameObject.SetActive(false);
                }
            }
            PlayerPrefs.SetInt("ActNumber", 3);
            PlayerPrefs.SetString("SelectedAct", "Act3");
            PlayerPrefs.SetInt("Act2Completed", 1);
            SceneManager.LoadScene("Story");
        }
      
        #endregion

        #region Dialogues
        [Space(5f)]
        [Header("Dialogues")]
        public GameObject dialogueBox;
        public Transform dialogueContainer;
        public AudioSource dialoguePlayer;
        public List<AudioClip> EnglishDialogues = new List<AudioClip>();
        IEnumerator showNextDialogue(float waitTime, int DialogueClipIndex, int senderType, string message, string senderName, bool isAbusive, Soldier soldier)
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
