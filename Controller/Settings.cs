using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Water;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Playables;

namespace IfThenElse
{
    public class Settings : MonoBehaviour
    {
        #region Variables
        [Header("Settings")]
        public bool isMenu = false;
        public Animator nearbyPanel;
        public GameObject miniMap;
        public Slider sensitivitySlider;
        public TMP_Dropdown fpsDD;
        public TMP_Dropdown gqDD;
        public Toggle minimapToggle;
        public GameObject interactables;
        public GameObject otherPanels;
        public Toggle grassToggle;
        public GameObject deathPanel;
        public TMP_Text deathReason;

        public GameObject SettingsUpdated;

        [Header("Navigation")]
        public Image GameplayImage;
        public Image GraphicsImage;
        public Image ImageEffectsImage;
        public Image AudioImage;

        public GameObject GamePlayPanel;
        public GameObject GraphicsPanel;
        public GameObject ImageEffectsPanel;
        public GameObject AudioPanel;

        public GameObject batteryDisplayRight;
        public GameObject batteryDisplayLeft;

        public AudioSource[] musicSources;
        Dictionary<AudioSource, float> Volumes = new();

        public PostProcessVolume volume;
        public PostProcessLayer layer;
        [HideInInspector]
        public int noOfEffects;
        public Terrain[] terrains;
        public Toggle adsToggle;
        public Toggle subtitlesToggle;
        public Toggle waterReflectionToggle;
        public Toggle ShadowsToggle;
        [Header("Image Effects")]
        public Toggle vignetteToggle;
        public Toggle bloomToggle;
        public Toggle motionBlurToggle;
        public Toggle grainToggle;
        public Toggle dofToggle;
        public Toggle aoToggle;

        public Slider musicVolumeSlider;
        public Slider masterVolumeSlider;
        public Slider camFarPlaneSlider;
        public GameObject farPlaneGO;
        public TMP_Text farPlaneText;

        public GameObject minimapCam;
        public Camera mainCam;
        public Player playerScript;

        public Water waterScript;

        public PlayableDirector introDirector;

        public Animator settingsPanel;
        bool isSettingsOpen = false;

        public Animator taskPanel;
        bool isTaskPanelOpen = false;

        #endregion

        #region Settings Navigation

        public void OptionClicked(int OptionID)  //0 Gameplay, 1 Graphics, 2 Image Effects, 3 Audio
        {
            if(OptionID == 0)
            {
                GameplayImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 255);
                GraphicsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                ImageEffectsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                AudioImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);

                GamePlayPanel.SetActive(true);
                GraphicsPanel.SetActive(false);
                AudioPanel.SetActive(false);
                ImageEffectsPanel.SetActive(false);
            }
            if (OptionID == 1)
            {
                GraphicsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 255);
                GameplayImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                ImageEffectsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                AudioImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);

                GraphicsPanel.SetActive(true);
                GamePlayPanel.SetActive(false);
                AudioPanel.SetActive(false);
                ImageEffectsPanel.SetActive(false);
            }
            if (OptionID == 2)
            {
                ImageEffectsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 255);
                GameplayImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                GraphicsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                AudioImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);

                ImageEffectsPanel.SetActive(true);
                GamePlayPanel.SetActive(false);
                AudioPanel.SetActive(false);
                GraphicsPanel.SetActive(false);
            }
            if (OptionID == 3)
            {
                AudioImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 255);
                GameplayImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                GraphicsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
                ImageEffectsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);

                AudioPanel.SetActive(true);
                GamePlayPanel.SetActive(false);
                GraphicsPanel.SetActive(false);
                ImageEffectsPanel.SetActive(false);
            }
        }

        #endregion

        public bool FirstTime = false;
        public PostProcessVolume introCamProfile;
        private void Start()
        {
            Debug.Log("Setting up settings");
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphics"));
            if(!isMenu)
            {
                GraphicsInfo.text = "GRAPHICS QUALITY: " + QualitySettings.names[QualitySettings.GetQualityLevel()];
            }
            //first set graphics, then set shadows 
            if (PlayerPrefs.GetString("Shadows") == "On")
            {
                ShadowsToggle.isOn = true;
                if (!isMenu)
                {
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                }

            }
            else
            {
                ShadowsToggle.isOn = false;
                if (!isMenu)
                {
                    QualitySettings.shadows = ShadowQuality.Disable;
                }

            }
            gqDD.value = QualitySettings.GetQualityLevel();
            float musicvolume = PlayerPrefs.GetFloat("MusicVolume");
            foreach (AudioSource audioSource in musicSources)
            {
                Volumes.Add(audioSource, audioSource.volume);
                audioSource.volume = ((musicvolume * 100) / 100) * Volumes[audioSource];
            }
            musicVolumeSlider.value = musicvolume;
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
            masterVolumeSlider.value = AudioListener.volume;
            camFarPlaneSlider.value = PlayerPrefs.GetFloat("CameraFarPlane");
            if(!isMenu && !isAct5OrAct3)
            {
                mainCam.farClipPlane = camFarPlaneSlider.value;
            }
            else if(isAct5OrAct3)
            {
                if(camFarPlaneSlider.value >= minPlane)
                {
                    mainCam.farClipPlane = camFarPlaneSlider.value;
                }
                else
                {
                    mainCam.farClipPlane = minPlane;
                }
            }
            farPlaneGO.SetActive(false);

            volume.enabled = false;
            layer.enabled = false;
            noOfEffects = 0;
            if (PlayerPrefs.GetString("ShowMM") == "On")
            {
                minimapToggle.isOn = true;
                if(!isMenu)
                {
                    miniMap.SetActive(true);
                    minimapCam.SetActive(true);
                    batteryDisplayLeft.SetActive(true);
                    batteryDisplayRight.SetActive(false);
                }
               
            }
            else
            {
                minimapToggle.isOn = false;
                if(!isMenu)
                {
                    miniMap.SetActive(false);
                    minimapCam.SetActive(false);
                    batteryDisplayLeft.SetActive(false);
                    batteryDisplayRight.SetActive(true);
                }
                
            }
            if (PlayerPrefs.GetString("WaterReflection") == "On")
            {
                waterReflectionToggle.isOn = true;
                if (!isMenu)
                {
                    if(waterScript != null)
                    {
                        waterScript.waterMode = Water.WaterMode.Reflective;
                    }
                }

            }
            else
            {
                waterReflectionToggle.isOn = false;
                if (!isMenu)
                {
                    if (waterScript != null)
                    {
                        waterScript.waterMode = Water.WaterMode.Simple;
                    }
                }

            }

            if (PlayerPrefs.GetString("Subtitles") == "On")
            {
                subtitlesToggle.isOn = true;
                if (!isMenu)
                {
                    if(playerScript.ActNumber == 1)
                    {
                        playerScript.controller.GetComponent<Act1Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 2)
                    {
                        playerScript.controller.GetComponent<Act2Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 3)
                    {
                        playerScript.controller.GetComponent<Act3Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 4)
                    {
                        playerScript.controller.GetComponent<Act4Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 5)
                    {
                        playerScript.controller.GetComponent<Act5Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 6)
                    {
                        playerScript.controller.GetComponent<Act6Controller>().ShowSubtitles = true;
                    }
                }
            }
            else
            {
                subtitlesToggle.isOn = false;
                if (!isMenu)
                {
                    if (playerScript.ActNumber == 1)
                    {
                        playerScript.controller.GetComponent<Act1Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 2)
                    {
                        playerScript.controller.GetComponent<Act2Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 3)
                    {
                        playerScript.controller.GetComponent<Act3Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 4)
                    {
                        playerScript.controller.GetComponent<Act4Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 5)
                    {
                        playerScript.controller.GetComponent<Act5Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 6)
                    {
                        playerScript.controller.GetComponent<Act6Controller>().ShowSubtitles = false;
                    }
                }
            }
            if (PlayerPrefs.GetString("ADS") == "On")
            {
                adsToggle.isOn = true;
                if(!isMenu)
                playerScript.useADS = true;
            }
            else
            {
                adsToggle.isOn = false;
                if(!isMenu)
                playerScript.useADS = false;
            }

            if (PlayerPrefs.GetString("Grass") == "On")
            {
                grassToggle.isOn = true;
                if(!isMenu)
                {
                    if (terrains.Length != 0)
                    {
                        foreach (Terrain t in terrains)
                        {
                            t.detailObjectDistance = 40;
                        }
                    }
                }
             
              
            }
            else
            {
                grassToggle.isOn = false;
                if (!isMenu)
                {
                    if (terrains.Length != 0)
                    {
                        foreach (Terrain t in terrains)
                        {
                            t.detailObjectDistance = 0;
                        }
                    }
                }


            }

            if (PlayerPrefs.GetString("Bloom") == "On")
            {
                bloomToggle.isOn = true;
                volume.profile.TryGetSettings(out Bloom bloom);
                bloom.active = true;
                volume.enabled = true;
                layer.enabled = true;
                noOfEffects += 1;
            }
            if (PlayerPrefs.GetString("AO") == "On")
            {
                aoToggle.isOn = true;
                volume.profile.TryGetSettings(out AmbientOcclusion ao);
                ao.active = true;
                volume.enabled = true;
                layer.enabled = true;
                noOfEffects += 1;
            }
            if (PlayerPrefs.GetString("Vignette") == "On")
            {
                vignetteToggle.isOn = true;
                volume.profile.TryGetSettings(out Vignette x);
                x.active = true;
                volume.enabled = true;
                layer.enabled = true;
                noOfEffects += 1;
            }
           
            if (PlayerPrefs.GetString("Grain") == "On")
            {
                grainToggle.isOn = true;
                volume.profile.TryGetSettings(out Grain x);
                x.active = true;
                volume.enabled = true;
                layer.enabled = true;
                noOfEffects += 1;
            }
            if(introCamProfile != null)
            {
                introCamProfile.profile = volume.profile;
            }
            else
            {
                Debug.Log("There's no intro cam profile attached");
            }

            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
            if(!isMenu)
            {
                playerScript.MainController.m_MouseLook.Sensitivity = sensitivitySlider.value;
            }

            fpsDD.value = PlayerPrefs.GetInt("FPS");



            #region Value Change Listeners
            adsToggle.onValueChanged.AddListener(delegate { OnADSChanged(); });
            subtitlesToggle.onValueChanged.AddListener(delegate { OnSubtitlesChanged(); });
            grassToggle.onValueChanged.AddListener(delegate { OnGrassChanged(); });
            minimapToggle.onValueChanged.AddListener(delegate { OnMMChanged(); });
            vignetteToggle.onValueChanged.AddListener(delegate { OnVignetteChanged(); });
            motionBlurToggle.onValueChanged.AddListener(delegate { OnMotionBlurChanged(); });
            grainToggle.onValueChanged.AddListener(delegate { OnGrainChanged(); });
            bloomToggle.onValueChanged.AddListener(delegate { OnBloomChanged(); });
            gqDD.onValueChanged.AddListener(delegate { OnGraphicsChanged(); });
            fpsDD.onValueChanged.AddListener(delegate { OnFPSChanged(); });
            dofToggle.onValueChanged.AddListener(delegate { OnDOFChanged(); });
            aoToggle.onValueChanged.AddListener(delegate { ONAOChanged(); });
            sensitivitySlider.onValueChanged.AddListener(delegate { OnSensitivityChange(); });
            masterVolumeSlider.onValueChanged.AddListener(delegate { OnMasterVolumeChanged(); });
            musicVolumeSlider.onValueChanged.AddListener(delegate { OnMusicVolumeChanged(); });
            camFarPlaneSlider.onValueChanged.AddListener(delegate { OnFarPlaneChanged(); });
            ShadowsToggle.onValueChanged.AddListener(delegate { OnShadowsChanged(); });
            waterReflectionToggle.onValueChanged.AddListener(delegate { OnWaterReflectionChanged(); });
            #endregion

        }
        CursorLockMode lockModePrev;
        public TMP_Text GraphicsInfo;
        bool prevSway;
        public void OpenSettings()
        {
            if(playerScript != null)
            {
                if(playerScript.isDead)
                {
                    return;
                }
            }
            if(!isMenu && introDirector.state == PlayState.Playing && !isAct6)
            {
                introDirector.Pause();
            }
            isSettingsOpen = true;
            lockModePrev = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
            if(!isMenu)
            {
                settingsPanel.SetTrigger("Open");
            }
            GameplayImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 255);
            GraphicsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
            ImageEffectsImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
            AudioImage.color = new Color(GameplayImage.color.r, GameplayImage.color.g, GameplayImage.color.b, 0);
            GamePlayPanel.SetActive(true);
            GraphicsPanel.SetActive(false);
            AudioPanel.SetActive(false);
            ImageEffectsPanel.SetActive(false);
            if (!isMenu)
            {
                playerScript.isFirePressed = false;
                prevAcceptFireControls = playerScript.AcceptFireControls;
                playerScript.AcceptFireControls = false;
                prevInvincible = playerScript.isInvincible;
                playerScript.isInvincible = true;
                interactables.SetActive(false);
                prevSway = sway.enabled;
                sway.enabled = false;
            }
           
        }
        bool prevInvincible = false;
        public void CloseSettings()
        {
            isSettingsOpen = false;
            if(introDirector != null)
            {
                if (introDirector.state == PlayState.Paused)
                {
                    introDirector.Resume();
                }
            }
           
            if (!isMenu)
            {
                settingsPanel.SetTrigger("Close");
            }
            if (!isMenu)
            {
                Cursor.lockState = lockModePrev;
                playerScript.isInvincible = prevInvincible;
                playerScript.AcceptFireControls = prevAcceptFireControls;
                if(introDirector.state != PlayState.Playing)
                {
                    interactables.SetActive(true);
                }
                sway.enabled = prevSway;
            }
           
        }

        public void OpenTaskPanel()
        {
            if (!isMenu && introDirector.state != PlayState.Playing)
            {
                if (isSettingsOpen)
                {
                    CloseSettings();
                }
                if (isPausePanelOpen)
                {
                    ClosePausePanel();
                }

                playerScript.MainController.inputs = Vector2.zero;
                prevInvincible = playerScript.isInvincible;
                prevAcceptFireControls = playerScript.AcceptFireControls;
                playerScript.isInvincible = true;
                playerScript.AcceptFireControls = false;
                playerScript.AcceptMovement = false;
                isTaskPanelOpen = true;
                taskPanel.SetTrigger("Open");
                Cursor.lockState = CursorLockMode.None;
            }
          
        
        }
        public void CloseTaskPanel()
        {
            isTaskPanelOpen = false;
            taskPanel.SetTrigger("Close");
            Cursor.lockState = CursorLockMode.Locked;


            playerScript.isInvincible = false;
            playerScript.AcceptFireControls = prevAcceptFireControls;
            playerScript.AcceptMovement = true;
        }

        public Animator quitPanel;
        public Animator pausePanel;
        public Sway sway;

        #region Quit and Pause
        bool isQuitPanelOpen = false;
        bool isPausePanelOpen = false;
        public TMP_Text difficultyText;
        public void OpenPausePanel()
        {
            if(!playerScript.isDead)
            {
                if (!isMenu && introDirector.state == PlayState.Playing && !isAct6)
                {
                    introDirector.Pause();
                }
                isPausePanelOpen = true;
                if (PlayerPrefs.GetInt("Difficulty") == 0)
                {
                    difficultyText.text = "DIFFICULTY: " + "EASY";
                }
                else if (PlayerPrefs.GetInt("Difficulty") == 1)
                {
                    difficultyText.text = "DIFFICULTY: " + "FIERCE";
                }
                else if (PlayerPrefs.GetInt("Difficulty") == 2)
                {
                    difficultyText.text = "DIFFICULTY: " + "REALITY";
                }
                Cursor.lockState = CursorLockMode.None;
                playerScript.isFirePressed = false;
                playerScript.isInvincible = true;
                pausePanel.SetTrigger("Open");
                prevAcceptFireControls = playerScript.AcceptFireControls;
                playerScript.AcceptFireControls = false;
                sway.enabled = false;
            }
          
        }
        bool prevAcceptFireControls = true;
        public void ClosePausePanel()
        {
            if (introDirector != null)
            {
                if (introDirector.state == PlayState.Paused)
                {
                    introDirector.Resume();
                }
            }
            isPausePanelOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            playerScript.isInvincible = false;
            pausePanel.SetTrigger("Close");
            playerScript.AcceptFireControls = true;
            sway.enabled = true;
        }
        public void OpenQuitPanel()
        {
            isQuitPanelOpen = true;
            playerScript.isFirePressed = false;
  
            playerScript.isInvincible = true;
            quitPanel.SetTrigger("Open");
        }
        public void CloseQuitPanel()
        {
            isQuitPanelOpen = false;
            quitPanel.SetTrigger("Close");
        }
        public void Quit()
        {
            PlayerPrefs.SetString("QuitFromAct1", "DontOpenTraining");
            PlayerPrefs.SetString("CurrentLoadScene", "MainMenu");
            playerScript.isInvincible = false;
            SceneManager.LoadScene("Loading");
        }
        #endregion

        #region SettingChanges

        public void OnGraphicsChanged()
        {
            QualitySettings.SetQualityLevel(gqDD.value);
            PlayerPrefs.SetInt("Graphics", gqDD.value);

            OnShadowsChanged();
            GraphicsInfo.text = "GRAPHICS QUALITY: " + QualitySettings.names[QualitySettings.GetQualityLevel()];

            ShowSettingsUpdatedMSG();
        }
        void OnSensitivityChange()
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
            if(!isMenu)
            {
                playerScript.MainController.m_MouseLook.Sensitivity = sensitivitySlider.value;
            }
            ShowSettingsUpdatedMSG();
        }
        void OnGrassChanged()
        {
            if (grassToggle.isOn)
            {
                if(terrains.Length != 0)
                {
                    foreach (Terrain t in terrains)
                    {
                        t.detailObjectDistance = 40;
                    }
                }
               
                PlayerPrefs.SetString("Grass", "On");
            }
            else
            {
                if (terrains.Length != 0)
                {
                    foreach (Terrain t in terrains)
                    {
                        t.detailObjectDistance = 0;
                    }
                }
                PlayerPrefs.SetString("Grass", "Off");
            }
            ShowSettingsUpdatedMSG();
        }
        void OnWaterReflectionChanged()
        {
            if (waterReflectionToggle.isOn)
            {
                if(waterScript != null)
                {
                    waterScript.waterMode = Water.WaterMode.Reflective;
                }
                PlayerPrefs.SetString("WaterReflection", "On");
            }
            else
            {
                if (waterScript != null)
                {
                    waterScript.waterMode = Water.WaterMode.Simple;
                }
                PlayerPrefs.SetString("WaterReflection", "Off");
            }
            ShowSettingsUpdatedMSG();
        }
        void OnShadowsChanged()
        {
            if (ShadowsToggle.isOn)
            {
                QualitySettings.shadows = ShadowQuality.All;
                PlayerPrefs.SetString("Shadows", "On");
            }
            else
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                PlayerPrefs.SetString("Shadows", "Off");
            }
            ShowSettingsUpdatedMSG();
        }
        public bool isAct5OrAct3 = false; //both acts require high clip plane
        public bool isAct6 = false;
        public float minPlane = 380;
        void OnFarPlaneChanged()
        {
            PlayerPrefs.SetFloat("CameraFarPlane", camFarPlaneSlider.value);
            farPlaneText.text = camFarPlaneSlider.value.ToString();
            farPlaneGO.SetActive(true);
            if(!isMenu && !isAct5OrAct3)
            {
                mainCam.farClipPlane = camFarPlaneSlider.value;
            }
            else if(isAct5OrAct3)
            {
                if(camFarPlaneSlider.value >= minPlane)
                {
                    mainCam.farClipPlane = camFarPlaneSlider.value;
                }
                else
                {
                    mainCam.farClipPlane = minPlane;
                }
            }
            CancelInvoke(nameof(HideFarPlaneValue));
            Invoke(nameof(HideFarPlaneValue), 3f);
            ShowSettingsUpdatedMSG();
        }
        void HideFarPlaneValue()
        {
            farPlaneGO.SetActive(false);
        }
        void OnMasterVolumeChanged()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            AudioListener.volume = masterVolumeSlider.value;
            ShowSettingsUpdatedMSG();
        }
        void OnMusicVolumeChanged()
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            float musicvolume = musicVolumeSlider.value;
            foreach (AudioSource audioSource in musicSources)
            {
                audioSource.volume = ((musicvolume * 100) / 100) * Volumes[audioSource];
            }
            ShowSettingsUpdatedMSG();
        }
        public void OnResetStettingsClicked()
        {
            SetDefaultSettings();
        }
        void ShowSettingsUpdatedMSG()
        {
            CancelInvoke(nameof(HideSUSMG));
            SettingsUpdated.SetActive(false);
            SettingsUpdated.SetActive(true);
            Invoke(nameof(HideSUSMG), 5f);
        }
        void HideSUSMG()
        {
            SettingsUpdated.SetActive(false);
        }

        public void SetDefaultSettings()
        {
            if (!isResetting)
            {
                isResetting = true;
                PlayerPrefs.SetFloat("Sensitivity", 0.2f);
                sensitivitySlider.value = 0.2f;
                if (!isMenu)
                {
                    playerScript.MainController.m_MouseLook.Sensitivity = sensitivitySlider.value;
                }

                PlayerPrefs.SetFloat("MusicVolume", 1f);
                musicVolumeSlider.value = 1f;
                foreach (AudioSource audioSource in musicSources)
                {
                    audioSource.volume = 1f;
                }
                musicVolumeSlider.value = 1f;

                PlayerPrefs.SetString("Grass", "On");
                if (terrains.Length != 0)
                {
                    foreach (Terrain t in terrains)
                    {
                        t.detailObjectDistance = 40;
                    }
                }
                grassToggle.isOn = true;

                PlayerPrefs.SetFloat("MasterVolume", 1f);
                masterVolumeSlider.value = 1f;
                AudioListener.volume = 1f;

                PlayerPrefs.SetFloat("CameraFarPlane", 120f);
                camFarPlaneSlider.value = 120f;
                if (!isMenu && !isAct5OrAct3)
                {
                    mainCam.farClipPlane = 120f;
                }
                else if(isAct5OrAct3)
                {
                    mainCam.farClipPlane = minPlane;
                }

                PlayerPrefs.SetInt("Graphics", 2);
                gqDD.value = 2;
                QualitySettings.SetQualityLevel(1);

                PlayerPrefs.SetInt("FPS", 2);
                fpsDD.value = 2;
                SetFrameRate(2);

                PlayerPrefs.SetString("WaterReflection", "On");
                if (waterScript != null)
                {
                    waterScript.waterMode = Water.WaterMode.Reflective;
                }
                waterReflectionToggle.isOn = true;

                PlayerPrefs.SetString("Shadows", "On");
                ShadowsToggle.isOn = true;
                if (!isMenu)
                {
                    QualitySettings.shadows = ShadowQuality.Disable;
                }

                PlayerPrefs.SetString("ADS", "On");
                adsToggle.isOn = true;
                if (!isMenu)
                {
                    playerScript.useADS = true;
                }

                PlayerPrefs.SetString("ShowMM", "Off");
                minimapToggle.isOn = false;
                if (!isMenu)
                {
                    miniMap.SetActive(false);
                    minimapCam.SetActive(false);
                    batteryDisplayLeft.SetActive(false);
                    batteryDisplayRight.SetActive(true);
                }

                PlayerPrefs.SetString("Bloom", "Off");
                bloomToggle.isOn = false;
                volume.profile.TryGetSettings(out Bloom bloom);
                bloom.active = false;

                PlayerPrefs.SetString("AO", "Off");
                bloomToggle.isOn = false;
                volume.profile.TryGetSettings(out AmbientOcclusion ao);
                ao.active = false;

                PlayerPrefs.SetString("MotionBlur", "Off");
                motionBlurToggle.isOn = false;
                volume.profile.TryGetSettings(out MotionBlur mb);
                mb.active = false;


                PlayerPrefs.SetString("DepthOfField", "Off");
                dofToggle.isOn = false;
                volume.profile.TryGetSettings(out DepthOfField dof);
                dof.active = false;

                PlayerPrefs.SetString("Grain", "Off");
                grainToggle.isOn = false;
                volume.profile.TryGetSettings(out Grain grain);
                grain.active = false;

                PlayerPrefs.SetString("Subtitles", "On");
                subtitlesToggle.isOn = true;
                if (!isMenu)
                {
                    if (playerScript.ActNumber == 1)
                    {
                        playerScript.controller.GetComponent<Act1Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 2)
                    {
                        playerScript.controller.GetComponent<Act2Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 3)
                    {
                        playerScript.controller.GetComponent<Act3Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 4)
                    {
                        playerScript.controller.GetComponent<Act4Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 5)
                    {
                        playerScript.controller.GetComponent<Act5Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 6)
                    {
                        playerScript.controller.GetComponent<Act6Controller>().ShowSubtitles = true;
                    }
                }

                PlayerPrefs.SetString("Vignette", "Off");
                vignetteToggle.isOn = false;
                volume.profile.TryGetSettings(out Vignette vg);
                vg.active = false;

                volume.enabled = false;
                layer.enabled = false;

                PlayerPrefs.SetString("isUsingDefaultControls", "Yes");

                Debug.Log("All settings were set to default");
                isResetting = false;
                if (!FirstTime)
                {
                    ShowSettingsUpdatedMSG();
                }
                else
                {
                    FirstTime = false;
                }
            }

        }
        bool isResetting = false;

        void OnADSChanged()
        {
            if (adsToggle.isOn)
            {
                PlayerPrefs.SetString("ADS", "On");
                Debug.Log("Turned on ADS");
                if(!isMenu)
                {
                    playerScript.useADS = true;
                }
            }
            else
            {
                PlayerPrefs.SetString("ADS", "Off");
                if(!isMenu)
                {
                    playerScript.useADS = false;
                }
            }
            ShowSettingsUpdatedMSG();
        }
        void OnSubtitlesChanged()
        {
            if (subtitlesToggle.isOn)
            {
                PlayerPrefs.SetString("Subtitles", "On");
                Debug.Log("Turned on subtitles");
                if (!isMenu)
                {
                    if (playerScript.ActNumber == 1)
                    {
                        playerScript.controller.GetComponent<Act1Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 2)
                    {
                        playerScript.controller.GetComponent<Act2Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 3)
                    {
                        playerScript.controller.GetComponent<Act3Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 4)
                    {
                        playerScript.controller.GetComponent<Act4Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 5)
                    {
                        playerScript.controller.GetComponent<Act5Controller>().ShowSubtitles = true;
                    }
                    else if (playerScript.ActNumber == 6)
                    {
                        playerScript.controller.GetComponent<Act6Controller>().ShowSubtitles = true;
                    }
                }
            }
            else
            {
                PlayerPrefs.SetString("Subtitles", "Off");
                Debug.Log("Turned off subtitles");
                if (!isMenu)
                {
                    if (playerScript.ActNumber == 1)
                    {
                        playerScript.controller.GetComponent<Act1Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 2)
                    {
                        playerScript.controller.GetComponent<Act2Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 3)
                    {
                        playerScript.controller.GetComponent<Act3Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 4)
                    {
                        playerScript.controller.GetComponent<Act4Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 5)
                    {
                        playerScript.controller.GetComponent<Act5Controller>().ShowSubtitles = false;
                    }
                    else if (playerScript.ActNumber == 6)
                    {
                        playerScript.controller.GetComponent<Act6Controller>().ShowSubtitles = false;
                    }
                }
            }
            ShowSettingsUpdatedMSG();
        }
        void OnMMChanged()
        {
            if (minimapToggle.isOn)
            {
                PlayerPrefs.SetString("ShowMM", "On");
                Debug.Log("Always show MM on");
                if(!isMenu)
                {
                    minimapCam.SetActive(true);
                    miniMap.SetActive(true);
                    batteryDisplayLeft.SetActive(true);
                    batteryDisplayRight.SetActive(false);
                }
               
            }
            else
            {
                PlayerPrefs.SetString("ShowMM", "Off");
                Debug.Log("Turned off always show Mini map");
                if(!isMenu)
                {
                    minimapCam.SetActive(false);
                    miniMap.SetActive(false);
                    batteryDisplayLeft.SetActive(false);
                    batteryDisplayRight.SetActive(true);
                }
              
            }
            ShowSettingsUpdatedMSG();
        }
        void OnFPSChanged()
        {
            PlayerPrefs.SetInt("FPS", fpsDD.value);
            Debug.Log("add here");
            ShowSettingsUpdatedMSG();
        }
        void OnBloomChanged()
        {
            volume.profile.TryGetSettings(out Bloom x);
            if (bloomToggle.isOn)
            {
                PlayerPrefs.SetString("Bloom", "On");
                Debug.Log("bloom is on");
                x.active = true;
                if (noOfEffects == 0)
                {
                    noOfEffects += 1;
                    volume.enabled = true;
                    layer.enabled = true;
                }
                else
                {
                    noOfEffects += 1;
                }
            }
            else
            {
                PlayerPrefs.SetString("Bloom", "Off");
                Debug.Log("Turned off bloom");
                x.active = false;
                noOfEffects -= 1;
                if (noOfEffects == 0)
                {
                    volume.enabled = false;
                    layer.enabled = false;
                }
            }
            ShowSettingsUpdatedMSG();
        }
        void OnDOFChanged()
        {
            if (dofToggle.isOn)
            {
                PlayerPrefs.SetString("DepthOfField", "On");
                Debug.Log("dof is on, this will take place only in intros");

            }
            else
            {
                PlayerPrefs.SetString("DepthOfField", "Off");
                Debug.Log("Turned off dof");
            }
            ShowSettingsUpdatedMSG();
        }
        void OnMotionBlurChanged()
        {
            volume.profile.TryGetSettings(out MotionBlur x);
            if (motionBlurToggle.isOn)
            {
                PlayerPrefs.SetString("MotionBlur", "On");
                Debug.Log("motion blur is on");
                x.active = true;
                if (noOfEffects == 0)
                {
                    noOfEffects += 1;
                    volume.enabled = true;
                    layer.enabled = true;

                }
                else
                {
                    noOfEffects += 1;
                }
            }
            else
            {
                PlayerPrefs.SetString("MotionBlur", "Off");
                Debug.Log("Turned off motion blur");
                x.active = false;
                noOfEffects -= 1;
                if (noOfEffects == 0)
                {
                    volume.enabled = false;
                    layer.enabled = false;
                }
            }
            ShowSettingsUpdatedMSG();
        }
        void OnGrainChanged()
        {
            volume.profile.TryGetSettings(out Grain x);
            if (grainToggle.isOn)
            {
                PlayerPrefs.SetString("Grain", "On");
                Debug.Log("grain is on");
                x.active = true;
                if (noOfEffects == 0)
                {
                    noOfEffects += 1;
                    volume.enabled = true;
                    layer.enabled = true;
                }
                else
                {
                    noOfEffects += 1;
                }
            }
            else
            {
                PlayerPrefs.SetString("Grain", "Off");
                Debug.Log("Turned off grain");
                x.active = false;
                noOfEffects -= 1;
                if (noOfEffects == 0)
                {
                    volume.enabled = false;
                    layer.enabled = false;
                }
            }
            ShowSettingsUpdatedMSG();
        }
        void ONAOChanged()
        {
            volume.profile.TryGetSettings(out AmbientOcclusion x);
            if (aoToggle.isOn)
            {
                PlayerPrefs.SetString("AO", "On");
                Debug.Log("AO is on");
                x.active = true;
                if (noOfEffects == 0)
                {
                    noOfEffects += 1;
                    volume.enabled = true;
                    layer.enabled = true;
                }
                else
                {
                    noOfEffects += 1;
                }
            }
            else
            {
                PlayerPrefs.SetString("AO", "Off");
                Debug.Log("Turned off AO");
                x.active = false;
                noOfEffects -= 1;
                if (noOfEffects == 0)
                {
                    volume.enabled = false;
                    layer.enabled = false;
                }
            }
            ShowSettingsUpdatedMSG();
        }
        void OnVignetteChanged()
        {
            volume.profile.TryGetSettings(out Vignette x);
            if (vignetteToggle.isOn)
            {
                PlayerPrefs.SetString("Vignette", "On");
                Debug.Log("vignette is on");
                x.active = true;
                if (noOfEffects == 0)
                {
                    noOfEffects += 1;
                    volume.enabled = true;
                    layer.enabled = true;
                }
                else
                {
                    noOfEffects += 1;
                }
            }
            else
            {
                PlayerPrefs.SetString("Vignette", "Off");
                Debug.Log("Turned off vignette");
                x.active = false;
                noOfEffects -= 1;
                if (noOfEffects == 0)
                {
                    volume.enabled = false;
                    layer.enabled = false;
                }
            }
            ShowSettingsUpdatedMSG();
        }

        #endregion

        private void Update()
        {
            if(!isMenu)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    if (isSettingsOpen)
                    {
                        CloseSettings();
                    }
                    else
                    {
                        OpenSettings();
                    }
                }
                if(Input.GetKeyDown(KeyCode.T))
                {
                    if(isTaskPanelOpen)
                    {
                        CloseTaskPanel();
                    }  
                    else
                    {
                        OpenTaskPanel();
                    }
                }
            }
          
            if (!isMenu && Input.GetKeyDown(KeyCode.Escape))
            {
                if (isSettingsOpen && !isPausePanelOpen && !isQuitPanelOpen)
                {
                    CloseSettings();
                }
                if(nearbyPanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
                {
                    nearbyPanel.SetTrigger("Close");
                }
                else
                {
                    if (!isPausePanelOpen && !isSettingsOpen)
                    {
                        OpenPausePanel();
                    }
                    else if(isPausePanelOpen && !isSettingsOpen)
                    {
                        ClosePausePanel();
                    }
                    else if(isQuitPanelOpen && !isPausePanelOpen)
                    {
                        CloseQuitPanel();
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.X))
            {
                if (nearbyPanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
                {
                    nearbyPanel.SetTrigger("Close");
                }
            }

        }

        void SetFrameRate(int index)
        {
            if(index == 0)
            {
                Application.targetFrameRate = 30;
            }
            if (index == 1)
            {
                Application.targetFrameRate = 50;
            }
            if (index == 2)
            {
                Application.targetFrameRate = 60;
            }
            if (index == 3)
            {
                Application.targetFrameRate = 90;
            }
            if (index == 4)
            {
                Application.targetFrameRate = 120;
            }
            if (index == 5)
            {
                Application.targetFrameRate = 140;
            }
        }

    }
}
