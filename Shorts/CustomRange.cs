using UnityEngine;

namespace IfThenElse
{
    public class CustomRange : MonoBehaviour
    {
        public int AreaCode;
        public delegate void OnObjectEnter(Collider collider);
        public delegate void OnObjectExit(Collider collider);
        public OnObjectEnter onObjectEnter;
        public OnObjectExit onObjectExit;

        public void OnTriggerEnter(Collider other)
        {
            if (onObjectEnter != null && other.GetType() == typeof(CapsuleCollider))
            {
                onObjectEnter(other);
            }
                
        }
        public void OnTriggerExit(Collider other)
        {
            if (onObjectExit != null && other.GetType() == typeof(CapsuleCollider))
                onObjectExit(other);
        }
    }
}
