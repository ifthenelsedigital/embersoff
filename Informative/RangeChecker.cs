using UnityEngine;

public class RangeChecker : MonoBehaviour
{
    public Transform target;

    public delegate void OnTargetInRange();
    public OnTargetInRange onTargetInRange;
    public delegate void OnTargetOutOfRange();
    public OnTargetOutOfRange onTargetOutOfRange;

    public void OnTriggerEnter(Collider other)
    {
        if(other.transform == target)
        {
            onTargetInRange();
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if(other.transform == target)
        {
            onTargetOutOfRange();
        }
    }
}
