using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUVS : MonoBehaviour
{
    [Range(0, 15)]
    public int index;
    public bool IsEnabled = true;
    void Awake()
    {
        if(IsEnabled)
        {
            List<Vector3> uvs = new List<Vector3>();
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            if (!mesh.isReadable)
            {
                Debug.Log(mesh.name);
            }
            mesh.GetUVs(0, uvs);
            for (int i = 0; i < uvs.Count; i++)
            {
                uvs[i] = new Vector3(uvs[i].x, uvs[i].y, index);
            }
            mesh.SetUVs(0, uvs);
        }
      
    }
   
}
