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
    public class Act4Controller : MonoBehaviour
    {
        #region Setup
        public Transform playerTransform;

        public TaskManager taskManager;

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

        public Transform[] britainSpawns;
        public Transform britishSoldierParent;

        public TriggerEnter[] inaCoverPoints;

        public ParticleSystem dustStorm;
        public Animator camAnim;

        public Animator blackout;

        int difficulty;
      
        float prevLODBias;
        public ItemsAndIcons iai;
        void Awake()
        {
            GetComponent<AudioSource>().Play();
            settings.terrains[0].detailObjectDistance = 40;
            difficulty = PlayerPrefs.GetInt("Difficulty");
            Debug.Log("difficulty is " + difficulty);
            if (PlayerPrefs.GetString("DepthOfField") == "On")
            {
                settings.dofToggle.isOn = true;
                settings.volume.profile.TryGetSettings(out DepthOfField x);
                x.active = true;
                settings.volume.enabled = true;
                settings.layer.enabled = true;
                settings.noOfEffects += 1;
            }

            settings.GraphicsInfo.gameObject.SetActive(true);
            dustStorm.Play();
            playerScript = playerTransform.GetComponent<Player>();
            playerScript.onPlayerDied += OnPlayerDied;
            TimeLineDirector.gameObject.SetActive(true);
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
                TimeLineDirector.Play();
                TimeLineDirector.time = 6.4f;
                Cursor.lockState = CursorLockMode.Locked;
            }
            if(!TestWithoutControl)
            {
                Plane plane = Instantiate(helicopterPrototype, paths[0].startPoint).GetComponent<Plane>();
                plane.actNumber = 4;
                plane.endPoint = paths[0].endPoint;
                plane.playerScript = playerScript;
                plane.cam = camAnim;
                Invoke(nameof(SpawnSecondHelicopter), 3f);
            }
           
        }
        public void OnTimeLineEnd()
        {
            EnablePlayer();
            settings.GraphicsInfo.gameObject.SetActive(false);
        }
        public GameObject helicopterPrototype;
        void EnablePlayer()
        {
            playerTransform.gameObject.SetActive(true);
            settings.interactables.SetActive(true);
            TimeLineDirector.gameObject.SetActive(false);

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


            warningOrMsgText.text = "<color=red>WARNING:</color> BRITAIN AIRSTRIKE APPROACHING";
            warningOrMsg.SetTrigger("Display");
            messageTextGO.gameObject.SetActive(false);
            messageTextGO.gameObject.SetActive(true);
            messageText.text = "10 HOURS";
            messageTextGO.SetTrigger("Show");
            Invoke(nameof(InitializeDefense), 2f);
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
            Invoke(nameof(EndDustStorm), 5f);
        }
     
        void HideMessageText()
        {
            messageTextGO.SetTrigger("Hide");
        }
       
        void EndDustStorm()
        {
            dustStorm.Stop();
            
        }

        #endregion

        #region MainAct

        [Header("Act")]
        bool isActStarted = false;
        public int britishSoldierCount;
        public int kills;
        public Soldier[] indianSoldiers;
        public GameObject[] BritainSoldierPrototypes;
        readonly List<Soldier> britishSoldiers = new();
        public Transform indianSoldierParent;

        public Animator warningOrMsg;
        public TMP_Text warningOrMsgText;

        public bool ShowSubtitles;

        #region Helicopter
        [System.Serializable]
        public class HelicopterPath
        {
            public Transform startPoint;
            public Transform endPoint;
        }
        public List<HelicopterPath> paths = new List<HelicopterPath>();

        void SpawnHelicopter()
        {
            Plane plane = Instantiate(helicopterPrototype, paths[0].startPoint).GetComponent<Plane>();
            plane.actNumber = 4;
            plane.endPoint = paths[0].endPoint;
            plane.playerScript = playerScript;
            plane.cam = camAnim;
            warningOrMsgText.text = "<color=red>WARNING:</color> BRITAIN AIRSTRIKE APPROACHING";
            warningOrMsg.SetTrigger("Display");
            Invoke(nameof(ShowDestroyHint), 4f);
        }
        void ShowDestroyHint()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=orange>RECOMMEND:</color> DESTROY THE HELICOPTERS";
            warningOrMsg.SetTrigger("Display");
        }
        void SpawnSecondHelicopter()
        {
            Plane plane = Instantiate(helicopterPrototype, paths[1].startPoint).GetComponent<Plane>();
            plane.actNumber = 4;
            plane.endPoint = paths[1].endPoint;
            plane.playerScript = playerScript;
            plane.cam = camAnim;
            if(!isActStarted)
            {
                if (difficulty == 0)
                {
                    InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 4f, 4.5f);
                }
                else if (difficulty == 1)
                {
                    InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 4f, 3.5f);
                }
                else if (difficulty == 2)
                {
                    InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 4f, 2.7f);
                }
                isActStarted = true;
            }
        }

        #endregion
        void InitializeDefense() //for INA (this means attacking)
        {
            foreach(Soldier s in indianSoldiers)
            {
                s.a4Controller = this;
                s.IAI = iai;
                s.ReachSpot(s.coverPoint, true);
            }
            Invoke(nameof(ShowFirstTask), 4);
        }
        void ShowFirstTask()
        {
            warningOrMsgText.text = "<color=yellow>TASK:</color> SHIELD THE ATTACK";
            taskManager.enabled = true;
            warningOrMsg.SetTrigger("Display");
            Invoke(nameof(ShowDestroyHint), 4.2f);
        }
        void SpawnAttackingBritainSoldier()
        {
            if(kills + britishSoldierCount < 40 && britishSoldierCount < 15)
            {
                Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawns[Random.Range(0, britainSpawns.Length)].position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                britSoldier.fightMode = Soldier.FightMode.Attacking;
                britSoldier.a4Controller = this;
                britSoldier.player = playerTransform;
                britSoldier.indianSoldierParent = indianSoldierParent;
                britishSoldierCount++;
                britSoldier.Act = 4;
                britSoldier.IAI = iai;
                britishSoldiers.Add(britSoldier);
                britSoldier.InitTarget(playerTransform);
            }
        }
       
        public void SoldierDied(Soldier s, Soldier.Nationality nation, Transform from)
        {
            if (nation == Soldier.Nationality.British)
            {
                britishSoldierCount -= 1;
                britishSoldiers.Remove(s);
                if(from == playerTransform)
                {
                    IncrementKills();
                }
            }
                
        }
        int streak = 0;
        public void OnPlayerDied()
        {
            StopCoroutine(nameof(showNextDialogue));
            streak = 0;
            CancelInvoke(nameof(ShowTaskSuggestion));

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
            Invoke(nameof(ShowTaskSuggestion), 15);
            kills += 1;
            if (kills == 10)
            {
                Invoke(nameof(SpawnHelicopter), 2f);
                Invoke(nameof(SpawnSecondHelicopter), 5f);
            }
            if (kills == 20)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "11 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke(nameof(HideMessageText), 5f);
                Invoke(nameof(SpawnHelicopter), 3f);
                Invoke(nameof(SpawnSecondHelicopter), 5f);
            }
            if (kills == 30)
            {
                Invoke(nameof(SpawnHelicopter), 3f);
                Invoke(nameof(SpawnSecondHelicopter), 5f);
            }
            if(kills == 35)
            {
                dustStorm.Play();
                foreach(Soldier s in britishSoldiers)
                {
                    if(!s.isDead)
                    {
                        s.InitTarget(playerTransform);
                    }
                }
            }
            if(kills == 40)
            {
                Invoke(nameof(FinishAct), 2);
            }
            streak += 1;

            if (streak >= 2)
            {
                ShowAppreciation("<color=yellow>" + streak + "X</color> Streak", streakIcon);
            }
        }
        
        void FinishAct()
        {
            settings.interactables.SetActive(false);
            warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED:</color> DEFENDED THE ATTACK ";
            taskManager.NextTask();
            warningOrMsg.SetTrigger("Display");
            blackout.SetTrigger("FadeOut");
            foreach (Transform t in settings.otherPanels.transform)
            {
                if (t.name != "Blackout" && t != warningOrMsg.transform)
                {
                    t.gameObject.SetActive(false);
                }
            }
            Invoke(nameof(EndAct), 4.5f);
        }
        public void TreatPlayerAsTraitor()
        {
            if(!playerScript.isDead)
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
                StopCoroutine(nameof(showNextDialogue));
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
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;

        
            PlayerPrefs.SetInt("ActNumber", 5);
            PlayerPrefs.SetString("SelectedAct", "Act5");
            PlayerPrefs.SetInt("Act4Completed", 1);  //yes basically
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
