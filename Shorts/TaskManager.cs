using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public int activeTaskNumber = 0;
    public List<Toggle> TaskToggles = new();
    public TMP_Text ActAndDifficultyText;

    public Sprite TaskCompleteIcon;
    public Sprite TaskActiveIcon;

    private void Start()
    {
        OpenTasks();
        Debug.Log("Tasks in this act: " + TaskToggles.Count);
    }
    public void OpenTasks()
    {
        Debug.Log("opening tasks");
        if(ActAndDifficultyText.text == null)
        {
            int difficulty = PlayerPrefs.GetInt("Difficulty");
            string s = null;
            if (difficulty == 0)
            {
                s = "Easy";
            }
            else if (difficulty == 1)
            {
                s = "Fierce";
            }
            else if (difficulty == 2)
            {
                s = "Reality";
            }
            ActAndDifficultyText.text = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " | " + s;
        }

        foreach(Toggle task in TaskToggles)
        {
            if(TaskToggles.IndexOf(task) == activeTaskNumber)
            {
                Debug.Log("task activated");
                Image img = task.graphic.GetComponent<Image>();
                img.sprite = TaskActiveIcon;
                img.color = Color.yellow;
                task.isOn = true;
            }
        }
    }
    
    public void NextTask()
    {
        Image img = TaskToggles[activeTaskNumber].graphic.GetComponent<Image>();
        img.sprite = TaskCompleteIcon;
        img.color = Color.green;

        activeTaskNumber++;

        if(TaskToggles.Count >= activeTaskNumber)
        {
            Debug.Log("next task here");
            OpenTasks();
        }
    }

   
}
