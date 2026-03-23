using UnityEngine.Playables;
using UnityEngine;

public class FastForwardIntro : MonoBehaviour
{
    public PlayableDirector director;
    public int Least;
    private void Awake()
    {
        if(Application.isEditor)
        {
            director = GetComponent<PlayableDirector>();
        }
        else
        {
            this.enabled = false;
        }
    }
    void Update()
    {
        if(Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                director.time = director.duration - Least;
            }
        }
       
    }
}
