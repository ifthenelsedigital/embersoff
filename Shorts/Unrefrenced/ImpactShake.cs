using UnityEngine;

public class ImpactShake : MonoBehaviour
{
    public Animator cam;
    public void ShowPowerfulImpact() // yaaa show that the magazine is attached so powerfully that the player vibrated
    {
        cam.SetTrigger("ReloadImpact");
    }
}
