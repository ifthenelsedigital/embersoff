using UnityEngine;
namespace IfThenElse
{

    public class AutoDestruct : MonoBehaviour, IPooledObject
    {
        public float DestructTime;
        public bool OnlyDeactivate = false;
        public bool isEnemyBag = false;
        public bool isParticleSystem = false;
        public ParticleSystem system;
        private void Awake()
        {
            Invoke(nameof(Destroy), DestructTime);
        }
        public void OnObjectSpawn()
        {
            Invoke(nameof(Destroy), DestructTime);
            if(isParticleSystem)
            {
                system.Play();
            }
        }
        private void Destroy()
        {         
            if (OnlyDeactivate)
            {
                gameObject.SetActive(false);
            }
            else
            {
               
                Destroy(gameObject);
            }
        }
    }
}
