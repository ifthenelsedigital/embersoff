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
    public class Act6Controller : MonoBehaviour
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

        public Transform[] britainSpawnsArea1;
        public Transform[] britainSpawnsArea2;
        public Transform britishSoldierParent;

        public TriggerEnter[] inaCoverPoints;
        public GameObject[] objectsToDisable;

        public Animator camAnim;

        public Animator blackout;

        int difficulty;
      
        float prevLODBias;
        void Awake()
        {
            settings.musicSources[0].Play();
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
            foreach(Soldier s in britishSoldiers)
            {
                s.a6Controller = this;
                s.IAI = iai;
            }
            
            playerScript = playerTransform.GetComponent<Player>();
            playerScript.onPlayerDied += OnPlayerDied;
            TimeLineDirector.gameObject.SetActive(true);
            indianSoldierParent.gameObject.SetActive(false);
            playerScript.playerSpawn = playerSpawns[0];
            settings.GraphicsInfo.gameObject.SetActive(true);
            foreach (GameObject g in objectsToDisable)
            {
                g.SetActive(true);
            }
            if(!isInTestMode)
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(false);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                britishSoldierParent.gameObject.SetActive(false);
                indianSoldierParent.gameObject.SetActive(false);
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
                Time.timeScale = 8;
            
                Cursor.lockState = CursorLockMode.Locked;
            }
      
        }
        public Transform playerStartPoint;
        public GameObject retreatLimiter;
        public void OnTimeLineEnd()
        {
            settings.GraphicsInfo.gameObject.SetActive(false);
            foreach(GameObject g in objectsToDisable)
            {
                g.SetActive(false);
            }
            playerTransform.position = playerStartPoint.position;
            EnablePlayer();
        }
        void EnablePlayer()
        {
            #region Basic Sets
            retreatLimiter.SetActive(true);
            playerTransform.gameObject.SetActive(true);
            settings.interactables.SetActive(true);
            indianSoldierParent.gameObject.SetActive(true);
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

          
            messageTextGO.gameObject.SetActive(false);
            messageTextGO.gameObject.SetActive(true);
            messageText.text = "10 HOURS";
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
            #endregion

            foreach(Soldier t in indianSoldiers)
            {
                t.transform.position = t.meetupPoints[0].transform.position;
            }
            indianSoldierParent.gameObject.SetActive(true);

            AreaProgress = 0;
            SoldiersInAreas[AreaProgress].nextAreaLimiter.SetActive(true);
            ReachStandPoint();
        }

        void HideMessageText()
        {
            messageTextGO.SetTrigger("Hide");
        }
       
   
        #endregion

        #region MainAct

        [Header("Act")]
        public int britishSoldierCount;
        public int kills;
        public GameObject[] plantedBombs;
        public Transform[] playerSpawns;
        public Soldier[] indianSoldiers;
        public GameObject[] BritainSoldierPrototypes;
        public List<Soldier> britishSoldiers = new();
        public Transform indianSoldierParent;

        public bool ShowSubtitles;

        public Animator warningOrMsg;
        public TMP_Text warningOrMsgText;
        Soldier Bhavya;
        Soldier Mihika;
        int SoldiersAtGate = 0;
        void ReachStandPoint() //for Mihika and bhavya
        {
            foreach (Soldier s in indianSoldiers)
            {
                if(s.SoldierName == "MIHIKA")
                {
                    Mihika = s;
                    s.onReachCoverPoint += SoldierReachStandupPoint; //a jugaad for saving variables 
                    s.anim.SetBool("IsCrouching", false);
                    s.ReachSpot(s.coverPoint, true);
                }
                else if(s.SoldierName == "BHAVYA")
                {
                    Bhavya = s;
                    s.onReachCoverPoint += SoldierReachStandupPoint; //a jugaad for saving variables 
                    s.anim.SetBool("IsCrouching", false);
                    s.ReachSpot(s.coverPoint, true);
                }
                s.a6Controller = this;
            }
        }
        public GameObject emergencyLight;
        void SoldierReachStandupPoint()
        {
            SoldiersAtGate++;
            if(SoldiersAtGate == 2)
            {
                Time.timeScale = 1;
                Bhavya.onReachCoverPoint -= SoldierReachStandupPoint; //a jugaad for saving variables 
                Mihika.onReachCoverPoint -= SoldierReachStandupPoint; //a jugaad for saving variables 
                Bhavya.target = Mihika.transform;
                Mihika.target = Bhavya.transform;
                Bhavya.isAttacking = true;
                Mihika.isAttacking = true;
                Bhavya.anim.SetBool("IsCrouching", true);
                Mihika.anim.SetBool("IsCrouching", true);

                IntroPlantHint.transform.parent.gameObject.SetActive(true);
                IntroPlantHint.onObjectTrigger += PlayerReachPlantSpot;
                IntroPlantHint.onObjectExit += PlayerLeavePlantSpot;
                warningOrMsg.SetTrigger("Hide");
                warningOrMsgText.text = "<color=yellow>TASK:</color> PLANT BOMB AT THE DOOR";
                warningOrMsg.SetTrigger("Display");
                taskManager.enabled = true;
                isIntro = true;
            }

      
        }
        public TaskManager taskManager;
        [Header("Intro Vars")]
        public GameObject IntroPlantedBomb;
        public TriggerEnter IntroPlantHint;
        public GameObject detonatePanel;
        bool canDetonate = false;
        bool isIntro = true;
        public GameObject[] Doors; //0 should be original and 1 should be broken
        public Rigidbody[] DoorRbs;
        public GameObject[] ExplosionVFXs; //0 is explosion and 1 is smoke

        void InitializeDefense() //for INA (this means attacking)
        {
            britishSoldierParent.gameObject.SetActive(true);
            foreach (Soldier s in britishSoldiers)
            {
                if (s.AreaSecurityCode == 0)
                {
                    s.isReadyToDefend = true;
                }
            }
            foreach(Soldier s in indianSoldiers)
            {
                s.anim.SetBool("IsCrouching", false);
                s.ReachSpot(s.coverPoint2, true);
            }
            Invoke(nameof(DestroyIndianSoldiers), 10);
            if (difficulty == 0)
            {
                InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 1f, 6f);
            }
            else if (difficulty == 1)
            {
                InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 1f, 5f);
            }
            else if (difficulty == 2)
            {
                InvokeRepeating(nameof(SpawnAttackingBritainSoldier), 1f, 3f);
            }
            ShowFirstTask();
        }
        public int AreaProgress = 0;
        void DestroyIndianSoldiers()
        {
            Destroy(indianSoldierParent.gameObject);
        }
        void ShowFirstTask()
        {
            taskManager.NextTask();
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> CLEAR THE ENTRANCE HALL";
            warningOrMsg.SetTrigger("Display");
            Invoke(nameof(DeclareEmergency), 3);
        }
       
        void DeclareEmergency()
        {
            emergencyLight.SetActive(true);
        }
        public ItemsAndIcons iai;
        void SpawnAttackingBritainSoldier()
        {
            if (AreaProgress == 0 && kills + SoldiersInAreas[0].CurrentSoldierCount < 15)
            {
                Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsArea1[Random.Range(0, britainSpawnsArea1.Length)].position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                britSoldier.fightMode = Soldier.FightMode.Attacking;
                britSoldier.a6Controller = this;
                britSoldier.IAI = iai;
                britSoldier.player = playerTransform;
                britSoldier.indianSoldierParent = indianSoldierParent;
                britSoldier.AreaSecurityCode = 0;
                britishSoldierCount++;
                britSoldier.Act = 6;
                britishSoldiers.Add(britSoldier);
                britSoldier.InitTarget(playerTransform);
                SoldiersInAreas[0].CurrentSoldierCount++;
            }

            else if (AreaProgress == 1 && kills + SoldiersInAreas[1].CurrentSoldierCount < 8)
            {
                Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsArea2[Random.Range(0, britainSpawnsArea2.Length)].position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                britSoldier.fightMode = Soldier.FightMode.Attacking;
                britSoldier.a6Controller = this;
                britSoldier.IAI = iai;
                britSoldier.player = playerTransform;
                britSoldier.AreaSecurityCode = 1;
                britSoldier.indianSoldierParent = indianSoldierParent;
                britishSoldierCount++;
                britSoldier.Act = 6;
                britishSoldiers.Add(britSoldier);
                SoldiersInAreas[0].CurrentSoldierCount++;
                britSoldier.InitTarget(playerTransform);

            }

            if(AreaProgress == 3)
            {
                Soldier britSoldier = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], LastSoldierSpawn.position, Quaternion.identity, britishSoldierParent).GetComponent<Soldier>();
                britSoldier.fightMode = Soldier.FightMode.Attacking;
                britSoldier.a6Controller = this;
                britSoldier.IAI = iai;
                britSoldier.player = playerTransform;
                britSoldier.indianSoldierParent = indianSoldierParent;
                britishSoldierCount++;
                britSoldier.Act = 6;
                britSoldier.AreaSecurityCode = 3;
                britishSoldiers.Add(britSoldier);
                SoldiersInAreas[0].CurrentSoldierCount++;
                britSoldier.InitTarget(playerTransform);
            }
          
        }
        public Transform LastSoldierSpawn;
        public void SoldierDied(Soldier s, Soldier.Nationality nation, Transform from, int AreaCode)
        {
            if (nation == Soldier.Nationality.British)
                {
                    britishSoldiers.Remove(s);
                    if (from == playerTransform)
                    {
                        IncrementKills();
                    }

                    SoldiersInArea soldiersInArea = SoldiersInAreas[s.AreaSecurityCode];
                    soldiersInArea.CurrentSoldierCount--;
                    if (soldiersInArea.CurrentSoldierCount == 0)
                    {
                        if (AreaProgress == 0)
                        {
                            warningOrMsg.SetTrigger("Hide");
                            warningOrMsgText.text = "<color=yellow>TASK: </color>PLANT BOMB AT THE SPOT.";
                            warningOrMsg.SetTrigger("Display");
                            PlantHints[AreaProgress].transform.parent.gameObject.SetActive(true);
                            PlantHints[AreaProgress].onObjectTrigger += PlayerReachPlantSpot;
                            PlantHints[AreaProgress].onObjectExit += PlayerLeavePlantSpot;

                        taskManager.NextTask();
                        }
                        if (AreaProgress == 1)
                        {
                            warningOrMsg.SetTrigger("Hide");
                            warningOrMsgText.text = "<color=yellow>TASK: </color>PLANT BOMB AT THE SPOT.";
                            warningOrMsg.SetTrigger("Display");
                            PlantHints[AreaProgress].transform.parent.gameObject.SetActive(true);
                            PlantHints[AreaProgress].onObjectTrigger += PlayerReachPlantSpot;
                            PlantHints[AreaProgress].onObjectExit += PlayerLeavePlantSpot;
                        taskManager.NextTask();

                    }
                    else if (AreaProgress == 2)
                        {
                            warningOrMsg.SetTrigger("Hide");
                            warningOrMsgText.text = "<color=yellow>TASK: </color>PLANT BOMB AT THE SPOT.";
                            warningOrMsg.SetTrigger("Display");
                            PlantHints[AreaProgress].transform.parent.gameObject.SetActive(true);
                            PlantHints[AreaProgress].onObjectTrigger += PlayerReachPlantSpot;
                            PlantHints[AreaProgress].onObjectExit += PlayerReachPlantSpot;
                            CancelInvoke(nameof(SpawnAttackingBritainSoldier));
                        taskManager.NextTask();
                    }

                    if (AreaProgress != 3)
                    {
                        arrowNav.enabled = false;
                        arrowNav.target = PlantHints[AreaProgress].transform;
                        arrowNav.enabled = true;
                    }
                        
                    }


                    else
                    {
                        britishSoldierCount -= 1;
                    }


                }
        }
        public GameObject doorEnterHint;
        public TriggerEnter endTrigger;

        #region Bombs
        public GameObject bombPlantPanel;
        public PressChecker pressChecker; //for checking if p is pressed
        void PlayerReachPlantSpot()
        {
            bombPlantPanel.SetActive(true);
            pressChecker.onKeyPressed += OnBombPlanted;
            pressChecker.Check = true;
        }
        void PlayerLeavePlantSpot()
        {
            bombPlantPanel.SetActive(false);
            pressChecker.onKeyPressed -= OnBombPlanted;
            pressChecker.Check = false;
        }
        string currentMsg;
        void ShowMsg()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = currentMsg;
            warningOrMsg.SetTrigger("Display");
        }
        void OnBombPlanted()
        {
            pressChecker.onKeyPressed -= OnBombPlanted;
            pressChecker.Check = false;
            bombPlantPanel.SetActive(false);
            arrowNav.target = null;
            if (!isIntro)
            {
                plantedBombs[AreaProgress].SetActive(true);
                PlantHints[AreaProgress].onObjectTrigger -= PlayerReachPlantSpot;
                PlantHints[AreaProgress].onObjectExit -= PlayerLeavePlantSpot;
                PlantHints[AreaProgress].transform.parent.gameObject.SetActive(false);
               
                SoldiersInAreas[AreaProgress].nextAreaLimiter.SetActive(false);
                AreaProgress++;
                warningOrMsg.SetTrigger("Hide");
                warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED: </color>" + AreaProgress + "/3 BOMBS PLANTED";
                warningOrMsg.SetTrigger("Display");
                if (AreaProgress == 1)
                {
                    doorEnterHint.SetActive(true);
                    messageText.text = "ENTER THE CORRIDOR";
                    messageTextGO.SetTrigger("Show");
                    Invoke(nameof(HideMessageText), 5);
                    playerScript.playerSpawn = playerSpawns[1];
                    foreach (Soldier s in britishSoldiers)
                    {
                        if (s.AreaSecurityCode == 1)
                        {
                            s.isReadyToDefend = true;
                        }
                    }

                    currentMsg = "<color=yellow>TASK:</color> ENTER AND CLEAR THE CORRIDOR";
                    taskManager.NextTask();
                    Invoke(nameof(ShowMsg), 4);
                }
                if (AreaProgress == 2)
                {
                    doorEnterHint.SetActive(false);
                    messageText.text = "ENTER THE SC STORAGE";
                    messageTextGO.SetTrigger("Show");
                    Invoke(nameof(HideMessageText), 5);
                    playerScript.playerSpawn = playerSpawns[2];
                    foreach (Soldier s in britishSoldiers)
                    {
                        if (s.AreaSecurityCode == 2)
                        {
                            s.isReadyToDefend = true;
                        }
                    }
                    currentMsg = "<color=yellow>TASK:</color> ENTER AND CLEAR THE SC STORAGE";
                    taskManager.NextTask();
                    Invoke(nameof(ShowMsg), 4);
                    emergencyLight.SetActive(false);
                }
                if (AreaProgress == 3)
                {
                    messageText.text = "FIND VICEROY";
                    messageTextGO.SetTrigger("Show");
                    currentMsg = "<color=yellow>TASK:</color> FIND THE VICEROY";
                    taskManager.NextTask();
                    Invoke(nameof(ShowMsg), 4);
                    Invoke(nameof(HideMessageText), 5);
                    foreach (Soldier s in britishSoldiers)
                    {
                        if (s.AreaSecurityCode == 3)
                        {
                            s.isReadyToDefend = true;
                        }
                    }
                    SpawnAttackingBritainSoldier();
                    endTrigger.onObjectTrigger += PlayerReachEndScene;
                }
            }

            else
            {
                warningOrMsg.SetTrigger("Hide");
                IntroPlantedBomb.SetActive(true);
                IntroPlantHint.onObjectTrigger -= PlayerReachPlantSpot;
                IntroPlantHint.onObjectExit -= PlayerLeavePlantSpot;
                Invoke(nameof(ShowDetonateHint), 3f);
                taskManager.NextTask();
            }
           
        }
        void ShowDetonateHint()
        {
            detonatePanel.SetActive(true);
            canDetonate = true;
        }
        public Transform forceLocation;
        private void Update()
        {
            if(canDetonate)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    canDetonate = false;
                    isIntro = false;

                    IntroPlantHint.transform.parent.gameObject.SetActive(false);
                    Destroy(IntroPlantedBomb);
                    
                    detonatePanel.SetActive(false);
                    foreach (GameObject g in ExplosionVFXs)
                    {
                        g.SetActive(true);
                    }
                    camAnim.SetTrigger("ExplodeShake");

                    Doors[0].SetActive(false);
                    Doors[1].SetActive(true);

                    foreach(Rigidbody rb in DoorRbs)
                    {
                        rb.AddForce(forceLocation.right * 50, ForceMode.Impulse);
                    }

                    settings.musicSources[0].Stop();
                    Invoke(nameof(InitializeDefense), 3.5f);
                }
            }
        }
        #endregion

        int streak = 0;
        public void OnPlayerDied()
        {
            CancelInvoke(nameof(ShowTaskSuggestion));
            StopCoroutine(nameof(ShowNextDialogue));
            streak = 0;
            bombPlantPanel.SetActive(false);
            if (ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }

        }
        public ParticleSystem dustStorm;
        void ShowTaskSuggestion()
        {
            warningOrMsgText.text = "<color=grey>SUGGESTION:</color> PRESS 'T' TO VIEW TASKS";
            warningOrMsg.SetTrigger("Display");
        }
        public void IncrementKills()
        {
            kills += 1;
         
            if(kills == 3)
            {
                dustStorm.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (kills == 14)
            {
                messageTextGO.gameObject.SetActive(false);
                messageTextGO.gameObject.SetActive(true);
                messageText.text = "09 HOURS";
                messageTextGO.SetTrigger("Show");
                Invoke(nameof(HideMessageText), 5f);
                
            }
          
            streak += 1;

            if (streak >= 2)
            {
                ShowAppreciation("<color=yellow>" + streak + "X</color> Streak", streakIcon);
            }

            if(kills > 1)
            {
                CancelInvoke(nameof(ShowTaskSuggestion));
                Invoke(nameof(ShowTaskSuggestion), 18);
            }
        }
        public ArrowIndicator arrowNav;
        public void TreatPlayerAsTraitor()
        {
            warningOrMsgText.text = "<color=red>WARNING: </color>DON'T SHOOT AT YOUR MATES!";
            warningOrMsg.SetTrigger("Display");
            CancelInvoke(nameof(ShowTaskSuggestion));
        }

        #endregion

        #region Area Defence Management
        [System.Serializable]
        public class SoldiersInArea
        {
            public int AreaCode;
            public int DefaultSoldierCount;
            public int CurrentSoldierCount;
            public GameObject nextAreaLimiter;
        }
        [Header("Area Defence")]
        [Space(5f)]
        public SoldiersInArea[] SoldiersInAreas;
        public TriggerEnter[] PlantHints;

        #endregion

        #region EndScene
        void PlayerReachEndScene()
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
            messageText.text = null;
            warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED: </color>FOUND THE VICEROY";
            warningOrMsg.SetTrigger("Display");
            Invoke(nameof(FadeOut), 3f);
        }
        void FadeOut()
        {
            blackout.SetTrigger("FadeOut");
            Invoke(nameof(EndAct), 2f);
        }
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

            string month;
            if (System.DateTime.Today.Month < 10)
            {
                month = "0" + System.DateTime.Today.Month;
            }
            else
            {
                month = System.DateTime.Today.Month.ToString();
            }

            if (difficulty == 0)
            {
                PlayerPrefs.SetString("CompletedInEasy", System.DateTime.Today.Day + "/" + month);
            }
            else if(difficulty == 1)
            {
                PlayerPrefs.SetString("CompletedInFierce", System.DateTime.Today.Day + "/" + month);
            }
            else if (difficulty == 2)
            {
                PlayerPrefs.SetString("CompletedInReality", System.DateTime.Today.Day + "/" + month);
            }
            PlayerPrefs.SetString("SelectedAct", "End");
            PlayerPrefs.SetInt("Act6Completed", 7);
            PlayerPrefs.SetInt("ActNumber", 1);
            SceneManager.LoadScene("Story");
        }
      
        #endregion

        #region Dialogues
        [Space(5f)]
        [Header("Dialogues")]
        public GameObject dialogueBox;
        public Transform dialogueContainer;
        public AudioSource dialoguePlayer;
        public List<AudioClip> EnglishDialogues = new();
        IEnumerator ShowNextDialogue(float waitTime, int DialogueClipIndex, int senderType, string message, string senderName, bool isAbusive, Act1Soldier soldier)
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
