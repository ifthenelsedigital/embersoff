using UnityEngine;
using System.Collections.Generic;

namespace IfThenElse
{

    public class Bomb : MonoBehaviour
    {
        public GameObject explosionVFX;
        public AudioSource explosionSFX;
        public MeshRenderer[] renderersToDisable;
        public bool isLaunchedFromMortar = false;
        public bool onlyDeactivate = false;
        public float explodeDelay = 1f;
        bool canExplode = true;
        public float Range;
        public int ActNumber;
        public Animator playerCam;
        public bool isFromPlayer = false;
        public bool isLaunched = false;
        [Tooltip("Shows smoke after explosion. Use only in cinematic shoots as it's expensive!")]
        public bool hasLongImpact = false;
        public GameObject smoke;
        public Player player;
        public bool isDropped = false;
        public int damage = 90;
        bool isExploded = false;
        public AudioSource waterEnterSound;
        public AudioClip waterExplosionSound;
        bool isInWater = false;
        private void Awake()
        {
            CancelInvoke(nameof(SelfDestroy));
            isExploded = false;
        }
        public void OnCollisionEnter(Collision collision)
        {     
            if(isLaunched && !isExploded)
            {
                if (!collision.collider.CompareTag("Player"))
                {
                    if (canExplode)
                    {
                        canExplode = false;
                        if(isLaunchedFromMortar || isDropped)
                        {

                            Explode();
                        }
                        else
                        {
                            canExplode = false;
                            isExploded = true;
                            Invoke(nameof(Explode), explodeDelay);
                        }
                      
                    }

                    if(collision.collider.CompareTag("Water"))
                    {
                        waterEnterSound.Play();
                        isInWater = true;
                    }
                }

              
            }
         
          
        }
        public AudioClip ringingSFX;
        public bool showSmokeTrail;
  
        public void Explode()
        {
            isExploded = true;
            if(renderersToDisable != null)
            {
                foreach(MeshRenderer mr in renderersToDisable)
                {
                    mr.enabled = false;
                }
            }
            if(showSmokeTrail)
            {
                Invoke(nameof(ExplodeAlt), 0.7f);
            }
            else
            {
                CancelInvoke(nameof(SelfDestroy));
                explosionVFX.SetActive(true);
                explosionVFX.GetComponent<ParticleSystem>().Play();
                if(isInWater)
                {
                    explosionSFX.clip = waterExplosionSound;
                }
                explosionSFX.Play();

                if (player == null)
                {
                    player = GameObject.Find("Player").GetComponent<Player>();
                }

                //Show impact on nearby things
                Collider[] nearbyItems = Physics.OverlapSphere(transform.position, Range);
                if (!isLaunchedFromMortar)
                {
                    playerCam.SetTrigger("ExplodeShake");
                }
               

                foreach (Collider t in nearbyItems)
                {
                    if(t.GetType() == typeof(CapsuleCollider))
                    if (t.CompareTag("Player"))
                    {
                        Debug.Log("You were standing near an explosive");
                        if(!player.isDead)
                        {                          
                            player.TakeDamage(damage, transform);
                            AudioSource ringSource = player.controller.GetComponent<AudioSource>();
                            ringSource.clip = ringingSFX;
                            ringSource.Play();
                            ringSource.loop = false;
                        }
                    }
                    else if (isFromPlayer && t.CompareTag("Soldier") && t.GetType() == typeof(CapsuleCollider))
                    {
                        if (ActNumber == 1)
                        {
                            Act1Soldier aS = t.transform.parent.GetComponent<Act1Soldier>();
                            if (aS.nationality == Act1Soldier.Nationality.Japanese)
                            {
                                aS.OnBulletShot(player.transform, 100, false);
                                List<int> killStats = new List<int>();
                                killStats.Add(0);
                                killStats.Add(1);
                                killStats.Add(3);
                                player.ShowKillStats(killStats);
                            }
                            else
                            {
                                aS.OnBulletShot(player.transform, 100, false);
                            }
                        }
                        else
                        {
                            Soldier s = t.transform.parent.GetComponent<Soldier>();
                            if (s.nationality == Soldier.Nationality.British)
                            {
                                s.OnBulletShot(player.transform, 100, false);
                                List<int> killStats = new List<int>();
                                killStats.Add(0);
                                killStats.Add(1);
                                killStats.Add(3);
                                player.ShowKillStats(killStats);
                            }
                            else
                            {
                                s.OnBulletShot(player.transform, 100, false);
                            }
                        }
                    }
                }

                foreach (Transform l in transform)
                {
                    if (l != explosionVFX.transform)
                        l.gameObject.SetActive(false);
                }
                if (isLaunchedFromMortar)
                {
                    GetComponent<MeshRenderer>().enabled = false;
                }
                if (hasLongImpact)
                {
                    Instantiate(smoke, transform);
                }
        //        Invoke("DestroyShakeNoise", 2f);
                Invoke("SelfDestroy", selfDestroyTime);
            }
           
        }

        void ExplodeAlt()
        {
            isExploded = true;
            CancelInvoke("SelfDestroy");
            explosionVFX.SetActive(true);
            explosionVFX.GetComponent<ParticleSystem>().Play();
            explosionSFX.Play();

            if (player == null)
            {
                player = GameObject.Find("Player").GetComponent<Player>();
            }

            //Show impact on nearby things
            Collider[] nearbyItems = Physics.OverlapSphere(transform.position, Range);
            if (!isLaunchedFromMortar && !isDropped)
            {
                player.transform.GetChild(0).GetComponent<Animator>().SetTrigger("ExplodeShake");
            }
            else if (!isDropped)
            {
                player.transform.GetChild(0).GetComponent<Animator>().SetTrigger("ExplodeShake");
               
            }


            foreach (Collider t in nearbyItems)
            {
                if (t.CompareTag("Player"))
                {
                    Debug.Log("You were standing near an explosive");
                    player.TakeDamage(damage, transform);
                    AudioSource ringSource = player.controller.GetComponent<AudioSource>();
                    ringSource.clip = ringingSFX;
                    ringSource.Play();
                    ringSource.loop = false;
                }
                else if (isFromPlayer && t.tag == "Soldier" && t.GetType() == typeof(CapsuleCollider))
                {
                    if (ActNumber == 1)
                    {
                        if (t.GetComponent<Act1Soldier>().nationality == Act1Soldier.Nationality.Japanese)
                        {
                            t.GetComponent<Act1Soldier>().OnBulletShot(GameObject.Find("Player").transform, 100, false);
                            List<int> killStats = new List<int>();
                            killStats.Add(0);
                            killStats.Add(1);
                            killStats.Add(3);
                            player.ShowKillStats(killStats);
                        }
                        else
                        {
                            t.GetComponent<Act1Soldier>().OnBulletShot(player.transform, 100, false);
                        }
                    }
                    else
                    {
                        Soldier s = t.GetComponent<Soldier>();
                        if (s.nationality == Soldier.Nationality.British)
                        {
                            s.OnBulletShot(player.transform, 100, false);
                            List<int> killStats = new List<int>();
                            killStats.Add(0);
                            killStats.Add(1);
                            killStats.Add(3);
                            player.ShowKillStats(killStats);
                        }
                        else
                        {
                            s.OnBulletShot(player.transform, 100, false);
                        }
                    }
                }
            }

            foreach (Transform l in transform)
            {
                if (l != explosionVFX.transform)
                    l.gameObject.SetActive(false);
            }
            if (isLaunchedFromMortar)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }
            if (hasLongImpact)
            {
                Instantiate(smoke, transform);
            }
          //  Invoke("DestroyShakeNoise", 2f);
            Invoke("SelfDestroy", selfDestroyTime);
        }
      
        public float selfDestroyTime = 2f;

        void DestroyShakeNoise()
        {
            Transform playerCam = player.currentWeaponScript.Cam.transform;
            player.transform.GetChild(0).localRotation = Quaternion.Euler(playerCam.localEulerAngles.x, playerCam.localEulerAngles.y, 0f);
        }

        void SelfDestroy()
        {
            if(onlyDeactivate)
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
