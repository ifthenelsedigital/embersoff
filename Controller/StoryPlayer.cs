using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace IfThenElse
{
    public class StoryPlayer : MonoBehaviour
    {
        [Header("Setup")]
        public GameObject SubtitlesOption;
        public GameObject SubtitleCover;
        public GameObject PausePanel;
        public Toggle SubtitleToggle;

        [Header("Video ActClips")]
        public VideoClip Act1Story;
        public VideoClip Act2Story;
        public VideoClip Act3Story;
        public VideoClip Act4Story;
        public VideoClip Act5Story;
        public VideoClip Act6Story;
        public VideoClip EndingVideo;

        [Header("Display")]
        public VideoPlayer videoPlayer;
        public GameObject loadingWave;
        readonly Dictionary<string, VideoClip> ActClips = new();

        bool ReadyToPlay = false;
        void Awake()
        {
            ActClips.Add("Act1", Act1Story);
            ActClips.Add("Act2", Act2Story);
            ActClips.Add("Act3", Act3Story);
            ActClips.Add("Act4", Act4Story);
            ActClips.Add("Act5", Act5Story);
            ActClips.Add("Act6", Act6Story);
            ActClips.Add("End", EndingVideo);

            if (ActClips[PlayerPrefs.GetString("SelectedAct")] != null)
            {
                InitStories();
            }
            else
            {
                Debug.Log("Story not available for " + PlayerPrefs.GetString("SelectedAct"));
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(PlayerPrefs.GetString("SelectedAct"));
            }
        }
        void InitStories()
        {
           
            loadingWave.SetActive(false);
            videoPlayer.clip = ActClips[PlayerPrefs.GetString("SelectedAct")];
            loadingWave.SetActive(true);
            if (PlayerPrefs.HasKey("Subtitles"))
            {
                ReadyToPlay = true;
                SubtitlesOption.SetActive(false);
                if (PlayerPrefs.GetInt("Subtitles") == 0)
                {
                    SubtitleCover.SetActive(true);
                    SubtitleToggle.isOn = true;
                }
                else
                {
                    SubtitleCover.SetActive(false);
                    SubtitleToggle.isOn = false;
                }
            }
            else
            {
                ReadyToPlay = false;
                SubtitlesOption.SetActive(true);
            }
            StartCoroutine(LoadStory());
        }
        
        IEnumerator LoadStory()
        {
       
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
            loadingWave.SetActive(false);
            if(ReadyToPlay)
            {
                videoPlayer.Play();
                videoPlayer.loopPointReached += OnStoryFinished;
            }
            
        }
        public void SkipStory()
        {
            PlayerPrefs.SetString("CurrentLoadScene", PlayerPrefs.GetString("SelectedAct"));
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Loading");
        }
        
        void OnStoryFinished(VideoPlayer player)
        {
            PlayerPrefs.SetString(PlayerPrefs.GetString("SelectedAct") + "CompletedStory", "Yes");
            PlayerPrefs.SetString("CurrentLoadScene", PlayerPrefs.GetString("SelectedAct"));
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Loading");
        }
        public void ChooseOption(int option) //0 caption off, 1 caption on
        {
            PlayerPrefs.SetInt("Subtitles", option);
            if(option == 0)
            {
                SubtitleCover.SetActive(true);
                SubtitleToggle.isOn = true;
            }
            else
            {
                SubtitleCover.SetActive(false);
                SubtitleToggle.isOn = false;
            }
            ReadyToPlay = true;
            SubtitlesOption.SetActive(false);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnStoryFinished;
        }
        private void Update()
        {
            if(videoPlayer.isPlaying)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    videoPlayer.Pause();
                    PausePanel.SetActive(true);
                }
            }
            else if(PausePanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    videoPlayer.Play();
                    PausePanel.SetActive(false);
                }
            }
        }
        public void SubtitleToggleClicked()
        {
            if(SubtitleToggle.isOn) //this means user has turned off subtitles
            {
                SubtitleCover.SetActive(true);
                PlayerPrefs.SetInt("Subtitles", 0);
            }
            else
            {
                SubtitleCover.SetActive(false);
                PlayerPrefs.SetInt("Subtitles", 1);
            }
            videoPlayer.Play();
            PausePanel.SetActive(false);
        }
        public void PausePanelClicked()
        {
            videoPlayer.Play();
            PausePanel.SetActive(false);
        }
    }
}
