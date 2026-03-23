using UnityEngine;

namespace IfThenElse
{
    public class ArrowIndicator : MonoBehaviour
    {
        public bool isDamageIndicator = true;
        public Transform player;

        public Transform target;
        public RectTransform damageImage;
        public float showTime = 6f;
        public Animator anim;
        private void Update()
        {
            if(target != null)
            {
                Vector3 difference = player.position - target.position;
                Quaternion targetRot = Quaternion.LookRotation(difference);
                targetRot.z = -targetRot.y;
                targetRot.x = 0f;
                targetRot.y = 0f;

                Vector3 northDir = new Vector3(0f, 0f, player.eulerAngles.y);
                damageImage.localRotation = targetRot * Quaternion.Euler(northDir);
            }
            else
            {
                CancelInvoke("SwitchOffSelf");
                SwitchOffSelf();
            }
         
        }
        private void OnEnable()
        {
            CancelInvoke(nameof(SwitchOffSelf));
            if(isDamageIndicator)
            {
                anim.SetTrigger("Show");
                Invoke(nameof(SwitchOffSelf), showTime);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(true);
                indicator.SetActive(true);
            }
        }
        public GameObject indicator;
        public void SwitchOffSelf()
        {
            if(isDamageIndicator)
            {
                anim.SetTrigger("Hide");
            }
            else
            {
                indicator.SetActive(false);
            }
           
            enabled = false;
        }
    }
}