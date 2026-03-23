using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IfThenElse
{
    public class Plane : MonoBehaviour
    {
       
        public GameObject bomb;
        public Transform bombSpawn;
        public Transform endPoint;
        public float speed;
        public int actNumber;
        public int InitialStartDelay = 1;
        public float bombDropRate;
        public Player playerScript;
        public Animator cam;
        [Range(0, 900)]
        public int Health;
        public GameObject destroyedVFX;
        public AudioSource source;
        public AudioClip destroyedSound;
        public MeshRenderer[] toDisable;

        void Start()
        {
            InvokeRepeating(nameof(DropBomb), InitialStartDelay, bombDropRate);
        }
        void DropBomb()
        {
            Bomb b = Instantiate(bomb, bombSpawn.position, Quaternion.identity).GetComponent<Bomb>();
            b.isLaunched = true;
            b.isFromPlayer = false;
            b.ActNumber = actNumber;
            b.player = playerScript;
            b.playerCam = cam;
        }

        private void Update()
        {
            if(Health != 0)
            {
                if (Vector3.Distance(transform.position, endPoint.position) > 0f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
          
        }
        public void BulletHit(int d)
        {
            if(Health != 0)
            {
                Health -= d;
                if(Health <= 0)
                {
                    CancelInvoke(nameof(DropBomb));
                    destroyedVFX.SetActive(true);
                    source.clip = destroyedSound;
                    source.Play();
                    foreach(MeshRenderer t in toDisable)
                    {
                        t.enabled = false;
                    }
                    Invoke(nameof(Destroy), 4);
                }

            }
        }
        private void Destroy()
        {
            gameObject.SetActive(false);
        }
    }
}
