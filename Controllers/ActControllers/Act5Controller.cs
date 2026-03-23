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
    public class Act5Controller : MonoBehaviour
    {
        #region Setup
        public Transform playerTransform;

        public bool useAbusiveWords = true;

        public GameObject nearbyPanel;
        public Animator nearbyPanelAnim;

        public GameObject[] IntroObjects;
        public GameObject[] GroundFloorObjects;
        public GameObject[] LowerBasementObjects;
        public GameObject[] UpperBasementObjects;
        public GameObject[] ExteriorEnvironment;

        public Animator messageTextGO;
        public TMP_Text messageText;
        public PlayableDirector TimeLineDirector;

        public Settings settings;

        public Player playerScript;
        public Transform[] playerSpawns;

        public bool DirectlyEnterMainShip = false;

        [Range(0, 2)]
        public int Progress = 0; //0 - Lower Basement, 1 - Upper Basement, 2 - Ground floor

        public Transform[] britainSpawnsLowerBasement;
        public Transform[] britainSpawnsUpperBasement;
        public Transform[] britainSpawnsGround;

        public Transform britishSoldierParentLB; //there;s a reason
        public GameObject britishSoldierParentUB;
        public GameObject britishSoldierParentGroundFloor;

        public Transform britishSoldierParentPrimaryShip;

        public TriggerEnter[] inaCoverPoints;

        public Animator camAnim;

        public Animator blackout;
        Dictionary<int, int> SoldiersInAreas = new();
        public int SoldiersInLB;
        public int SoldiersInUB;
        public int SoldiersInGF;

        int difficulty;
      
        float prevLODBias;
        public ItemsAndIcons iai;
        void Awake()
        {
            difficulty = PlayerPrefs.GetInt("Difficulty");
            Debug.Log("difficulty is " + difficulty);
            OptimizeGroundFloor();
            foreach (Soldier s in indianSoldiers)
            {
                s.a5Controller = this;
                s.IAI = iai;
                s.britainSoldierParent = britishSoldierParentPrimaryShip;
            }
            britishSoldierParentLB.gameObject.SetActive(false);
            
            SoldiersInAreas.Add(0, SoldiersInLB);
            SoldiersInAreas.Add(1, SoldiersInUB);
            SoldiersInAreas.Add(2, SoldiersInGF);
           
            if (PlayerPrefs.GetString("DepthOfField") == "On")
            {
                settings.dofToggle.isOn = true;
                settings.volume.profile.TryGetSettings(out DepthOfField x);
                x.active = true;
                settings.volume.enabled = true;
                settings.layer.enabled = true;
                settings.noOfEffects += 1;
            }

            rainScript.RainIntensity = 1; //for outside
            playerScript = playerTransform.GetComponent<Player>();
            playerScript.onPlayerDied += OnPlayerDied;


            foreach (Soldier s in britainSoldiers)
            {      
                s.a5Controller = this;
                s.IAI = iai;
            }
            britishSoldierParentPrimaryShip.gameObject.SetActive(false);
            TimeLineDirector.gameObject.SetActive(true);
            settings.GraphicsInfo.gameObject.SetActive(true);
            Invoke(nameof(HideGraphicInfo), 9);
            if (!DirectlyEnterMainShip)
            {
                settings.interactables.SetActive(false);
                playerTransform.gameObject.SetActive(true);
                PlayerCam.enabled = false;
                playerScript.AcceptMovement = false;
                foreach(Soldier s in indianSoldiers)
                {
                    s.nav.enabled = false;
                }
                playerScript.doContinousRaycasting = false;
                weaponsCam.enabled = false;
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                playerScript.AcceptFireControls = false;
                playerScript.AcceptMovement = false;
                if(fastForwardIntro)
                {
                    Time.timeScale = 5;
                    Time.fixedDeltaTime = 0.03f;
                }
                TimeLineDirector.Play();
                Cursor.lockState = CursorLockMode.Locked;
            }

            else
            {
                weaponsCam.gameObject.SetActive(true);
                settings.interactables.SetActive(true);
                playerTransform.gameObject.SetActive(true);
                PlayerCam.gameObject.SetActive(true);
                prevLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 1.2f;
                BlackoutScene();
                Cursor.lockState = CursorLockMode.Locked;
            }
            

        }
        void HideGraphicInfo()
        {
            settings.GraphicsInfo.gameObject.SetActive(false);
        }
        public GameObject introCam;
        public void OnTimeLineEnd()
        {
            introCam.SetActive(false);
            PlayerCam.gameObject.SetActive(true);
            playerScript.doContinousRaycasting = true;
            britishSoldierParentPrimaryShip.gameObject.SetActive(true);
            playerScript.foreverInvincible = true;
            playerScript.weaponsAnim.Play("Open");
         
            PlayerCam.enabled = true;
            weaponsCam.enabled = true;
            #region Basic Sets
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

         
            messageTextGO.gameObject.SetActive(false);
            messageTextGO.gameObject.SetActive(true);
            messageText.text = "16 HOURS";
            messageTextGO.SetTrigger("Show");
            if (!DirectlyEnterMainShip)
            {
                playerScript.AcceptMovement = false;
                playerScript.AcceptFireControls = true;
            }
            else
            {
                playerScript.AcceptMovement = true;
                playerScript.AcceptFireControls = true;
            }
        
            Invoke(nameof(HideMessageText), 5f);
            #endregion

                                             DefaultLayerMask = PlayerCam.cullingMask;
                                             // PlayerCam.cullingMask = NoWaterMask;
            
            explosivesTrigger.enabled = false;
            UBSoldiers.SetActive(false);
            LBSoldiers.SetActive(false);
            GFSoldiers.SetActive(false);
           
            playerScript.foreverInvincible = true; //for sometime
            Invoke(nameof(ShowPrimaryShipTask), 9f);
            
        }
        public GameObject innerwall;
        public Camera PlayerCam;
        public Camera weaponsCam;
        LayerMask DefaultLayerMask;
        public LayerMask NoWaterMask;
        void HideMessageText()
        {
            messageTextGO.SetTrigger("Hide");
        }
        public DigitalRuby.RainMaker.RainScript rainScript;


        #endregion

        #region MainAct

        [Header("Act")]
        public bool fastForwardIntro = false; 
        bool isActStarted = false;
        public int kills;
        public List<Soldier> indianSoldiers = new();
        public GameObject[] BritainSoldierPrototypes;
        public Soldier[] britainSoldiers;
        public Transform indianSoldierParent;

        public GameObject LBSoldiers;
        public GameObject UBSoldiers;
        public GameObject GFSoldiers;

        public bool ShowSubtitles;

        public Animator warningOrMsg;
        public TMP_Text warningOrMsgText;
        public TriggerEnter upperBasementTrigger;
        public TriggerEnter lowerBasementTrigger;
        public TriggerEnter groundFloorTrigger;

        public PathTracer[] speedBoats;
        float prevVolume;

        #region Optimization
       
        void OptimizeLowerBasement()
        {
            
            foreach (GameObject f in UpperBasementObjects)
            {
                f.SetActive(false);
            }
            foreach (GameObject h in GroundFloorObjects)
            {
                h.SetActive(false);
            }
            foreach (GameObject g in LowerBasementObjects)
            {
                g.SetActive(true);
            }
            foreach (GameObject g in IntroObjects)
            {
                if (g.name != "OuterSurface" || g.name.Contains("SpeedBoat"))
                {
                    g.SetActive(false);
                }
                else
                {
                    g.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
        void OptimizeUpperBasement()
        {
            foreach (GameObject g in UpperBasementObjects)
            {
                g.SetActive(true);
            }
            foreach (GameObject f in LowerBasementObjects)
            {
                f.SetActive(false);
            }
            foreach (GameObject h in GroundFloorObjects)
            {
                h.SetActive(false);
            }
        }
        void OptimizeGroundFloor()
        {
            foreach (GameObject g in LowerBasementObjects)
            {
                g.SetActive(false);
            }
            foreach (GameObject f in UpperBasementObjects)
            {
                f.SetActive(false);
            }
            foreach (GameObject h in GroundFloorObjects)
            {
                h.SetActive(true);
            }
        }
        #endregion

        void ShowPrimaryShipTask()
        {
            warningOrMsgText.text = "<color=yellow>TASK: </color>ASSASSINATE THE NAVAL SOLDIERS";
            taskManager.enabled = true;
            warningOrMsg.SetTrigger("Display");
            speedBoats[0].onReachDetonateSpot += ShowDetonateHint;
        }
        void ShowDetonateHint()
        {
            playerScript.AcceptFireControls = false;
            warningOrMsgText.text = "<color=yellow>TASK: </color>DETONATE THE BOMB";
            taskManager.NextTask();
            warningOrMsg.SetTrigger("Display");
            settings.interactables.SetActive(false);
            detonatePanel.SetActive(true);
            canDetonate = true;

        }
        void OnBoatReachMainShip()
        {
            Invoke(nameof(BlackoutScene), 2.4f);
            playerScript.AcceptFireControls = false;
            prevVolume = AudioListener.volume;
        }
        public AudioSource RopeSound;
        void BlackoutScene()
        {
            if(!DirectlyEnterMainShip)
            {
                AudioListener.volume = 0.5f;
                settings.musicSources[0].Pause();
                playerScript.foreverInvincible = false;
            }
          
            blackout.gameObject.SetActive(true);
            blackout.SetTrigger("FadeOut");
            rainScript.RainIntensity = 0.5f; //for basement
            settings.musicSources[0].Pause();
            playerScript.playerSpawn = playerSpawns[1];
            List<Soldier> toBeRemovedSoldiers = new List<Soldier>();
            foreach(Soldier s in indianSoldiers)
            {
                s.transform.SetParent(null);
                if(s.coverPoint != null)
                {
                    s.transform.position = s.coverPoint.transform.position; //the LB start points
                }
                else
                {
                    toBeRemovedSoldiers.Add(s);
                    Destroy(s.gameObject);
                }
            }

            foreach(Soldier r in toBeRemovedSoldiers)
            {
                indianSoldiers.Remove(r);
            }
            toBeRemovedSoldiers = null;
            Invoke(nameof(StartMainAct), 4);
            RopeSound.Play();
        }
        void StartMainAct()
        {
            playerTransform.SetParent(playerSpawns[0]);
            FirstPersonController fpc = playerScript.MainController;
            fpc.enabled = false;
            fpc.m_MouseLook.m_CharacterTargetRot = Quaternion.Euler(Vector3.zero);
            fpc.m_MouseLook.m_CameraTargetRot = Quaternion.Euler(Vector3.zero);
            PlayerCam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            blackout.SetTrigger("FadeIn");
            playerTransform.position = playerSpawns[0].position;
            fpc.enabled = true;
            if(!DirectlyEnterMainShip)
            {
                AudioListener.volume = prevVolume;
            }
            OptimizeLowerBasement();

            settings.musicSources[0].time = 0;
            settings.musicSources[0].Play();
            settings.musicSources[0].volume = prevMusicVolume - 28/100 * prevMusicVolume;

            playerScript.AcceptFireControls = true;
            InitializeDefense();
            if(fastForwardIntro)
            {
                Time.timeScale = 1;
                Time.fixedDeltaTime = 0.02f;
            }
            britishSoldierParentLB.gameObject.SetActive(true);
            playerScript.weaponsAnim.Play("Open");
        }
        public GameObject LBUBPreventer;
        public GameObject UBLBPreventer;
        public GameObject UBGFPreventer;
        public GameObject GFUBPreventer;
        public void SpawnAttackers()
        {
            if(Progress == 0)
            {
                if(kills + SoldiersInLB <= 17)
                {
                    Soldier s = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsLowerBasement[Random.Range(0, britainSpawnsLowerBasement.Length)]).GetComponent<Soldier>();
                    s.a5Controller = this;
                    s.indianSoldierParent = indianSoldierParent;
                    s.Act = 5;
                    s.IAI = iai;
                    s.AreaSecurityCode = 0;
                    s.fightMode = Soldier.FightMode.Attacking;
                    s.player = playerTransform;
                    s.InitTarget(playerTransform);
                    SoldiersInAreas[Progress]++;
                }
             
            }
            else if (Progress == 1)
            {
                 if(kills + SoldiersInUB <= 25)
                 {
                    Soldier s = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsUpperBasement[Random.Range(0, britainSpawnsUpperBasement.Length)]).GetComponent<Soldier>();
                    s.a5Controller = this;
                    s.indianSoldierParent = indianSoldierParent;
                    s.Act = 5;
                    s.IAI = iai;
                    s.AreaSecurityCode = 1;
                    s.fightMode = Soldier.FightMode.Attacking;
                    s.player = playerTransform;
                    s.InitTarget(playerTransform);
                    SoldiersInAreas[Progress]++;
                 }
                  
                
            }
            else if (Progress == 2)
            {
                if(kills + SoldiersInGF <= 44)
                {
                    Soldier s = Instantiate(BritainSoldierPrototypes[Random.Range(0, BritainSoldierPrototypes.Length)], britainSpawnsGround[Random.Range(0, britainSpawnsGround.Length)]).GetComponent<Soldier>();
                    s.a5Controller = this;
                    s.Act = 5;
                    s.IAI = iai;
                    s.AreaSecurityCode = 2;
                    s.indianSoldierParent = indianSoldierParent;
                    s.fightMode = Soldier.FightMode.Attacking;
                    s.player = playerTransform;
                    s.InitTarget(playerTransform);
                    SoldiersInAreas[Progress]++;
                }
                   
                
            }
        }
        public GameObject detonatePanel;
        bool canDetonate = false;
        public int britainSoldierCountPrShip;
        public GameObject[] explosionVFX;
        private void Update()
        {
            if (canDetonate)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    settings.musicSources[0].Pause();
                    foreach(GameObject e in explosionVFX)
                    {
                        e.SetActive(true);
                    }
                    foreach (PathTracer pt in speedBoats)
                    {
                        pt.IsDetonated = true;
                        if(pt.isPlayerBoat)
                        {
                            pt.onReachEndPoint += OnBoatReachMainShip;
                        }
                    }
                    camAnim.SetTrigger("ExplosionShake");
                    ShowTopCameraShot();
                    canDetonate = false;
                }
            }
        }
        public GameObject topCam;
        void ShowTopCameraShot()
        {
            playerTransform.gameObject.SetActive(false);
            Destroy(britishSoldierParentPrimaryShip.gameObject);
            topCam.SetActive(true);
            detonatePanel.SetActive(false);
            Invoke(nameof(ReturnToNormalView), 5f);
        }
        void ReturnToNormalView()
        {
            playerTransform.gameObject.SetActive(true);
            topCam.SetActive(false);
            playerScript.AcceptFireControls = true;
            settings.interactables.SetActive(true);
            settings.musicSources[0].time = 48;
            settings.musicSources[0].Play();
            prevMusicVolume = settings.musicSources[0].volume;
            settings.musicSources[0].volume = 1f;
        }
        float prevMusicVolume;
        void InitializeDefense() //for INA (this means attacking) jugaad hehe
        {
            britishSoldierParentLB.gameObject.SetActive(true);
            foreach(Soldier s in indianSoldiers)
            {
               
                    s.nav.enabled = true;
                    s.anim.enabled = true;
                    s.britainSoldierParent = britishSoldierParentLB;
                    s.ReachSpot(s.coverPoint2, true);
                
               
            }
            Invoke(nameof(ShowFirstTask), 1);
            
        }
        public TaskManager taskManager;
        void ShowFirstTask()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> CLEAR THE LOWER BASEMENT";
           
            warningOrMsg.SetTrigger("Display");
            playerScript.AcceptMovement = true;
            playerScript.AcceptFireControls = true;

            GFPreTrigger.onObjectTrigger += OptimizeGroundFloor;

            taskManager.NextTask();
        }
        public TriggerEnter GFPreTrigger;
        public void SoldierDied(Soldier s, Soldier.Nationality nation, Transform from, int AreaCode)
        {
            if (nation == Soldier.Nationality.British)
            {
                SoldiersInAreas[AreaCode]--;
                if(SoldiersInAreas[AreaCode] == 0)
                {
                    if(AreaCode == 0)
                    {
                        warningOrMsg.SetTrigger("Hide");
                        warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED:</color> CLEARED LOWER BASEMENT";
                        warningOrMsg.SetTrigger("Display");

                        UBSoldiers.SetActive(true);
                        LBSoldiers.SetActive(false);
                        GFSoldiers.SetActive(false);

                        Progress++;
                        Invoke(nameof(ShowUBHint), 4f);
                        upperBasementTrigger.onObjectTrigger += PlayerReachUpperBasement;

                        LBUBPreventer.SetActive(false);
                        UBGFPreventer.SetActive(true);
                        CancelInvoke(nameof(SpawnAttackers));
                    }
                    else if (AreaCode == 1)
                    {
                        warningOrMsg.SetTrigger("Hide");
                        warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED:</color> CLEARED UPPER BASEMENT";
                        warningOrMsg.SetTrigger("Display");
                        Invoke(nameof(ShowGroundFloorHint), 4f);

                        UBSoldiers.SetActive(false);
                        LBSoldiers.SetActive(false);
                        GFSoldiers.SetActive(true);

                        Progress++;
                        
                        groundFloorTrigger.onObjectTrigger += PlayerReachGroundFloor;

                        UBGFPreventer.SetActive(false);
                        UBLBPreventer.SetActive(true);
                        CancelInvoke(nameof(SpawnAttackers));
                    }
                    else if (AreaCode == 2)
                    {
                        Invoke(nameof(ShowFindHint), 4f);
                        Progress++;
                        warningOrMsg.SetTrigger("Hide");
                        warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED:</color> CLEARED THE SHIP";
                        warningOrMsg.SetTrigger("Display");
                        CancelInvoke(nameof(SpawnAttackers));
                    }
                }
                IncrementKills();
            }
                
        }

        #region Hints
        void ShowUBHint()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> CLIMB TO UPPER BASEMENT";
            warningOrMsg.SetTrigger("Display");
            messageText.text = "REACH UPSTAIRS";
            messageTextGO.SetTrigger("Show");
            taskManager.NextTask();
        }
        void ShowGroundFloorHint()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> CLIMB TO GROUND FLOOR";
            warningOrMsg.SetTrigger("Display");
            messageText.text = "REACH UPSTAIRS";
            messageTextGO.SetTrigger("Show");
            taskManager.NextTask();
        }
        void ShowFindHint()
        {
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> FIND EXPLOSIVES";
            warningOrMsg.SetTrigger("Display");
            taskManager.NextTask();
            messageText.text = "FIND EXPLOSIVES";
            messageTextGO.SetTrigger("Show");
            explosivesTrigger.enabled = true;
            explosivesTrigger.contact = playerTransform;
            pressChecker.onKeyPressed += OnBombStored;
            explosivesTrigger.onObjectTrigger += OnPlayerReachExplosiveSpot;
            explosivesTrigger.onObjectExit += OnPlayerExitExplosiveSpot;
        }

        #endregion
     
        void PlayerReachUpperBasement()
        {
            OptimizeUpperBasement();
            UBLBPreventer.SetActive(true);
            upperBasementTrigger.onObjectTrigger -= PlayerReachUpperBasement;
            UBLBPreventer.SetActive(true);
            messageTextGO.SetTrigger("Hide");
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> CLEAR UPPER BASEMENT";
            warningOrMsg.SetTrigger("Display");
            taskManager.NextTask();
            playerScript.playerSpawn = playerSpawns[2];
         
        }
        void PlayerReachGroundFloor()
        {
            OptimizeGroundFloor();

            GFUBPreventer.SetActive(true);

            groundFloorTrigger.onObjectTrigger -= PlayerReachGroundFloor;
            Destroy(groundFloorTrigger.gameObject);

            messageTextGO.SetTrigger("Hide");
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=yellow>TASK:</color> CLEAR THE SHIP";
            warningOrMsg.SetTrigger("Display");
            taskManager.NextTask();
            playerScript.playerSpawn = playerSpawns[3];
            messageText.text = "CLEAR SHIP";
            messageTextGO.SetTrigger("Show");

        }

        int streak = 0;
        public void OnPlayerDied()
        {
            StopCoroutine(nameof(showNextDialogue));
            playerTransform.SetParent(null);
            streak = 0;
            if (ap.childCount != 0)
            {
                foreach (Transform s in ap)
                {
                    Destroy(s.gameObject);
                }
            }

              
            
        }
        public Transform playerBoatTransform;
        public TriggerEnter explosivesTrigger;
        public GameObject storeExplosivePanel;
        public PressChecker pressChecker;
        void OnPlayerReachExplosiveSpot()
        {
            pressChecker.Check = true;
            storeExplosivePanel.SetActive(true);
        }
        void OnPlayerExitExplosiveSpot()
        {
            pressChecker.Check = false;
            storeExplosivePanel.SetActive(false);
        }
        void OnBombStored()
        {
            taskManager.NextTask();
            settings.interactables.SetActive(false);
            warningOrMsg.SetTrigger("Hide");
            warningOrMsgText.text = "<color=green>TASK ACCOMPLISHED:</color> FOUND EXPLOSIVES!";
            warningOrMsg.SetTrigger("Display");
            blackout.SetTrigger("FadeOut");
            foreach (Transform t in settings.otherPanels.transform)
            {
                if (t.name != "Blackout" && t != warningOrMsg.transform)
                {
                    t.gameObject.SetActive(false);
                }
            }
            Invoke(nameof(EndAct), 4f);
        }
        public void IncrementKills()
        {
            kills += 1;
           
            if(kills == 3)
            {
                InvokeRepeating(nameof(SpawnAttackers), 1, 4);
            }

            if(kills == 19)
            {
                InvokeRepeating(nameof(SpawnAttackers), 0, 4);
            }
            if (kills == 27)
            {
                InvokeRepeating(nameof(SpawnAttackers), 3, 6);
            }
            streak += 1;

            if (streak >= 2)
            {
                ShowAppreciation("<color=yellow>" + streak + "X</color> Streak", streakIcon);
            }
        }
        void ShowTaskSuggestion()
        {
            warningOrMsgText.text = "<color=grey>SUGGESTION:</color> PRESS 'T' TO VIEW TASKS";
            warningOrMsg.SetTrigger("Display");
        }
        public void TreatPlayerAsTraitor()
        {
            if(!playerScript.isDead)
            {
                if (!playerScript.foreverInvincible)
                {
                    playerScript.Die(false);
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
                }
                else
                {
                    Time.timeScale = 1;
                    Time.fixedDeltaTime = 0.02f;
                    warningOrMsgText.text = "<color=red>WARNING </color> DON'T FIRE AT YOUR MATES";
                    warningOrMsg.SetTrigger("Display");
                }
            
            
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

        
            PlayerPrefs.SetInt("ActNumber", 6);
            PlayerPrefs.SetString("SelectedAct", "Act6");
            PlayerPrefs.SetInt("Act5Completed", 1);
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
