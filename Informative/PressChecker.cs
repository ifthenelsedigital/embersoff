using UnityEngine;
namespace IfThenElse
{
    public class PressChecker : MonoBehaviour
    {
        public KeyCode TargetKey = KeyCode.P;
        public bool Check = false;
        public delegate void OnKeyPressed();
        public OnKeyPressed onKeyPressed;
        void Update()
        {
            if (Check)
            {
                if (Input.GetKeyDown(TargetKey))
                {
                    onKeyPressed?.Invoke();
                }
            }
        }
    }
}
