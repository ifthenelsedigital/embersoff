using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.ImageEffects;
using System.Collections.Generic;
using GunType = IfThenElse.Weapon.GunType;
using System.Collections;

namespace IfThenElse
{
    public class Player : MonoBehaviour
    {
        #region Setup

        public bool isInvincible = false;
        public bool foreverInvincible = false;
        public int ActNumber;
        public bool AcceptFireControls = true;
        public GameObject controller;
        public TMP_Text areaNameText;
        public TMP_Text PrimaryAmmoText;
        public TMP_Text SecondaryAmmoText;
        public TMP_Text grenadeText;

        public delegate void AmmoOver();
        public AmmoOver onAmmoOver;

        public GameObject AllyWarningPanel;

        public UIOptimizer PrimaryGunIcon;
        public UIOptimizer SecondaryGunIcon;
        public UIOptimizer meleeIcon;
        public ItemsAndIcons iai;
        public string defaultPrimaryGun;
        public string defaultSecondaryGun;
        public string defaultMelee;
        public int defaultGrenades;
        readonly Dictionary<string, RuntimeAnimatorController> gunsAndAoc = new Dictionary<string, RuntimeAnimatorController>();
        private void Awake()
        {
            playerAnim = GetComponent<Animator>();
            gunsAndAoc.Add("SLR", SLRAOC);
            gunsAndAoc.Add("Thompson", thompsonAOC);
            gunsAndAoc.Add("LeeEnfield", LeeEnfieldAC);
            gunsAndAoc.Add("AK74", ak74AOC);
            gunsAndAoc.Add("Winchester", winchesterAOC);
            gunsAndAoc.Add("M1911", m1911AOC);
            gunsAndAoc.Add("M1 Garand", M1GarandOAC);
            gunsAndAoc.Add("M4 Carbine", M4CarbineAC);
            gunsAndAoc.Add("M79", M79AC);
            gunsAndAoc.Add("Browning HP", browningHPAOC); 
            gunsAndAoc.Add("Bren", BrenAOC);

            Debug.Log("default screen resolution is " + Screen.currentResolution);

            interactables.SetActive(true);

            AllyWarningPanel.SetActive(false);

         }

        void Start()
        {
            ml = MainController.m_MouseLook;
            if(ActNumber != 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            if (PlayerPrefs.HasKey("Sensitivity"))
            {
                float Sensitivity = PlayerPrefs.GetFloat("Sensitivity");
                ml.Sensitivity = Sensitivity;
            }
            else
            {
                PlayerPrefs.SetFloat("Sensitivity", 0.3f);
                ml.Sensitivity = 0.3f;
            }


            if (PlayerPrefs.GetString("ADS") == "On")
            {
                useADS = true;
            }
            else
            {
                useADS = false;
                if(ActNumber == 0) //training
                {
                    useADS = true;
                    Debug.Log("Welcome to training!");
                }
            }

          
            playerHealth = 100;
            grenadeCount = defaultGrenades;
            primaryGun = defaultPrimaryGun;
            secondaryGun = defaultSecondaryGun;
            currentWeapon = primaryGun;
            currentMelee = defaultMelee;
            PrimaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
            SecondaryGunIcon.SetSprite("Button_Frame_Mono");
            isUsingMelee = false;
            weaponsAnim.runtimeAnimatorController = gunsAndAoc[currentWeapon];
           
            ml.smooth = false;
            foreach (Weapon gunScript in Arms)
            {
                    if(gunScript.Name == primaryGun)
                    {
                        gunScript.gameObject.SetActive(true);
                        currentWeaponScript = gunScript;
                        currentWeaponScript.currentMagAmmo = currentWeaponScript.maxAmmoPerMag;
                        if(currentWeaponScript.Type == GunType.AR || currentWeaponScript.Type == GunType.SMG)
                        {
                            currentWeaponScript.currentAmmo = ARAmmoCount; //as smg and ar are same as default
                        }
                        else if(currentWeaponScript.Type == GunType.Sniper)
                        {
                            currentWeaponScript.currentAmmo = SniperAmmoCount;
                        }
                        else if(currentWeaponScript.Type == GunType.SG ) 
                        {
                            currentWeaponScript.currentAmmo = SGAmmoCount;
                        }
                        else if (currentWeaponScript.Type == GunType.LMG)
                        {
                            currentWeaponScript.currentAmmo = LMGAmmoCount;
                        }
                    }

                    else if(gunScript.Name == secondaryGun)
                    {
                        gunScript.currentAmmo = HGAmmoCount; //because secondary weapons are always pistols
                        gunScript.gameObject.SetActive(false);
                        gunScript.currentMagAmmo = gunScript.maxAmmoPerMag;
                        SecondaryAmmoText.text = gunScript.currentMagAmmo.ToString() + "/" + gunScript.currentAmmo.ToString();
                    }

                    else
                    {
                        gunScript.gameObject.SetActive(false);
                    }
                
            }
            if(iai.gunIcons[primaryGun] == null)        
            {
                Debug.Log("hii");
            }
            UpdateSpeedAccordingToGun();
           Instantiate(iai.gunIcons[primaryGun], PrimaryGunIcon.transform);
            Instantiate(iai.gunIcons[secondaryGun], SecondaryGunIcon.transform);
            PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + currentWeaponScript.currentAmmo.ToString();
            grenadeText.text = "x" + grenadeCount.ToString();
            meleeIcon.enabled = true;
            meleeIcon.SetSprite(iai.otherIcons[defaultMelee].name);
            meleeIcon.enabled = false;
            isUsingPrimaryWeapon = true;
        }
       
        #endregion

        #region Controls

        public FirstPersonController MainController;
        public GameObject CrosshairCentre;
        public GameObject Crosshair;

        public GameObject sniperScopeOverlay;


        [Header("Animator Controllers")]
        public AnimatorOverrideController M1GarandOAC;
        public AnimatorOverrideController thompsonAOC;
        public AnimatorOverrideController winchesterAOC;
        public AnimatorOverrideController SLRAOC;
        public AnimatorOverrideController m1911AOC;
        public AnimatorOverrideController browningHPAOC;
        public RuntimeAnimatorController LeeEnfieldAC;
        public RuntimeAnimatorController ak74AOC;
        public RuntimeAnimatorController BrenAOC;
        public RuntimeAnimatorController M4CarbineAC;
        public RuntimeAnimatorController M79AC;

        public RuntimeAnimatorController meleeAC;

        [HideInInspector]
        public Weapon currentWeaponScript;

       public bool isCrouching = false;

        Animator playerAnim;
        bool isReloadCalled = false;
  
        public void RefreshControls(bool enterVehicleMode, bool useDefaultWeapons)
        {
            EnableCrosshair();
            CancelInvoke(nameof(ShowSniperScope));
            HideSniperScope();
           
            currentWeaponScript.isPaused = false;
            currentWeaponScript.canFire = true;
            isCrouching = false;
            isJumpCalled = false;
            weaponsAnim.SetBool("isRunning", false);
            weaponsAnim.SetBool("isScoped", false);
            weaponsAnim.SetFloat("RunSpeed", 1f);

            if(useDefaultWeapons)
            {
                currentWeapon = defaultPrimaryGun;
                isUsingPrimaryWeapon = true;
                isUsingMelee = false;
                isHoldingExplosive = false;
                isThrowingGrenade = false;
                foreach(Weapon w in Arms)
                {
                    if(w.Name == currentWeapon)
                    {
                        w.gameObject.SetActive(true);
                    }
                    else
                    {
                        w.gameObject.SetActive(false);
                    }
                }
            }
            
            weaponsAnim.enabled = false;
            currentWeaponScript.transform.parent.localRotation = Quaternion.Euler(Vector3.zero);
            currentWeaponScript.transform.parent.localPosition = new Vector3(0f, 0.01f, 0f);
            weaponsAnim.SetTrigger("Open");
            cam.GetComponent<Animator>().SetBool("isScoped", false);
            cam.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cam.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            weaponsAnim.enabled = true;

            if(isProning)
            {
                ml.Sensitivity = prevSensitivity;
                isProning = false;
            }
            isFirePressed = false;
            
          
            MainController.m_IsWalking = false;

            crossHairAnim.SetBool("ShowEnemyIndication", false);
            crossHairAnim.SetBool("ShowFriendlyIndication", false);

            AcceptFireControls = true;
            AcceptMovement = true;

            if(isHoldingExplosive)
            {
                HoldGrenade();
            }

            Debug.Log("Controls Refreshed!");

            if(enterVehicleMode)
            {
                Debug.Log("add something here");
            }    
        }  

        public void ObtainGun(string gunName, bool isPrimary)
        {
            currentWeapon = gunName;
            isUsingPrimaryWeapon = isPrimary;
            foreach(Weapon w in Arms)
            {
                if(w.Name == currentWeapon)
                {
                    w.gameObject.SetActive(true);
                    weaponsAnim.SetTrigger("Open");
                }
                else
                {
                    w.gameObject.SetActive(false);
                }
            }
        }
        void DisableCrosshair()
        {
            CrosshairCentre.SetActive(false);
            Crosshair.SetActive(false);
        }
        void EnableCrosshair()
        {
            CrosshairCentre.SetActive(true);
            Crosshair.SetActive(true);
        }
        public LayerMask rayMask;
        public Transform nbPanelContainer;
        [HideInInspector]
        public bool doContinousRaycasting = true;
        public bool AcceptMovement = true;
        bool CanScroll = true;
     
        void Update()
        {
           
            #region Key Commands
            if (AcceptMovement)
            {
                MainController.inputs.x = Input.GetAxis("Horizontal");
                MainController.inputs.y = Input.GetAxis("Vertical");
            }

            if (Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                }

            if (Input.GetKeyDown(KeyCode.R))
            {
                isReloadCalled = true;
                Reload();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Crouch();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                OnProneBtnPressed();
            }
            if(CanScroll) 
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)//upwards
                {
                    if (isUsingPrimaryWeapon && !isUsingMelee && !isHoldingExplosive)
                    {
                        if (currentWeaponScript.Type != GunType.HG)
                        {
                            MeleeClicked();
                            CanScroll = false;
                            Invoke(nameof(ResetScroll), 0.5f);
                        }
                    }
                    else if (!isUsingPrimaryWeapon && !isUsingMelee && !isHoldingExplosive)
                    {
                        if (currentWeaponScript.Type == GunType.HG)
                        {
                            PrimaryWeaponClicked();
                            CanScroll = false;
                            Invoke(nameof(ResetScroll), 0.5f);
                        }
                    }
                    else if (!isUsingPrimaryWeapon && isUsingMelee && !isHoldingExplosive)
                    {
                        SecondaryWeaponClicked();
                        CanScroll = false;
                        Invoke(nameof(ResetScroll), 0.5f);
                    }
                    else if (!isUsingPrimaryWeapon && !isUsingMelee && isHoldingExplosive)
                    {
                        if (currentWeaponScript.Type == GunType.HG)
                        {
                            PrimaryWeaponClicked();
                            CanScroll = false;
                            Invoke(nameof(ResetScroll), 0.5f);
                        }
                    }
                }

                if (Input.GetAxis("Mouse ScrollWheel") < 0)//downwards
                {
                    if (isUsingPrimaryWeapon && !isUsingMelee && !isHoldingExplosive)
                    {
                        if (currentWeaponScript.Type != GunType.HG)
                        {
                            SecondaryWeaponClicked();
                            CanScroll = false;
                            Invoke(nameof(ResetScroll), 0.5f);
                        }
                    }
                    else if (!isUsingPrimaryWeapon && !isUsingMelee && !isHoldingExplosive)
                    {
                        if (currentWeaponScript.Type == GunType.HG)
                        {
                            MeleeClicked();
                            CanScroll = false;
                            Invoke(nameof(ResetScroll), 0.5f);
                        }
                    }
                    else if (!isUsingPrimaryWeapon && isUsingMelee && !isHoldingExplosive)
                    {
                        PrimaryWeaponClicked();
                        CanScroll = false;
                        Invoke(nameof(ResetScroll), 0.5f);
                    }
                    else if (!isUsingPrimaryWeapon && !isUsingMelee && isHoldingExplosive)
                    {
                        if (currentWeaponScript.Type == GunType.HG)
                        {
                            SwitchToPrimaryGun();
                            CanScroll = false;
                            Invoke(nameof(ResetScroll), 0.5f);
                        }
                    }
                }

            }
            if (AcceptFireControls)
            {
                ml.LookRotation();
                if (Input.GetMouseButtonDown(0))
                {
                    RightFireReleased();
                    RightFirePressed();
                }
                if (Input.GetMouseButtonUp(0))
                {
                    RightFireReleased();
                }
            }
               
                
                if (Input.GetMouseButtonDown(1)) //right click
                {
                    ScopeBtnPressed();
                }
                if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    PrimaryWeaponClicked();
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SecondaryWeaponClicked();
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    OnGrenadeClicked();
                }
            if (Input.GetMouseButton(2))
            {
                OnGrenadeClicked();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    MeleeClicked();
                }
               
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (currentWeaponBox != null && nbPanelContainer.gameObject.activeSelf)
                    {
                        if (nbPanelContainer.childCount != 0)
                        {
                            nbPanelContainer.GetChild(0).GetComponent<Overview>().ItemClicked();
                        }
                    }
                }
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (currentWeaponBox != null && nbPanelContainer.gameObject.activeSelf)
                {
                    if (nbPanelContainer.childCount != 0)
                    {
                        nbPanelContainer.GetChild(1).GetComponent<Overview>().ItemClicked();
                    }
                }
            }

            #endregion

            if (isFirePressed)
            {
                if (currentWeaponScript.canFire && currentWeaponScript.currentMagAmmo != 0 && !currentWeaponScript.isReloading)
                {
                    if(currentWeaponScript.Type == GunType.GrenadeLauncher)
                    {
                        if (currentWeaponScript.currentMagAmmo != 0 && !currentWeaponScript.isScoping && !isReloadCalled)
                        {
                            currentWeaponScript.Fire();
                            weaponsAnim.SetBool("isShooting", true);
                            weaponsAnim.SetBool("isScoped", false);
                        }      
                    }
                   
                    else
                    {      
                        if (weaponsAnim.GetBool("isScoped") && currentWeaponScript.currentMagAmmo != 0 && !currentWeaponScript.isScoping && !isReloadCalled)
                        {
                            currentWeaponScript.Fire();
                            weaponsAnim.SetBool("isShooting", true);
                        }
                        else if (IsAdsShot && !currentWeaponScript.isScoping && !isReloadCalled)
                        {
                            weaponsAnim.SetBool("isRunning", false);
                            weaponsAnim.SetBool("isScoped", true);
                            MainController.m_IsWalking = true;
                            DisableCrosshair();
                            currentWeaponScript.cameraObject.SetBool("isScoped", true);
                            IsAdsShot = true;
                           
                            currentWeaponScript.isScoping = true;
                            Invoke("Fire", currentWeaponScript.ADSTime);
                        }
                        else if (!IsAdsShot && !currentWeaponScript.isScoping && !isReloadCalled)
                        {
                            weaponsAnim.SetBool("isShooting", true);
                            weaponsAnim.SetBool("isScoped", false);
                            currentWeaponScript.cameraObject.SetBool("isScoped", false);
                            EnableCrosshair();
                            currentWeaponScript.Fire();
                        }

                    }
                }

                else if (currentWeaponScript.currentMagAmmo == 0 && currentWeaponScript.currentAmmo != 0 && !currentWeaponScript.isReloading)
                {
                    Reload();
                }
                else if (currentWeaponScript.currentMagAmmo == 0 && currentWeaponScript.currentAmmo == 0)
                {
                    weaponsAnim.SetBool("isShooting", false);
                    onAmmoOver?.Invoke();
                }

            }
  
            if(doContinousRaycasting && !isUsingMelee && currentWeapon != "M79" && !isHoldingExplosive)
            {
                if(Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out RaycastHit hit, 100f, rayMask, QueryTriggerInteraction.Ignore))
                {
                    if (ActNumber == 1 && hit.collider.CompareTag("Soldier"))
                    {
                        Act1Soldier act1Soldier = null;
                        if(hit.collider.transform.parent.name.Contains("Japanese"))
                        {
                            act1Soldier = hit.collider.transform.parent.GetComponent<Act1Soldier>();
                        }
                        else if(hit.collider.transform.parent.name.Contains("Elements"))  //a britain soldier
                        {
                            act1Soldier = hit.collider.transform.parent.parent.parent.GetComponent<Act1Soldier>();
                        }
                        if (act1Soldier != null)
                        {
                            if (!act1Soldier.isDead)
                            {
                                if (act1Soldier.nationality == Act1Soldier.Nationality.British)
                                {
                                    crossHairAnim.SetBool("ShowEnemyIndication", false);
                                    crossHairAnim.SetBool("ShowFriendlyIndication", true);
                                  
                                    act1Soldier.ShowNameDisplay();

                                    if(!isWarningDisplayed)
                                    {
                                        CancelInvoke(nameof(ShowAllyAimWarning));
                                        isWarningDisplayed = true;
                                        Invoke(nameof(ShowAllyAimWarning), .8f);
                                    }
                                }
                                else
                                {
                                    crossHairAnim.SetBool("ShowFriendlyIndication", false);
                                    crossHairAnim.SetBool("ShowEnemyIndication", true);
                                    if(isWarningDisplayed)
                                    {
                                        AllyWarningPanel.SetActive(false);
                                        CancelInvoke(nameof(ShowAllyAimWarning));
                                        isWarningDisplayed = false;
                                    }
                                }
                            }
                        }

                        else
                        {
                            crossHairAnim.SetBool("ShowFriendlyIndication", false);
                            crossHairAnim.SetBool("ShowEnemyIndication", false);
                            if (isWarningDisplayed)
                            {
                                AllyWarningPanel.SetActive(false);
                                CancelInvoke(nameof(ShowAllyAimWarning));
                                isWarningDisplayed = false;
                            }
                        }
                    }

                    else if(ActNumber != 1 && hit.collider.CompareTag("Soldier"))
                    {
                        Soldier soldier = hit.collider.transform.parent.GetComponent<Soldier>();
                        if (soldier != null)
                        {
                            if (!soldier.isDead)
                            {
                                if (soldier.nationality == Soldier.Nationality.Indian)
                                {
                                    crossHairAnim.SetBool("ShowEnemyIndication", false);
                                    crossHairAnim.SetBool("ShowFriendlyIndication", true);
                                    soldier.ShowNameDisplay();
                                    if (!isWarningDisplayed)
                                    {
                                        isWarningDisplayed = true;
                                        CancelInvoke(nameof(ShowAllyAimWarning));
                                        Invoke(nameof(ShowAllyAimWarning), .8f);
                                    }
                                }
                                else
                                {
                                    crossHairAnim.SetBool("ShowFriendlyIndication", false);
                                    crossHairAnim.SetBool("ShowEnemyIndication", true);
                                    if (isWarningDisplayed)
                                    {
                                        AllyWarningPanel.SetActive(false);
                                        CancelInvoke(nameof(ShowAllyAimWarning));
                                        isWarningDisplayed = false;
                                    }
                                }
                            }
                        }

                        else
                        {
                            crossHairAnim.SetBool("ShowFriendlyIndication", false);
                            crossHairAnim.SetBool("ShowEnemyIndication", false);
                            if (isWarningDisplayed)
                            {
                                AllyWarningPanel.SetActive(false);
                                CancelInvoke(nameof(ShowAllyAimWarning));
                                isWarningDisplayed = false;
                            }
                        }
                    }

                    else if(hit.collider.CompareTag("Helicopter"))
                    {
                        crossHairAnim.SetBool("ShowFriendlyIndication", false);
                        crossHairAnim.SetBool("ShowEnemyIndication", true);
                    }

                    else
                    {
                        crossHairAnim.SetBool("ShowFriendlyIndication", false);
                        crossHairAnim.SetBool("ShowEnemyIndication", false);
                        if (isWarningDisplayed)
                        {
                            AllyWarningPanel.SetActive(false);
                            isWarningDisplayed = false;
                            CancelInvoke(nameof(ShowAllyAimWarning));
                        }
                    }
                }
                else
                {
                    crossHairAnim.SetBool("ShowFriendlyIndication", false);
                    crossHairAnim.SetBool("ShowEnemyIndication", false);
                    if (isWarningDisplayed)
                    {
                        AllyWarningPanel.SetActive(false);
                        isWarningDisplayed = false;
                        CancelInvoke(nameof(ShowAllyAimWarning));
                    }
                }
              
            }
           
            else
            {
                crossHairAnim.SetBool("ShowFriendlyIndication", false);
                crossHairAnim.SetBool("ShowEnemyIndication", false);
                if (isWarningDisplayed)
                {
                    AllyWarningPanel.SetActive(false);
                    isWarningDisplayed = false;
                    CancelInvoke(nameof(ShowAllyAimWarning));
                }
            }

            
        }

        public Animator crossHairAnim;
        public Camera cam;
        bool isWarningDisplayed = false;
        void ShowAllyAimWarning()
        {
            AllyWarningPanel.SetActive(true);
            isWarningDisplayed = true;
        }
        void ResetScroll()
        {
            CanScroll = true;
        }
        void Fire()
        {
            Crosshair.GetComponent<Animator>().SetBool("isCrosshairLarge", true);
            if (!isUsingMelee)
            {
                currentWeaponScript.Fire();
                if(currentWeaponScript.Type == GunType.Sniper)
                {
                    weaponsAnim.SetTrigger("Shoot");
                }
                else if(currentWeaponScript.Type == GunType.RocketLauncher)
                {
                    weaponsAnim.SetTrigger("Shoot");
                    Invoke(nameof(DisableScope), 0.8f);
                }
                else
                {
                    weaponsAnim.SetBool("isShooting", true);
                }
                currentWeaponScript.isScoping = false;
              
            }
            else
            {
                currentWeaponScript.Fire();
            }
           
        }
        void DisableScope()
        {
            weaponsAnim.SetBool("isScoped", false);
            currentWeaponScript.cameraObject.SetBool("isScoped", false);
            Reload();
        }

        #region Jump
        public bool isJumpCalled= false;
        public void Jump()
        {
            if(!MainController.m_Jumping && !isProning && !isCrouching)
            {
             
                MainController.m_Jump = true;
                weaponsAnim.SetBool("isRunning", false);
                playerAnim.SetBool("isCrouching", false);
                playerAnim.SetBool("IsProning", false);
                MainController.m_MouseLook.MinimumX = -70f;
                MainController.m_MouseLook.MaximumX = 70f;
                MainController.m_IsWalking = false;
                isProning = false;
                isCrouching = false;
                Invoke("ReleaseJumpBtn", 0.55f);
            }
            else if(isCrouching || isProning)
            {
                playerAnim.SetBool("isCrouching", false);
                playerAnim.SetBool("IsProning", false);
                MainController.m_MouseLook.MinimumX = -70f;
                MainController.m_MouseLook.MaximumX = 70f;
                MainController.m_IsWalking = false;
              
                if(isProning)
                {
                    MainController.m_MouseLook.Sensitivity = prevSensitivity;
                }
                isProning = false;
                isCrouching = false;
            }
         
        }
        public void ReleaseJumpBtn()
        {
            isJumpCalled = false;
        }
        #endregion

        #region Crouch
        public void Crouch()
        {
            if (isCrouching)
            {

                if (MainController.inputs.y == 0)
                {
                    AcceptMovement = false;
                    MainController.inputs.y = -1f;
                    Invoke(nameof(ReEnableInputs), 0.35f);
                }
                playerAnim.SetBool("isCrouching", false);
                MainController.m_IsWalking = false;
                isCrouching = false;

                if (!weaponsAnim.GetBool("isScoped"))
                {
                    weaponsAnim.SetBool("isRunning", false);
                }

                
            }
            else
            {
                if (MainController.inputs.y == 0)
                {
                    AcceptMovement = false;
                    MainController.inputs.y = 1f;
                    Invoke(nameof(ReEnableInputs), 0.35f);
                }
                playerAnim.SetBool("isCrouching", true);
                playerAnim.SetBool("IsProning", false);
                MainController.m_MouseLook.MinimumX = -70f;
                MainController.m_MouseLook.MaximumX = 70f;
                if (isProning)
                {
                    MainController.m_MouseLook.Sensitivity = prevSensitivity;
                }

                MainController.m_IsWalking = true;
                isCrouching = true;
                isProning = false;
                weaponsAnim.SetBool("isRunning", false);

             
               
               
            }
        }
        void ReEnableInputs()
        {
            if(!isDead)
            {
                AcceptMovement = true;
            }
        }
        #endregion

        #region Prone

        public bool isProning = false;
        float prevSensitivity;
        public void OnProneBtnPressed()
        {
            if (!isProning)
            {
                if (MainController.inputs.y == 0)
                {
                    AcceptMovement = false;
                    MainController.inputs.y = 1f;
                    Invoke(nameof(ReEnableInputs), 1f);
                }
                weaponsAnim.SetBool("IsRunning", false);                              
                playerAnim.SetBool("isCrouching", false);
                playerAnim.SetBool("IsProning", true);
                isProning = true;
                MainController.m_IsWalking = true;
                MainController.m_MouseLook.MinimumX = -25f;
                MainController.m_MouseLook.MaximumX = 20f;
                prevSensitivity = MainController.m_MouseLook.Sensitivity;
                MainController.m_MouseLook.Sensitivity = 0.05f; 
                weaponsAnim.SetFloat("RunSpeed", 0.5f);

              
               
            }
            else
            {
                playerAnim.SetBool("IsCrouching", false);
                playerAnim.SetBool("IsProning", false);
                isProning = false;
                MainController.m_IsWalking = false;
                MainController.m_MouseLook.MinimumX = -70f;
                MainController.m_MouseLook.MaximumX = 70f;
                MainController.m_MouseLook.Sensitivity = prevSensitivity;
                weaponsAnim.SetFloat("RunSpeed", 1f);

                if (MainController.inputs.y == 0)
                {
                    AcceptMovement = false;
                    MainController.inputs.y = -1f;
                    Invoke(nameof(ReEnableInputs), 1f);
                }
            }
        }

        #endregion

    

        #endregion

        #region Inventory

        [Header("Inventory")]
        [Space(5f)]
        string primaryGun;
        string secondaryGun;
       

        [Space(5f)]
        [Header("Ammos")]
        public int ARAmmoCount = 150;
        public int SMGAmmoCount = 150;
        public int LMGAmmoCount = 500;
        public int SniperAmmoCount = 54;
        public int SGAmmoCount = 70;
        public int HGAmmoCount = 140;

        bool isUsingPrimaryWeapon = true;

        bool IsUsedGun;
        int magAmmoOnGun;

        string prevWeapon;
        public void PickUpObject(string name, int value, string type, bool isUsedGun, int magAmmo)
        {
            weaponsAnim.SetTrigger("PickupObject");
            if (type == "Gun")
            {
                weaponsAnim.SetTrigger("Close");
                IsUsedGun = isUsedGun;
                magAmmoOnGun = magAmmo;
                if(isUsingPrimaryWeapon)
                {
                    prevWeapon = primaryGun;
                }
                else
                {
                    prevWeapon = secondaryGun;
                }
                if (iai.useTypes[name] == "P") //it's a primary weapon
                {
                    primaryGun = name;
                    currentWeapon = name;
                    foreach(Transform t in PrimaryGunIcon.transform)
                    {
                        if(t != PrimaryAmmoText.transform)
                        {
                            Destroy(t.gameObject);
                        }
                    }
                    Instantiate(iai.gunIcons[name], PrimaryGunIcon.transform);
                    PrimaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                    SecondaryGunIcon.SetSprite("Button_Frame_Mono");
                }
                else
                {
                    secondaryGun = name;
                    currentWeapon = name;
                    foreach (Transform s in SecondaryGunIcon.transform)
                    {
                        if(s != SecondaryAmmoText.transform)
                        {
                            Destroy(s.gameObject);
                        }
                    }
                    Instantiate(iai.gunIcons[name], SecondaryGunIcon.transform);
                    SecondaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                    PrimaryGunIcon.SetSprite("Button_Frame_Mono");
                }
               
                foreach (Weapon gunScript in Arms)
                {
                    if (gunScript.Name == currentWeapon)
                    {
                       
                                currentWeaponBox.GetComponent<CollectablesManager>().AddPlayerGun(prevWeapon, gunScript.currentMagAmmo);
                            
                        
                        break;
                    }
                }
                Invoke("SwitchGun", 0.6f);
            }

            else if (name == "Grenade")
            {
                grenadeCount += value;
                grenadeText.text = grenadeCount.ToString();
            }

            else if (name == "AR Ammo")
            {
                ARAmmoCount += value;
                UpdateAmmoOnGuns();
            }

            else if (name == "SMG Ammo")
            {
                SMGAmmoCount += value;
                UpdateAmmoOnGuns();
            }

            else if (name == "SG Ammo")
            {
                SGAmmoCount += value;
                UpdateAmmoOnGuns();
            }

            else if (name == "Sniper Ammo")
            {
                SniperAmmoCount += value;
                UpdateAmmoOnGuns();
            }

            else if (name == "HG Ammo")
            {
                HGAmmoCount += value;
                UpdateAmmoOnGuns();
            }

            else if (name == "LMG Ammo")
            {
                LMGAmmoCount += value;
                UpdateAmmoOnGuns();
            }

            
        }
       
        public TMP_Text warningText;
        public Animator warningTextGO;
        void SwitchGun()
        {
            foreach (Weapon gunScript in Arms)
            {
             
                    if (gunScript.Name == currentWeapon)
                    {
                        gunScript.gameObject.SetActive(true);
                        currentWeaponScript = gunScript;
                        currentWeaponScript.isPaused = false;
                        if(!IsUsedGun)
                        {
                            currentWeaponScript.currentMagAmmo = currentWeaponScript.maxAmmoPerMag;
                        }
                        else
                        {
                            currentWeaponScript.currentMagAmmo = magAmmoOnGun;
                        }
                        isUsingMelee = false;
                        isHoldingExplosive = false;
                        MainController.m_RunSpeed = currentWeaponScript.playerSpeedWithGun;
                        MainController.crouchSpeed = currentWeaponScript.playerSpeedWithGun / 3;
                        UpdateAmmoOnGuns();
                        weaponsAnim.SetBool("isRunning", false);
                        weaponsAnim.SetBool("isScoped", false);
                        HideSniperScope();
                        weaponsAnim.runtimeAnimatorController = gunsAndAoc[currentWeapon];
                        weaponsAnim.Play("Open");
                 
                        if(currentWeaponScript.useType == Weapon.UseType.Primary)
                        {
                            Instantiate(iai.gunIcons[currentWeapon], PrimaryGunIcon.transform);
                            PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo + "/" + currentWeaponScript.currentAmmo;
                        PrimaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                        SecondaryGunIcon.SetSprite("Button_Frame_Mono");
                        isUsingPrimaryWeapon = true;
                            
                        }
                        else
                        {
                            Instantiate(iai.gunIcons[currentWeapon], SecondaryGunIcon.transform);
                            SecondaryAmmoText.text = currentWeaponScript.currentMagAmmo + "/" + currentWeaponScript.currentAmmo;
                        SecondaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                        PrimaryGunIcon.SetSprite("Button_Frame_Mono");
                        isUsingPrimaryWeapon = false;
                        }
                        if (isProning)
                        {
                            weaponsAnim.SetFloat("RunSpeed", 0.5f);
                        }
                        else
                        {
                            weaponsAnim.SetFloat("RunSpeed", 1f);
                        }

                    }
                    else
                    {
                        gunScript.gameObject.SetActive(false);
                    }
                
            }
        }
        void UpdateAmmoOnGuns()
        {
            if(currentWeaponScript.Type == GunType.AR)
            {
                currentWeaponScript.currentAmmo = ARAmmoCount;
 
                PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + ARAmmoCount.ToString();
               
            }
            else if (currentWeaponScript.Type == GunType.SMG)
            {
                currentWeaponScript.currentAmmo = SMGAmmoCount;
 
                PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + SMGAmmoCount.ToString();
                
            }
            else if (currentWeaponScript.Type == GunType.SG)
            {
                currentWeaponScript.currentAmmo = SGAmmoCount;
                PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + SGAmmoCount.ToString();
               
            }
            else if (currentWeaponScript.Type == GunType.LMG)
            {
                currentWeaponScript.currentAmmo = LMGAmmoCount;
                PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + LMGAmmoCount.ToString();
             
            }
            else if (currentWeaponScript.Type == GunType.Sniper)
            {
                currentWeaponScript.currentAmmo = SniperAmmoCount;
                PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + SniperAmmoCount.ToString();
              
            }
            else if (currentWeaponScript.Type == GunType.HG)
            {
                currentWeaponScript.currentAmmo = HGAmmoCount;
                SecondaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + HGAmmoCount.ToString();
              
            }
           
        }
        
        public GameObject currentWeaponBox;

        void UpdateSpeedAccordingToGun()
        {
            MainController.m_RunSpeed = currentWeaponScript.playerSpeedWithGun;
            MainController.crouchSpeed = currentWeaponScript.playerSpeedWithGun / 3;
        }

        public void PrimaryWeaponClicked()
        {
            if (!isUsingPrimaryWeapon)
            {
                weaponsAnim.SetTrigger("Close");
                isUsingMelee = false;
                isHoldingExplosive = false;

               
                PrimaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                SecondaryGunIcon.SetSprite("Button_Frame_Mono");
                isUsingPrimaryWeapon = true;

              
                currentWeaponScript.cameraObject.SetBool("isScoped", false);

                

                if (!isCrouching && !isProning)
                {
                    MainController.m_IsWalking = false;
                }

                
                Invoke("SwitchToPrimaryGun", 0.5f);
            }
                
        }
        public void SecondaryWeaponClicked()
        {
            if(isUsingPrimaryWeapon || isHoldingExplosive || isUsingMelee)
            {
                weaponsAnim.SetTrigger("Close");
                isUsingMelee = false;
                isUsingPrimaryWeapon = false;
                isHoldingExplosive = false;
              
                SecondaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                PrimaryGunIcon.SetSprite("Button_Frame_Mono");

                
                if (!isCrouching && !isProning)
                {
                    MainController.m_IsWalking = false;
                }

               
                currentWeaponScript.cameraObject.SetBool("isScoped", false);

                Invoke("SwitchToSecondaryGun", 0.5f);
            }
            
        }
        void SwitchToPrimaryGun()
        {
            currentWeapon = primaryGun;
            weaponsAnim.runtimeAnimatorController = gunsAndAoc[currentWeapon];
            weaponsAnim.SetBool("isRunning", false);
            weaponsAnim.SetBool("isScoped", false);
            weaponsAnim.SetTrigger("Open");
            weaponsAnim.ResetTrigger("Close");
            currentWeaponScript.CancelInvoke("FillAmmo");
            currentWeaponScript.isReloading = false;

            foreach (Weapon gunScript in Arms)
            {
               
                    if (gunScript.Name == currentWeapon)
                    {
                        gunScript.gameObject.SetActive(true);
                        currentWeaponScript = gunScript;
                        UpdateSpeedAccordingToGun();
                      
                    }
                    else
                    {
                        gunScript.gameObject.SetActive(false);
                    }
                
            }
        }
        void SwitchToSecondaryGun()
        {
            currentWeapon = secondaryGun;
            weaponsAnim.runtimeAnimatorController = gunsAndAoc[currentWeapon];
            weaponsAnim.SetBool("isRunning", false);
            weaponsAnim.SetBool("isScoped", false);
            weaponsAnim.SetTrigger("Open");
            weaponsAnim.ResetTrigger("Close");
            currentWeaponScript.CancelInvoke("FillAmmo");
            currentWeaponScript.isReloading = false;
            foreach (Weapon gunScript in Arms)
            {
                
                    if (gunScript.Name == currentWeapon)
                    {
                        gunScript.gameObject.SetActive(true);
                        currentWeaponScript = gunScript;
                        UpdateSpeedAccordingToGun();
                     
                    }
                    else
                    {
                        gunScript.gameObject.SetActive(false);
                    }
                
            }
        }

        public void MeleeClicked()
        {
            if(!isUsingMelee && !isThrowingGrenade)
            {
                weaponsAnim.SetTrigger("Close");
                Invoke("SwitchToMelee", 0.5f);
                currentWeaponScript.CancelInvoke("FillAmmo");
                currentWeaponScript.isReloading = false;


                PrimaryGunIcon.SetSprite("Button_Frame_Mono");
                SecondaryGunIcon.SetSprite("Button_Frame_Mono");

                isUsingPrimaryWeapon = false;
                isHoldingExplosive = false;
            }
        }
        string currentMelee;
        void SwitchToMelee()
        {
            foreach (Weapon l in Arms)
            {
                if(l.Name == currentMelee)
                {
                    l.gameObject.SetActive(true);
                    currentWeapon = currentMelee;
                    isUsingMelee = true;
                    weaponsAnim.runtimeAnimatorController = meleeAC;
                    currentWeaponScript = l;
                    UpdateSpeedAccordingToGun();
                }
                else
                {
                    l.gameObject.SetActive(false);
                }
            }
                            
            
        }

        #endregion

        #region FightControls

        #region ButtonEvents

        public Animator weaponsAnim;

        public RuntimeAnimatorController grenadeAC;

        public Weapon[] Arms;
        public GameObject scopeCam;
        public bool useADS;

        public string currentWeapon;

       public bool isUsingMelee = false;
        public bool isHoldingExplosive = false;

        int grenadeCount;

        bool IsAdsShot;

        [Range(0, 100)]
        public int playerHealth = 100;

        public void SetRunningArmsAnimation()
        {

            if (!weaponsAnim.GetBool("isScoped") && !isReloadCalled && !isCrouching && !isHoldingExplosive)
            {
                weaponsAnim.SetBool("isRunning", true);
            }
        }
        MouseLook ml;
      

        #region RightFireBtn
        public void RightFirePressed()
        {
            if (!isDead)
            {
                if (currentWeaponScript.Type == GunType.Sniper && !isUsingMelee && !isHoldingExplosive)
                {
                    if (currentWeaponScript.canFire && !currentWeaponScript.isReloading)
                    {
                        if (currentWeaponScript.currentMagAmmo != 0)
                        {
                            
                            if (useADS)
                            {
                                if (weaponsAnim.GetBool("isScoped"))
                                {
                                    IsAdsShot = false;
                                    MainController.m_IsWalking = true;
                                    weaponsAnim.SetBool("isRunning", false);
                                    weaponsAnim.SetBool("isScoped", true);
                                }
                                else
                                {
                                    weaponsAnim.SetBool("isScoped", true);
                                    currentWeaponScript.cameraObject.SetBool("isScoped", true);
                                    MainController.m_IsWalking = true;
                                    weaponsAnim.SetBool("isRunning", false);
                                    IsAdsShot = true;
                                   
                                    DisableCrosshair();
                                    if (currentWeapon == "LeeEnfield")
                                        Invoke("ShowSniperScope", 0.25f);
                                }

                            }
                            else
                            {
                                weaponsAnim.SetTrigger("Shoot");
                                
                                IsAdsShot = false;
                                
                                currentWeaponScript.Fire();
                            }
                        }

                        else if (currentWeaponScript.currentAmmo != 0 && currentWeaponScript.currentAmmo != 0)
                        {
                            CancelInvoke("ShowSniperScope");
                            HideSniperScope();
                            Reload();
                        }
                        else
                        {
                            currentWeaponScript.PlayEmptyClipSound();
                            if(warningText.text != "<color=orange>RECOMMENDED: </color> SEARCH FOR AMMO IN ENEMY BAGS")
                            {
                                warningText.text = "<color=orange>RECOMMENDED: </color> SEARCH FOR AMMO IN ENEMY BAGS";
                                warningTextGO.SetTrigger("Display");
                            }
                        }
                    }

                }

                else if(currentWeaponScript.Type == GunType.RocketLauncher && !isUsingMelee && !isHoldingExplosive)
                {
                    if(currentWeaponScript.canFire && currentWeaponScript.currentMagAmmo != 0 && !currentWeaponScript.isReloading)
                    {
                        weaponsAnim.SetBool("isScoped", true);
                        currentWeaponScript.cameraObject.SetBool("isScoped", true);
                        MainController.m_IsWalking = true;
                        weaponsAnim.SetBool("isRunning", false);
                        Invoke(nameof(Fire), 0.2f);
                    }
                }

                else if (!isUsingMelee && !isHoldingExplosive)
                {
                    if (currentWeaponScript.currentMagAmmo != 0 && currentWeaponScript.canFire && !currentWeaponScript.isReloading)
                    {
                        if (useADS && !weaponsAnim.GetBool("isScoped"))
                        {
                            IsAdsShot = true;
                        }
                        else
                        {
                            IsAdsShot = false;
                        }
                        weaponsAnim.SetBool("isRunning", false);
                        isFirePressed = true;
                    }
                    else if (currentWeaponScript.currentMagAmmo == 0 && currentWeaponScript.currentAmmo != 0)
                    {
                        Reload();
                    }
                    else if (currentWeaponScript.currentMagAmmo == 0 && currentWeaponScript.currentAmmo == 0)
                    {
                        weaponsAnim.SetBool("isShooting", false);
                        currentWeaponScript.PlayEmptyClipSound();
                        onAmmoOver?.Invoke();
                        if (warningText.text != "<color=yellow>RECOMMENDED: </color> SEARCH FOR AMMO IN ENEMY BAGS")
                        {
                            warningText.text = "<color=yellow>RECOMMENDED: </color> SEARCH FOR AMMO IN ENEMY BAGS";
                            warningTextGO.SetTrigger("Display");
                        }
                        Debug.Log("Ammo khallaas");
                    }
                }

                else if (isUsingMelee && !isHoldingExplosive)
                {
                    if (currentWeaponScript.canFire)
                    {
                        currentWeaponScript.canFire = false;
                        weaponsAnim.SetTrigger("Attack");
                        currentWeaponScript.Fire();
                    }
                }


                else if (isHoldingExplosive && !isThrowingGrenade && !isUsingMelee)
                {
                    weaponsAnim.SetTrigger("Throw");
                    isThrowingGrenade = true;
                    Invoke("ThrowGrenade", 1f);
                }
            }
        }
        public void RightFireReleased()
        {
            if (!isDead)
            {                               
                if (currentWeaponScript.Type == GunType.Sniper)
                {
                    if (IsAdsShot)
                    {
                        
                        if (weaponsAnim.GetBool("isScoped") && currentWeaponScript.canFire && currentWeaponScript.currentMagAmmo != 0 && !currentWeaponScript.isReloading)
                        {
                            weaponsAnim.SetTrigger("Shoot");
                            currentWeaponScript.Fire();
                            IsAdsShot = false;
                            
                            Invoke("HideSniperScope", 0.7f);
                            if(!isCrouching && !isProning)
                            {
                                MainController.m_IsWalking = false;
                            }
                        }
                    }
                    else
                    {

                        if (weaponsAnim.GetBool("isScoped") && currentWeaponScript.canFire && currentWeaponScript.currentMagAmmo != 0 && !currentWeaponScript.isReloading)
                        {
                            weaponsAnim.SetTrigger("Shoot");
                            weaponsAnim.SetBool("isScoped", true);
                            DisableCrosshair();
                            currentWeaponScript.Fire();
                            IsAdsShot = false;
                            MainController.m_IsWalking = true;
                            


                            if (weaponsAnim.GetBool("isScoped"))
                            {
                                
                                MainController.m_IsWalking = true;
                                weaponsAnim.SetBool("isScoped", true);
                                DisableCrosshair();
                                if (currentWeapon == "LeeEnfield")
                                    Invoke("ShowSniperScope", 0.1f);
                            }
                        }
                    }
                }

                else if (!isUsingMelee && !isHoldingExplosive && !isThrowingGrenade)
                {
                    isFirePressed = false;
                    if (IsAdsShot)
                    {
                        weaponsAnim.SetBool("isShooting", false);
                        weaponsAnim.SetBool("isScoped", false);
                        EnableCrosshair();
                        currentWeaponScript.cameraObject.SetBool("isScoped", false);
                        CancelInvoke("Fire");
                        currentWeaponScript.isScoping = false;
                      

                        if (!isCrouching && !isProning)
                            MainController.m_IsWalking = false;
                    }
                    else if (!currentWeaponScript.isReloading)
                    {
                        weaponsAnim.SetBool("isShooting", false);
                    }
                }
            }
        }

        public bool isFirePressed = false;

        #endregion

        #region LeeEnfieldScope
        void ShowSniperScope()
        {
            if (!isDead)
            {
                scopeCam.SetActive(true);
                sniperScopeOverlay.SetActive(true);
                CrosshairCentre.SetActive(true);
                Crosshair.SetActive(false);
                CrosshairCentre.GetComponent<Image>().color = Color.red;
                MainController.m_MouseLook.smooth = true;
                MainController.m_IsWalking = true;
            }
        }
        void HideSniperScope()
        {
            scopeCam.SetActive(false);
            sniperScopeOverlay.SetActive(false);
            CrosshairCentre.SetActive(true);
            Crosshair.SetActive(true);
            CrosshairCentre.GetComponent<Image>().color = Color.white;
            MainController.m_MouseLook.smooth = false;
            weaponsAnim.SetBool("isScoped", false);
            currentWeaponScript.cameraObject.SetBool("isScoped", false);
        }

        #endregion

      
        public void ScopeBtnPressed()
        {
            if (!isDead && currentWeaponScript.Type != GunType.GrenadeLauncher)
            {
                if (!currentWeaponScript.isReloading && !currentWeaponScript.isScoping)
                {
                    if (currentWeaponScript.Type == GunType.Sniper)
                    {
                        if (weaponsAnim.GetBool("isScoped") || IsAdsShot)
                        {
                            weaponsAnim.SetBool("isScoped", false);
                            weaponsAnim.SetTrigger("ScopeOut");
                            EnableCrosshair();
                            Crosshair.GetComponent<Animator>().SetBool("isCrosshairLarge", false);
                            currentWeaponScript.cameraObject.SetBool("isScoped", false);
                            CancelInvoke("ShowSniperScope");
                            HideSniperScope();
                            IsAdsShot = false;
                          
                            if (!isCrouching && !isProning)
                                MainController.m_IsWalking = false;
                        }
                        else
                        {
                            weaponsAnim.SetBool("isScoped", true);
                            currentWeaponScript.cameraObject.SetBool("isScoped", true);
                            DisableCrosshair();
                            MainController.m_IsWalking = true;
                            if (currentWeapon == "LeeEnfield")
                            {
                                currentWeaponScript.cameraObject.SetBool("isScoped", true);
                                Invoke("ShowSniperScope", 0.2f);
                            }
                            IsAdsShot = false;
                          
                            weaponsAnim.SetBool("isRunning", false);
                        }

                    }

                    else if(!isUsingMelee || !isHoldingExplosive)
                    {
                        isFirePressed = false;
                        if (weaponsAnim.GetBool("isScoped") || IsAdsShot)
                        {
                            weaponsAnim.SetBool("isScoped", false);
                            
                            EnableCrosshair();
                            currentWeaponScript.cameraObject.SetBool("isScoped", false);
                            IsAdsShot = false;
                           

                            if (!isCrouching && !isProning)
                                MainController.m_IsWalking = false;
                        }
                        else
                        {
                            weaponsAnim.SetBool("isScoped", true);
                            DisableCrosshair();
                          
                            currentWeaponScript.cameraObject.SetBool("isScoped", true);
                            IsAdsShot = false;
                           
                            MainController.m_IsWalking = true;

                            weaponsAnim.SetBool("isRunning", false);
                        }
                    }
                }
            }
            else if(currentWeaponScript.Type == GunType.GrenadeLauncher && !isDead)
            {
                warningText.text = "You cannot scope in a launcher.";
                warningTextGO.SetTrigger("Display");
            }
        }
        public void Reload()
        {
            if (!isDead)
            {
                if (currentWeaponScript.currentMagAmmo != currentWeaponScript.maxAmmoPerMag && currentWeaponScript.currentAmmo > 0 && !currentWeaponScript.isReloading)
                {
                    
                    isFirePressed = false;

                    EnableCrosshair();


                    if (!isCrouching && !isProning)
                    {
                        MainController.m_IsWalking = false;
                    }
                    if (currentWeapon == "LeeEnfield")
                    {
                        currentWeaponScript.cameraObject.SetBool("isScoped", false);
                        CancelInvoke("ShowSniperScope");
                        HideSniperScope();
                        IsAdsShot = false;
                        weaponsAnim.SetBool("isScoped", false);
                       
                        isReloadCalled = false;
                        currentWeaponScript.Reload();
                    }
                    else
                    {
                        IsAdsShot = false;
                        weaponsAnim.SetBool("isScoped", false);
                        currentWeaponScript.cameraObject.SetBool("isScoped", false);
                      
                        isReloadCalled = false;
                        currentWeaponScript.Reload();
                    }

                }
                else
                {
                    isReloadCalled = false;
                }
            }

        }

        #endregion

        #region Grenade
        //Grenade System

        public void OnGrenadeClicked()
        {
            if(grenadeCount != 0 && !isHoldingExplosive)
            {
                weaponsAnim.SetTrigger("Close");
                currentWeaponScript.CancelInvoke("FillAmmo");
                currentWeaponScript.isReloading = false;

                if (currentWeapon == "LeeEnfield")
                {
                    CancelInvoke("ShowSniperScope");
                    HideSniperScope();
                }
                isHoldingExplosive = true;
                Invoke(nameof(HoldGrenade), 0.5f);
                isUsingMelee = false;
                currentWeaponScript.isReloading = false;
                currentWeaponScript.CancelInvoke("FillAmmo");
                currentWeaponScript.isReloading = false;

                weaponsAnim.SetBool("isReloading", false);
                
                isFirePressed = false;
                
              
                PrimaryGunIcon.SetSprite("Button_Frame_Mono");
                SecondaryGunIcon.SetSprite("Button_Frame_Mono");

                isUsingPrimaryWeapon = false;
            }
        }
        bool isThrowingGrenade = false;
        void HoldGrenade()
        {
            foreach(Weapon t in Arms)
            {
                    if (t.Name == "Grenade")
                    {
                        t.gameObject.SetActive(true);
                        currentWeaponScript = t;
                        weaponsAnim.SetBool("isScoped", false);
                       
                        UpdateSpeedAccordingToGun();
                        weaponsAnim.runtimeAnimatorController = grenadeAC;
                    }
                    else
                    {
                        t.gameObject.SetActive(false);
                    }
                    currentWeapon = "Grenade";      

            }
        }
        public GameObject grenadeGO;
        public Transform grenadeSpawnPoint;

        void ThrowGrenade()
        {
            Bomb grenade = Instantiate(grenadeGO, grenadeSpawnPoint.position, Quaternion.identity).GetComponent<Bomb>();
            grenade.isFromPlayer = true;
            grenade.ActNumber = ActNumber;
            grenade.gameObject.SetActive(true);
            grenade.playerCam = currentWeaponScript.cameraObject;
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            grenadeCount -= 1;
            grenadeText.text = "x" + grenadeCount.ToString();
            if (cam.transform.localRotation.x < 0f)
            {
                rb.AddForce(transform.GetChild(0).forward * 20f, ForceMode.Impulse);
            }
            else if (cam.transform.localRotation.x == 0f)
            {
                rb.AddForce(transform.GetChild(0).forward * 15f, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.GetChild(0).forward * 10f, ForceMode.Impulse);
            }
            grenade.GetComponent<Bomb>().isLaunched = true;
            Invoke("SwitchBackToGun", 1f);
        }

        public void SwitchBackToGun()
        {
            currentWeaponScript.enabled = true;
            
            UpdateSpeedAccordingToGun();
            foreach (Weapon t in Arms)
            {
                if (t.Name == primaryGun)
                {
                    t.gameObject.SetActive(true);
                    currentWeaponScript = t;
                    currentWeapon = primaryGun;
                    currentWeaponScript.isPaused = false;
                    weaponsAnim.runtimeAnimatorController = gunsAndAoc[currentWeapon];
                    isHoldingExplosive = false;
                    UpdateSpeedAccordingToGun();
                    isUsingPrimaryWeapon = true;
                    PrimaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
                    SecondaryGunIcon.SetSprite("Button_Frame_Mono");
                    weaponsAnim.SetBool("isRunning", false);
                }
                else
                {
                    t.gameObject.SetActive(false);
                    isThrowingGrenade = false;
                }
            }
        }

        #endregion
        public GameObject interactables;


        #region KillStats
        [Header("Kill Stats")]
        public GameObject killPopUp;
        public Transform killStatsLG;
        public AudioSource killSound;

        public Sprite oneShotIcon;
        public Sprite headShotIcon;
        public Sprite normalKillIcon;
        public Sprite explodeKillIcon;

        public Animator killVignette;
        public void ShowKillStats(List<int> killType)
        {
            if(killStatsLG.childCount >= 3)
            {
                foreach(Transform t in killStatsLG)
                {
                    Destroy(t.gameObject);
                }
            }

                killSound.Play();
            

            foreach (int kt in killType)
            {
                if (kt == 0) // a normal kill
                {
                    GameObject killPop = Instantiate(killPopUp, killStatsLG);
                    killPop.transform.GetChild(0).GetComponent<Image>().sprite = normalKillIcon;
                }
                else if (kt == 1) // a one shot
                {
                    GameObject killPop = Instantiate(killPopUp, killStatsLG);
                    killPop.transform.GetChild(0).GetComponent<Image>().sprite = oneShotIcon;
                }
                else if (kt == 2) // a headshot
                {
                    GameObject killPop = Instantiate(killPopUp, killStatsLG);
                    killPop.transform.GetChild(0).GetComponent<Image>().sprite = headShotIcon;
                }
                else if(kt == 3) // an explosion kill
                {
                    GameObject killPop = Instantiate(killPopUp, killStatsLG);
                    killPop.transform.GetChild(0).GetComponent<Image>().sprite = explodeKillIcon;
                }
            }

            killVignette.SetTrigger("Show");
          
        }

        #endregion

    

        #endregion

        #region AreaManagement

        public void SetCurrentArea(string areaName)
        {
            areaNameText.text = areaName;
        }
        public void ShowBattleFieldLimitWarning(string dialogue, bool isWarning)
        {
            if(isWarning)
            {
                warningText.text = "<color=red>WARNING: </color>" + dialogue; 
                warningTextGO.SetTrigger("Display");
            }
            else
            {
                warningText.text = "<color=orange>RECOMMEND: </color>" + dialogue;
                warningTextGO.SetTrigger("Display");
            }
        }

        #endregion

        #region Health
        [Space(5f)]
        [Header("Health")]
        [HideInInspector]
        public bool isDead = false;
        public Animator bloodAnim;
        public List<AudioClip> hurtSounds = new List<AudioClip>();
        public List<string> deathQuotes = new List<string>();
        string deathSuggestion;
        public GameObject deathPanel;
        public TMP_Text deathReason;
        public AudioSource hurtSource;
        public ArrowIndicator damageIndicator;
        public AudioSource flyBySource;

        
        public void TakeDamage(int damage, Transform from)
        {
            if(!isDead && !isInvincible && !foreverInvincible)
            {
                hurtSource.clip = hurtSounds[Random.Range(0, hurtSounds.Count)];
                damageIndicator.target = from;
                damageIndicator.enabled = true;
                flyBySource.Play();
                if (playerHealth - damage >= 0)
                {
                    playerHealth -= damage;
                    bloodAnim.SetInteger("Health", playerHealth);
                    CancelInvoke("RecoverHP");
                    InvokeRepeating("RecoverHP", 1f, 4f);
                }
                else
                {
                    playerHealth = 0;
                    bloodAnim.SetInteger("Health", playerHealth);
                    isDead = true;
                    Die(true);                   
                }
            }
        }
        float prevVolume;
        public void Die(bool generateQuote)
        {
            isDead = true;
            CancelInvoke(nameof(ShowSniperScope));
            CancelInvoke(nameof(Reload));
            CancelInvoke(nameof(Fire));
          

            if(!isProning)
            {
                transform.GetChild(0).GetComponent<Animator>().SetTrigger("Die");
            }
            currentWeaponScript.cameraObject.SetBool("isScoped", false);
            MainController.m_IsWalking = false;
            weaponsAnim.SetFloat("RunSpeed", 1f);
            playerAnim.SetBool("isCrouching", false);
            playerAnim.SetBool("IsProning", false);
            AcceptFireControls = false;
            
            isReloadCalled = false;
            isProning = false;
            MainController.m_MouseLook.MaximumX = 70f;
            MainController.m_MouseLook.MinimumX = -70f;
            isCrouching = false;
            interactables.SetActive(false);
            onPlayerDied();
            currentWeaponScript.damageX.ResetTrigger("ShowDeathIndication");
            currentWeaponScript.damageX.ResetTrigger("ShowDamageIndication");

            if(AudioListener.volume != 0)
            {
                prevVolume = AudioListener.volume;
                AudioListener.volume = 0.1f;
            }
            GetComponent<CharacterController>().enabled = false;
            MainController.inputs = Vector2.zero;
            MainController.m_MouseLook.smooth = false;
            MainController.enabled = false;
            
            warningTextGO.ResetTrigger("Display");
            warningTextGO.SetTrigger("Hide");
            if(generateQuote)
            {
                deathPanel.SetActive(true);
                deathSuggestion = deathQuotes[Random.Range(0, deathQuotes.Count)];
                deathReason.text = deathSuggestion;
            }

            CrosshairCentre.GetComponent<Image>().color = Color.white;
            Crosshair.GetComponent<Animator>().SetTrigger("HideIndication");
            isFirePressed = false;
            
            weaponsAnim.SetBool("isScoped", false);
            weaponsAnim.SetBool("isShooting", false);
            weaponsAnim.SetBool("isRunning", false);
            weaponsAnim.SetBool("isReloading", false);
            weaponsAnim.ResetTrigger("Reload");
            weaponsAnim.SetTrigger("Close");

            nbPanelContainer.parent.parent.GetComponent<Animator>().SetTrigger("Close");
        
            HideSniperScope();
            Crosshair.GetComponent<Animator>().SetBool("isCrosshairLarge", false);
            isJumpCalled = false;
            CancelInvoke(nameof(ShowSniperScope));
            cam.transform.GetChild(0).gameObject.SetActive(false);
            bloodAnim.SetInteger("Health", playerHealth);
            Invoke(nameof(RespawnPlayer), InstantRespawn ? 0 : 5);

        }
        public bool InstantRespawn = false;
        public void RecoverHP()
        {
            if(playerHealth != 100)
            {
                playerHealth += 20;
                playerHealth = Mathf.Clamp(playerHealth, 0, 100);
                bloodAnim.SetInteger("Health", playerHealth);
            }
            else
            {
                CancelInvoke("RecoverHP");
                bloodAnim.SetInteger("Health", playerHealth);

            }
        }

        public delegate void OnPlayerDied();
        public OnPlayerDied onPlayerDied;
        public Transform playerSpawn;
     
        [Tooltip("Wo beech ka X jo damage ke vakt aata hai")]
        public Animator damageX;
 
        void RespawnPlayer()
        {
            playerHealth = 100;
            bloodAnim.SetInteger("Health", 100);
            currentWeaponScript.isPaused = false;
            transform.position = playerSpawn.position;
            MainController.m_PreviouslyGrounded = true;
            isDead = false;
            cam.GetComponent<Animator>().enabled = false;
            cam.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cam.transform.GetChild(0).gameObject.SetActive(true);
            cam.fieldOfView = 60;
            transform.GetChild(0).GetComponent<Animator>().enabled = true;
        
            currentWeaponScript.cameraObject.SetBool("isScoped", false);
            interactables.SetActive(true);

            weaponsAnim.enabled = false;
            weaponsAnim.transform.localRotation = Quaternion.Euler(0f, 0.01f, 0f);
            weaponsAnim.transform.localPosition = Vector3.zero;
            weaponsAnim.ResetTrigger("Close");
            weaponsAnim.ResetTrigger("Shoot");
            weaponsAnim.Play("Open");
            weaponsAnim.enabled = true;

            if(prevSensitivity != 0)
            {
                MainController.m_MouseLook.Sensitivity = prevSensitivity;
            }

            deathPanel.SetActive(false);

            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;

            AcceptMovement = true;
            AcceptFireControls = true;
  
            damageX.ResetTrigger("ShowDamageIndication");
            damageX.Play("Idle", 2);
            isHoldingExplosive = false;
            GetComponent<CharacterController>().enabled = true;
        

            AudioListener.volume = prevVolume;

            foreach(Transform t in nbPanelContainer)
            {
                Destroy(t.gameObject);
            }

            isUsingPrimaryWeapon = true;
            grenadeCount = defaultGrenades;
            primaryGun = defaultPrimaryGun;
            secondaryGun = defaultSecondaryGun;
            currentWeapon = primaryGun;
            currentMelee = defaultMelee;
            isUsingMelee = false;
            weaponsAnim.runtimeAnimatorController = gunsAndAoc[currentWeapon];
            weaponsAnim.enabled = true;

            MainController.enabled = true;

            crossHairAnim.SetBool("ShowEnemyIndication", false);
            crossHairAnim.SetBool("ShowFriendlyIndication", false);

            MainController.m_MouseLook.smooth = false;
            ARAmmoCount = 250;
            HGAmmoCount = 140;
            LMGAmmoCount = 500;
            SMGAmmoCount = 250;
            SGAmmoCount = 100;
            SniperAmmoCount = 100;
      
            foreach (Weapon gunScript in Arms)
            {
              
                    if (gunScript.Name == currentWeapon)
                    {
                        gunScript.gameObject.SetActive(true);
                        currentWeaponScript = gunScript;
                        currentWeaponScript.currentMagAmmo = currentWeaponScript.maxAmmoPerMag;
                        if (currentWeaponScript.Type == GunType.AR || currentWeaponScript.Type == GunType.SMG)
                        {
                            currentWeaponScript.currentAmmo = ARAmmoCount; //as smg and ar are same as default
                        }
                        else if (currentWeaponScript.Type == GunType.Sniper)
                        {
                            currentWeaponScript.currentAmmo = SniperAmmoCount;
                        }
                        else if (currentWeaponScript.Type == GunType.SG)
                        {
                            currentWeaponScript.currentAmmo = SGAmmoCount;
                        }
                    }
                    else if(gunScript.name == secondaryGun)
                    {
                        gunScript.currentAmmo = HGAmmoCount;
                        gunScript.currentMagAmmo = gunScript.maxAmmoPerMag;
                        SecondaryAmmoText.text = gunScript.maxAmmoPerMag + "/" + HGAmmoCount;
                        gunScript.gameObject.SetActive(false);
                    }
                    else
                    {
                        gunScript.gameObject.SetActive(false);
                    }
                

            }
            foreach (Transform t in PrimaryGunIcon.transform)
            {
                if (t != PrimaryAmmoText.transform)
                {
                    Destroy(t.gameObject);
                }
            }
            foreach (Transform t in SecondaryGunIcon.transform)
            {
                if (t != SecondaryAmmoText.transform)
                {
                    Destroy(t.gameObject);
                }
            }
            Instantiate(iai.gunIcons[currentWeapon], PrimaryGunIcon.transform);
            Instantiate(iai.gunIcons[secondaryGun], SecondaryGunIcon.transform);
            PrimaryAmmoText.text = currentWeaponScript.currentMagAmmo.ToString() + "/" + currentWeaponScript.currentAmmo.ToString();
            currentWeaponScript.cameraObject.SetTrigger("SetEverythingNormal");
            grenadeText.text = "x" + grenadeCount.ToString();
            PrimaryGunIcon.SetSprite("Button_Frame_Hover_Mono");
            SecondaryGunIcon.SetSprite("Button_Frame_Mono");

          

            currentWeaponScript.damageX.ResetTrigger("ShowDeathIndication");
            currentWeaponScript.damageX.ResetTrigger("ShowDamageIndication");
            meleeIcon.SetSprite(iai.otherIcons[defaultMelee].name);
            MainController.m_RunSpeed = currentWeaponScript.playerSpeedWithGun;
            MainController.crouchSpeed = currentWeaponScript.playerSpeedWithGun/3;

            AcceptMovement = false;
            MainController.inputs.y = 1f;
            Invoke(nameof(ReEnableInputs), 0.5f);

        }
        #endregion

    }
}
