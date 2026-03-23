using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class EndController : MonoBehaviour
{
    public TMP_Text motivatingText;
    public string SteamURL;
    public Animator RatePanel;
    public Animator FeedbackAskPanel;
    public Animator FeedbackPanel;
    public Toggle HeartTouchingToggle;
    public TMP_InputField NameInputField;
    public TMP_InputField DescribeInputField;
    public Slider StarSlider;
    public TMP_Text starsText;
    public GameObject StarError;
    public GameObject NameError;

    private void Awake()
    {
        if (PlayerPrefs.GetInt("Difficulty") == 0)
        {
            motivatingText.text = "You've successfully completed this glorious pick of Embers Off in Easy difficulty! You can now try playing again with a more harder difficulty.";
        }
        else if(PlayerPrefs.GetInt("Difficulty") == 1)
        {
            motivatingText.text = "You've successfully completed this glorious pick of Embers Off in Fierce difficulty! You can now try playing again with a more harder difficulty.";
        }
        else
        {
            motivatingText.text = "You've successfully completed this glorious pick of Embers Off in Reality difficulty! You've fought till the toughest challenge to you got defeated!";
        }

        if(!PlayerPrefs.HasKey("StatsGiven"))
        {
            if(Application.isEditor)
            {
                ID = 1504;
                PlayerPrefs.SetInt("ID", ID);
            }
            else
            {
                ID = Random.Range(000000000, 999999999);
                PlayerPrefs.SetInt("ID", ID);
            }

            PlayFabClientAPI.LoginWithCustomID(new PlayFab.ClientModels.LoginWithCustomIDRequest
            {
                CreateAccount = true,
                CustomId = (ID).ToString(),
                
            }, LoggedIn, LoginFailure);
        }
      
    }
    int ID;
    bool isLoggedIn = false;
    void LoggedIn(LoginResult result)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = ID.ToString()
        }, StatsUpdated, StatsFailure);
    }
    void StatsUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Logged in successfully, your completed game has been marked");
        PlayerPrefs.SetInt("StatsGiven", 1);
        isLoggedIn = true;
    }
    void LoginFailure(PlayFabError error)
    {
        Debug.Log("login failed, " + error.Error);
    }
    void StatsFailure(PlayFabError error)
    {
        Debug.Log("mark stats failed, " + error.Error);
    }
    public void GoAhead() //on first continue btn
    {
        RatePanel.SetTrigger("Open");
    }

    public void RateOnSteam()
    {
        Application.OpenURL(SteamURL);
        RatePanel.SetTrigger("Close");
        if (PlayerPrefs.HasKey("GivenFeedback"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            FeedbackAskPanel.SetTrigger("Open");
        }
    }
    public void SkipSteamRating()
    {
        RatePanel.SetTrigger("Close");
        if(PlayerPrefs.HasKey("GivenFeedback"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            FeedbackAskPanel.SetTrigger("Open");
        }
    }
    public void OpenFeedbackPanel()
    {
        FeedbackAskPanel.SetTrigger("Close");
        FeedbackPanel.SetTrigger("Open");
    }
    public void SkipFeedback()
    {
        FeedbackAskPanel.SetTrigger("Close");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    public void CancelFeedback()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    public void OnStarsGiven()
    {
        StarSlider.minValue = 1;
        starsText.text = StarSlider.value + "/10";
    }
   
    public void SubmitFeedback()
    {
        if (NameInputField.text.Length == 0)
        {
            NameError.SetActive(true);
            Debug.Log("yes nukl");
            return;
        }
        else
        {
            NameError.SetActive(false);
            Debug.Log("s" + NameInputField.text.Length + "el");
        }
        if (StarSlider.minValue != 1)
        {
            StarError.SetActive(true);
            return;
        }
        else
        {
            StarError.SetActive(false);
        }


        if (NameInputField.text != null && StarSlider.value > 0)
        {
            Dictionary<string, string> playerData = new();
            playerData.Add("Name", NameInputField.textComponent.text);
            playerData.Add("HeartTouching", HeartTouchingToggle.isOn ? "Yes" : "No");
            playerData.Add("StarRating", StarSlider.value.ToString());
            playerData.Add("Feedback", DescribeInputField.text);

            WaitPanel.SetActive(true);
            WaitText.fontStyle = FontStyles.Normal;
            WaitText.text = "Submitting Feedback, please wait";

            if (!isLoggedIn)
            {
                PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
                {
                    CustomId = PlayerPrefs.GetInt("ID").ToString()
                },
                Result =>
                {
                    Debug.Log("Logged in, submitting feedback.");
                    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                    {
                        Data = playerData,
                        Permission = UserDataPermission.Public
                    }, FeedbackReceived, FeedbackReceiveError);
                },
                m_Error =>
                {
                    Debug.Log("login error, " + m_Error.Error);
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                });
            }
            else
            {
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                {
                    Data = playerData,
                    Permission = UserDataPermission.Public
                }, FeedbackReceived, FeedbackReceiveError);
            }
        }
       
       
    }

    public GameObject WaitPanel;
    public TMP_Text WaitText;
    void FeedbackReceived(UpdateUserDataResult result)
    {
        Debug.Log("Successfully Received Feedback, Thanks!");
        WaitText.fontStyle = FontStyles.Italic;
        WaitText.text = "Thank you for the Feedback!";
        Invoke(nameof(GoToMenu), 2.5f);
    }
    void GoToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    void FeedbackReceiveError(PlayFabError error)
    {
        Debug.Log("Feedback receive error, " + error.Error);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

    }
}
