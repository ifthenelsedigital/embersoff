using System.Collections;
using UnityEngine;
namespace IfThenElse
{
    public class FlickerLights : MonoBehaviour
    {
        public bool ForcedRender;
        public int MaxOnTimes = 4;
        public Light TargetLight;
        public int MinimumIntensity;
        public float speed;
        int CurrentOnTime = 0;
        float DefaultIntensity;
        void Start()
        {
            if(!ForcedRender)
            {
                if (PlayerPrefs.GetInt("Graphics") > 0) //medium or high graphics required
                {
                    StartCoroutine(Flicker());
                }
                else
                {
                    this.enabled = false;
                }
            }
            else
            {
                StartCoroutine(Flicker());
            }

            DefaultIntensity = TargetLight.intensity;
        }
        IEnumerator Flicker()
        {
            yield return new WaitForSeconds(1.2f);
            while(CurrentOnTime != MaxOnTimes)
            {
                TargetLight.intensity = Mathf.Lerp(0, DefaultIntensity, speed);
              //  yield return new WaitForSeconds(speed);
                TargetLight.intensity = Mathf.Lerp(DefaultIntensity, MinimumIntensity, speed);
              //  yield return new WaitForSeconds(speed);
                CurrentOnTime++;
                yield return null;
            }
            TurnOff();
            StopCoroutine(Flicker());
        }
        void TurnOff()
        {
            TargetLight.intensity = 0;
            CurrentOnTime = 0;
            StartCoroutine(Flicker());
        }
    }
}
