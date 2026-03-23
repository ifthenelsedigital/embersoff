using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IfThenElse
{
    public class Act3Controller : MonoBehaviour
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

        public GameObject introObjects;

        public bool ShowSubtitles;

        public Settings settings;

        public Player playerScript;

        public bool isInTestMode = false;

        public AudioSource musicSource;

        string BaseProgress = "A";

        public Transform[] britainSpawnsBaseA;
        public Transform[] britainSpawnsBaseB;
        public Soldier[] britainSoldiers;

        public Transform[] playerSpawns;

        public GameObject blackout;

        public TaskManager taskManager;
      
        float prevLODBias;
        public ItemsAndIcons iai;

        void Awake()
        {
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

            foreach(Soldier s in britainSoldiers)
            {
                s.a3Controller = this;
                s.IAI = iai;
            }
            playerScript = playerTransform.GetComponent<Player>();
            playerScript.onPlayerDied += OnPlayerDied;
            settings.GraphicsInfo.gameObject.SetActive(true);
            if (!isInTestMode)
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(false);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                
                introObjects.SetActive(true);
                TimeLineDirector.Play();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(false);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;

                introObjects.SetActive(true);
                TimeLineDirector.Play();
                TimeLineDirector.time = 36.6f; 
                Cursor.lockState = CursorLockMode.Locked;
            }    
        }
        
        public void OnTimeLineEnd()
        {
            settings.GraphicsInfo.gameObject.SetActive(false);
            foreach(Soldier s in indianSoldiers)
            {
                s.a3Controller = this;
            }
            britishSoldierParentBaseA.gameObject.SetActive(true);
            britishSoldierParentBaseB.gameObject.SetActive(false);
            EnablePlayer();
        }
        void EnablePlayer()
        {
            introObjects.SetActive(false);
            playerTransform.gameObject.SetActive(true);
            settings.interactables.SetActive(true);
            blackout.SetActive(false);

            musicSource.Play();
            QualitySettings.lodBias = prevLODBias;

            britishSoldierCount = 19;  //soldier count of base a

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
           
            else
            {
                playerScript.AcceptMovement = true;
                playerScript.AcceptFireControls = true;
            }
            Invoke(nameof(ReachDefenseSpots), 1f);  //some time for dialogues
           
            messageTextGO.gameObject.SetActive(false);
            messageTextGO.gameObject.SetActive(true);
            messageText.text = "07 HOURS";
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
            
        }

        void HideMessageText()
        {
            messageTextGO.SetTrigger("Hide");
        }
       


        #endregion

        #region MainAct

        [Header("Act")]
        public int britishSoldierCount;
        public ArrowIndicator arrowNav;
        public int kills;
        public PlayableDirector baseAMeetupTimeline;
        public Soldier[] indianSoldiers;
        public GameObject[] BritainSoldierPrototypes;
        public Transform britishSoldierParentBaseA;
        public Transform britishSoldierParentBaseB;
        public Transform indianSoldierParent;

        public Animator warningOrMsg;
        public TMP_Text warningOrMsgText;
        public TriggerEnter playerMeetupPoint;
        public GameObject meetupCam;

        #region INA Soldiers Movement Plan
        void ReachDefenseSpots() //for INA
        {
            warningOrMsgText.text = "<color=yellow>TASK: </color>FOLLOW CP PAVAN";
            taskManager.enabled = true;
            warningOrMsg.SetTrigger("Display");
            foreach (Soldier s in indianSoldiers)
            {
                s.britainSoldierParent = britishSoldierParentBaseA;
                s.ReachSpot(s.coverPoint, true);
                Invoke(nameof(ShowTaskMsg), 11f);
                indianSoldiersCount++; //count the number of indian soldiers jugaad for no specific struggle
            }
        }
        void ShowTaskMsg() //ik ise simple kiya jaa sakta tha
        {
            if(BaseProgress == "A")
            {
                taskManager.NextTask();
                warningOrMsgText.text = "<color=yellow>TASK: </color>INVADE BASE A";
                warningOrMsg.SetTrigger("Display");
                Invoke(nameof(ShowCoverHint), 5.5f);
            }
            else
            {
                taskManager.NextTask();
                warningOrMsgText.text = "<color=yellow>TASK: </color>INVADE BASE B";
                warningOrMsg.SetTrigger("Display");
                Invoke(nameof(ShowCoverHint), 15.5f);
            }
        }
        void ShowCoverHint()
        {
            if(!playerScript.isDead)
            {
                warningOrMsgText.text = "<color=orange>RECOMMEND: </color>STAY IN COVER";
                warningOrMsg.SetTrigger("Display");
            }
            else
            {
                Invoke(nameof(ShowCoverHint), 5.5f);
            }
        }
        void ReachMeetupPoint(int meetupPointNumber)
        {
            foreach (Soldier s in indianSoldiers)
            {
                s.meetupPoints[meetupPointNumber].gameObject.SetActive(true);
                s.onReachMP += OnSoldierReachMP;
                s.britainSoldierParent = britishSoldierParentBaseB;
                s.lookAtTarget = playerTransform;
                s.ReachSpot(s.meetupPoints[meetupPointNumber], false);
            }
        }
        void ReachSecondDefenseSpots() //for INA
        {
            foreach (Soldier s in indianSoldiers)
            {
                warningOrMsgText.text = "<color=yellow>TASK: </color>FOLLOW CP PAVAN";
                warningOrMsg.SetTrigger("Display");
                taskManager.NextTask();
                s.britainSoldierParent = britishSoldierParentBaseB;
                s.ReachSpot(s.coverPoint2, true);
                Invoke(nameof(ShowSniperRecommendation), 5.5f);
                Invoke(nameof(ShowTaskMsg), 14);
            }
        }
        void ShowSniperRecommendation()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=orange>RECOMMEND: </color>GRAB SOME LMG AMMO FROM SUPPLY BOXES";
            warningOrMsg.SetTrigger("Display");
        }
        #endregion

        void SpawnAttackingBritainSoldier()
        {
            if (BaseProgress == "A")
            {
                if (kills + britishSoldierCount < 30)
                {
                    Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsBaseA[Random.Range(0, britainSpawnsBaseA.Length)].position, Quaternion.identity, britishSoldierParentBaseA).GetComponent<Soldier>();
                    britSoldier.fightMode = Soldier.FightMode.Attacking;
                    britSoldier.a3Controller = this;
                    britSoldier.Act = 3;
                    britSoldier.IAI = iai;
                    britSoldier.player = playerTransform;
                    britSoldier.indianSoldierParent = indianSoldierParent;
                    britishSoldierCount++;
                    britSoldier.InitTarget(playerTransform);
                    
                }
            }
            else if (BaseProgress == "B")
            {
                if (kills + britishSoldierCount < 70)
                {
                    Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsBaseB[Random.Range(0, britainSpawnsBaseB.Length)].position, Quaternion.identity, britishSoldierParentBaseB).GetComponent<Soldier>();
                    britSoldier.fightMode = Soldier.FightMode.Attacking;
                    britSoldier.Act = 3;
                    britSoldier.a3Controller = this;
                    britSoldier.player = playerTransform;
                    britSoldier.IAI = iai;
                    britSoldier.indianSoldierParent = indianSoldierParent;
                    britishSoldierCount++;
                    britSoldier.InitTarget(playerTransform);
                }
         
            }

        }
        public void SoldierDied(Soldier s, Soldier.Nationality nation, Transform from)
        {
            if (nation == Soldier.Nationality.British)
            {
                britishSoldierCount -= 1;
                if(from == playerTransform)
                {
                    IncrementKills();
                }
            }
          
        }
        int streak = 0;
        bool isPlayerOnMP;
        public int soldiersOnMP = 0;
        public int indianSoldiersCount;
        public void OnPlayerDied()
        {
            CancelInvoke(nameof(ShowTaskSuggestion));
            StopCoroutine(nameof(showNextDialogue));
            streak = 0;
            if(BaseProgress == "A")
            {
                playerScript.playerSpawn = playerSpawns[0];
            }
            else
            {
                playerScript.playerSpawn = playerSpawns[1];
            }
            if (ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }         

        }

        #region Meetup
        void OnPlayerReachedMeetupPoint()
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

            playerScript.Crosshair.gameObject.SetActive(false);
            playerScript.CrosshairCentre.gameObject.SetActive(false);

            playerScript.weaponsAnim.enabled = false;
            playerScript.currentWeaponScript.transform.parent.localRotation = Quaternion.Euler(Vector3.zero);
            playerScript.currentWeaponScript.transform.parent.localPosition = new Vector3(0f, 0.01f, 0f);

            playerScript.cam.GetComponent<Animator>().SetBool("isScoped", false);
            playerScript.cam.transform.localRotation = Quaternion.Euler(Vector3.zero);
            playerScript.cam.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            playerScript.isProning = false;
            playerScript.MainController.m_IsWalking = false;
            playerScript.gameObject.SetActive(false);
            meetupCam.SetActive(true);

            Destroy(playerMeetupPoint.gameObject);
            arrowNav.enabled = true;
            arrowNav.gameObject.SetActive(false);

            playerScript.AllyWarningPanel.SetActive(false);
            isPlayerOnMP = true;

            if (soldiersOnMP == indianSoldiersCount)
            {
                HideMessageText();
                baseAMeetupTimeline.Play();
            }
            else
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "Wait For Soldiers";
                messageTextGO.SetTrigger("Show");
                
            }

            Invoke(nameof(SolveError), 20);
        }
        public void MeetupTimelineEnd()
        {
            meetupCam.SetActive(false);
            playerTransform.gameObject.SetActive(true);
            playerScript.RefreshControls(false, false);
            britishSoldierCount = 20;  //soldier count of base b
            musicSource.Play();
            playerScript.Crosshair.gameObject.SetActive(true);
            playerScript.CrosshairCentre.gameObject.SetActive(true);
            ReachSecondDefenseSpots();
        }

        public void OnSoldierReachMP()
        {
            if (baseAMeetupTimeline.state != PlayState.Playing)
            {
                soldiersOnMP++;
                if (soldiersOnMP == indianSoldiersCount && isPlayerOnMP)
                {
                    HideMessageText();
                    baseAMeetupTimeline.Play();
                }
            }
          
        }
        void SolveError()
        {
            HideMessageText();
            baseAMeetupTimeline.Play();
        }
        #endregion
        void ShowTaskSuggestion()
        {
            warningOrMsgText.text = "<color=grey>SUGGESTION:</color> PRESS 'T' TO VIEW TASKS";
            warningOrMsg.SetTrigger("Display");
        }
        public void IncrementKills()
        {
            CancelInvoke(nameof(ShowTaskSuggestion));
            Invoke(nameof(ShowTaskSuggestion), 15);
            kills += 1;
            if(kills == 1)
            {
                InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 0f, 4f);
            }
            if (kills == 10)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "08 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke(nameof(HideMessageText), 5f);
            }
            if (kills == 30)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "09 HOURS";
                messageTextGO.SetTrigger("Show");
                warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED: </color>CAPTURED BASE A";
                taskManager.NextTask();
                warningOrMsg.SetTrigger("Display");
             
                BaseProgress = "B";
                ReachMeetupPoint(0);
                CancelInvoke(nameof(SpawnAttackingBritainSoldier));
                playerMeetupPoint.gameObject.SetActive(true);
                playerMeetupPoint.onObjectTrigger += OnPlayerReachedMeetupPoint;
                arrowNav.gameObject.SetActive(true);
                arrowNav.target = playerMeetupPoint.transform;
                arrowNav.enabled = true;
                Invoke(nameof(ShowReachHint), 2);
                if(britishSoldierParentBaseA.childCount != 0)
                {
                    foreach(Transform t in britishSoldierParentBaseA)
                    {
                        Soldier s = t.GetComponent<Soldier>();
                        s.OnBulletShot(transform, 100, false);
                    }
                }
                
                britishSoldierParentBaseB.gameObject.SetActive(true);
                Invoke(nameof(HideMessageText), 5f);
            }
            if(kills == 31)
            {
                InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 0, 3);
            }
            if (kills == 35)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "10 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke(nameof(HideMessageText), 5f);
            }
        
            if (kills == 45)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "11 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke(nameof(HideMessageText), 5f);
            }
            if (kills == 70)
            {
                blackout.gameObject.SetActive(true);
                blackout.GetComponent<Animator>().SetTrigger("FadeOut");
                warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED: </color>CAPTURED THE INDO-BURMA BORDER";
                warningOrMsg.SetTrigger("Display");
                Invoke(nameof(EndAct), 5f);
            }
            streak += 1;

            if (streak >= 2)
            {
                ShowAppreciation("<color=yellow>" + streak + "X</color> Streak", streakIcon);
            }
        }
        void ShowReachHint()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK: </color> REACH THE MEET SPOT";
            taskManager.NextTask();
            warningOrMsg.SetTrigger("Display");
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

        public void BaseAMeetupFinished()
        {
            playerScript.gameObject.SetActive(true);
            ReachSecondDefenseSpots();
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
            PlayerPrefs.SetInt("ActNumber", 4);
            PlayerPrefs.SetString("SelectedAct", "Act4");
            PlayerPrefs.SetInt("Act3Completed", 1);
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
