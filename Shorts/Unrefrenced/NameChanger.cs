using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameChanger : MonoBehaviour
{
    public bool Perform;
    public string Replacement = "mixamorig:";
    void OnValidate()
    {
        transform.name = transform.name.Replace(Replacement, null);
        

    }

    
}
