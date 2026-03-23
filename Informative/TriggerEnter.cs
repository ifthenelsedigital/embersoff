using UnityEngine;

public class TriggerEnter : MonoBehaviour
{
    public delegate void OnObjectTrigger();
    public OnObjectTrigger onObjectTrigger;
    public delegate void OnObjectExit();
    public OnObjectTrigger onObjectExit;
    public Transform contact;
    public bool requireCapCollider = false;
    public int AreaCode;
    public bool isReached = false;
    public void OnTriggerEnter(Collider other)
    {
        if (other.transform == contact)
        {
            if(requireCapCollider)
            {
                if(other.GetType() == typeof(CapsuleCollider))
                {
                    isReached = true;
                    onObjectTrigger?.Invoke();
                }
                
            }
            else
            {
                isReached = true;
                onObjectTrigger();
            }
        }
        
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.transform == contact)
        {
            if (requireCapCollider)
            {
                if (other.GetType() == typeof(CapsuleCollider))
                {
                    onObjectExit?.Invoke();
                }

            }
            else
            {
                onObjectExit?.Invoke();
            }
        }

    }

}
