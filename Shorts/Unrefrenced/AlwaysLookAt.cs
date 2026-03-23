using UnityEngine;

namespace IfThenElse
{
    public class AlwaysLookAt : MonoBehaviour
    {
        public Transform focus;
 
        void Update()
        {
            transform.LookAt(new Vector3(focus.position.x, transform.position.y, focus.position.z));
        }
    }
}
