using UnityEngine;
namespace IfThenElse
{
    public class ShootableObject : MonoBehaviour
    {
        public Animator anim;
        public string material;
        bool isDestroyed = false;
        public TrainingController controller;
        public void OnShot()
        {
            if(!isDestroyed)
            {
                anim.SetTrigger("GoDown");
                isDestroyed = true;
                controller.OnTargetShot();
            }
           
        }
      
    }
}
