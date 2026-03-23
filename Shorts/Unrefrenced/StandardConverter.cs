using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardConverter : MonoBehaviour
{
    public bool l;
    private void OnValidate()
    {
        Material[] materials = GetComponent<MeshRenderer>().sharedMaterials;
        foreach (Material m in materials)
        {
            m.shader = Shader.Find("Standard");
        }
    }
}
