using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IfThenElse
{
    public class BatteryDisplay : MonoBehaviour
    {
        Slider slider;
        // Start is called before the first frame update
        void Start()
        {
            slider = GetComponent<Slider>();
            slider.value = SystemInfo.batteryLevel;
        }


    }
}