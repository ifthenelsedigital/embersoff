using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shatter : MonoBehaviour
{
    public MeshRenderer[] RenderersToDisable;
    public MeshCollider[] CollidersToDisable;
  
    public GameObject shatteredObject;
    public void ShatterObject()
    {
        foreach(MeshRenderer r in RenderersToDisable)
        {
            r.enabled = false;
        }
        if(CollidersToDisable != null)
        {
            foreach(MeshCollider c in CollidersToDisable)
            {
                c.sharedMesh = null;
                c.enabled = false;
            }
        }
        shatteredObject.SetActive(true);
    }
}
