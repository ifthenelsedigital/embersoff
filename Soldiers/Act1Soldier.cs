using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
using GunType = IfThenElse.Weapon.GunType;
using System.Collections;

namespace IfThenElse
{
    public class Act1Soldier : MonoBehaviour
    {
        #region Variables
        [Header("Basic Soldier Info")]
        public bool isDead = false;
        public string SoldierName;
        public enum Nationality { British = 0, Japanese = 1 }
        public Nationality nationality;

        public GameObject nearbyPanel;
        public Animator nearbyPanelAnim;

        public Act1Controller controller;
        public string WeaponName;
        public GunType gunType;
        public int maxAmmoPerMag;
        int currentBullets;
        public NavMeshAgent nav;
        public Animator anim;
        [Range(0, 100)]
        [HideInInspector]
        public int Health;

        [Range(0, 100)]
        public int Damage;

        public AudioSource fireSource;
        public AudioSource mouth;
        public AudioClip fireSound;
        public float fireRate;

        public ParticleSystem[] muzzleFlash;


        [Header("General Info")]
        public Transform target;
        int maxHitTimePerMag;
        int SceneDifficulty;
        public AudioSource bloodSFX;
        public List<AudioClip> hurtSounds = new List<AudioClip>();
        public LayerMask shootMask;
        public bool isTargetInRange = false;
        bool isReloading = false;
        bool isAttacking = false;
        bool isTargetDead = false;
        public delegate void OnTargetDead();
        public OnTargetDead onTargetDead;
        public delegate void OnDead();
        public OnDead onDead;
        public GameObject enemyBag;
        public Animator crosshair;
        public Image crosshairCenter;
        public bool isIndicating = false;

        [Header("For Britain Soldiers")]
        public Transform coverPoint;
        public GameObject nameTag;
        public TMP_Text nameText;
        public Transform japaneseSoldierParent;
        public Transform player;

        [HideInInspector]
        public Act1Soldier[] britishSoldiers;

        #endregion

        #region Setup

        void Awake()
        {
            if (nationality == Nationality.British)
            {
                nameTag.SetActive(false);
                nameText.text = SoldierName;
            }
            currentBullets = maxAmmoPerMag;
            fireSource.clip = fireSound;
            if(nationality == Nationality.Japanese)
            {
                SetDamageAccordingToDifficulty();
            }
        }
        void SetDamageAccordingToDifficulty()
        {
            if (SceneDifficulty == 0)
            {
                
                if (gunType == GunType.Sniper)
                {
                    Damage = 50;
                    maxHitTimePerMag = 2;
                }
                else
                {
                    Damage = 6;
                    maxHitTimePerMag = 9;
                }
            }
            else if (SceneDifficulty == 1)
            {
                if (gunType == GunType.Sniper)
                {
                    Damage = 70;
                    maxHitTimePerMag = 2;
                }
                else
                {
                    Damage = 16;
                    maxHitTimePerMag = 5;
                }
            }
            else if (SceneDifficulty == 2)
            {
                if (gunType == GunType.Sniper)
                {
                    Damage = 90;
                    maxHitTimePerMag = 2;
                }
                else
                {
                    Damage = 25;
                    maxHitTimePerMag = 4;
                }
            }
        }


        #endregion

        #region Range Check, Chase & Callbacks
        public void OnTriggerEnter(Collider other)
        {
            bool isJapaneseSoldier = false;
            if(other.CompareTag("Soldier") && other.transform.parent.name.Contains("Japanese")) //then it's a japanese soldier
            {
                isJapaneseSoldier = true;
                if (nationality == Nationality.British && other.GetType() != typeof(SphereCollider))
                {
                    Transform JapSoldier = other.transform;
                    if (isJapaneseSoldier)
                    {
                        JapSoldier.parent.GetComponent<Act1Soldier>().OnBulletShot(transform, 100, false);
                    }
                }

                else
                {
                    if (isJapaneseSoldier && nationality == Nationality.British)
                    {
                        if (other.transform.parent == target && !isTargetDead)
                        {
                            isTargetInRange = true;
                            if (!isAttacking && !isReloading)
                            {
                                nav.isStopped = true;
                                anim.SetBool("IsRunning", false);
                                isRunning = false;
                                anim.SetBool("IsAiming", true);
                                InvokeRepeating(nameof(Attack), 1f, fireRate);
                            }
                        }
                    }
                 

                }
            }

            
        }
        public void OnTriggerExit(Collider other)
        {
            if(nationality == Nationality.Japanese && target != null && !isTargetDead)
            {
                if(target.CompareTag("Player") && other.transform == target)
                {
                    isTargetInRange = false;
                    isReloading = false;
                    CancelInvoke("Attack");
                    isAttacking = false;
                    StartCoroutine(ChaseTarget());
                }
                else if (target.CompareTag("Soldier") && other.transform.parent == target)
                {
                    isTargetInRange = false;
                    isReloading = false;
                    CancelInvoke("Attack");
                    isAttacking = false;
                    StartCoroutine(ChaseTarget());
                }
            }
        }

        public void StartAttacking(Transform ts) //only for the japanese
        {
            target = ts;
            if (target.tag == "Player")
            {
                target.GetComponent<Player>().onPlayerDied += OnTargetDied;
            }
            else
            {
                targetSoldier = target.GetComponent<Act1Soldier>();
                targetSoldier.onDead += OnTargetDied;
            }
            StartCoroutine(ChaseTarget());
        }
        public void OnBaseAttacked() // only for british
        {
            coverPoint.GetComponent<TriggerEnter>().onObjectTrigger += SoldierReachedCoverPoint;
            coverPoint.GetComponent<TriggerEnter>().contact = anim.transform;
            nav.SetDestination(coverPoint.position);
            anim.SetBool("IsRunning", true);
            isRunning = true;
        }
        void SoldierReachedCoverPoint()
        {
            Destroy(coverPoint.gameObject);
            nav.isStopped = true;
            anim.SetBool("IsRunning", false);
            isRunning = false;
            anim.SetBool("IsAiming", true);
            target = japaneseSoldierParent.GetChild(Random.Range(0, japaneseSoldierParent.childCount));
            InvokeRepeating(nameof(Attack), Random.Range(1f, 3f), fireRate);
        }
        public float range;
        public float ViewAngle;
        public LayerMask obstacleMask;
        IEnumerator ChaseTarget() //only the japanese chase the player
        {
            anim.SetBool("IsRunning", true);
            isRunning = true;
            nav.isStopped = false;
            anim.SetBool("IsCrouching", false);
            anim.SetBool("IsAiming", false);
          
            while (!isTargetInRange)
            {
                Vector3 aim = (target.position - transform.position);
             //   transform.rotation = Quaternion.LookRotation(aim);

                if(Vector3.Angle(transform.forward, aim) < ViewAngle/2)
                {
                    float distance = Vector3.Distance(transform.position, target.position);
                    if(distance <= range)
                    {
                        if(Physics.Raycast(transform.forward, aim, distance, obstacleMask, QueryTriggerInteraction.Ignore) == false)
                        {
                            isTargetInRange = true;
                            yield return null;
                        }
                        else
                        {
                            isTargetInRange = false;
                            nav.SetDestination(target.position);
                           

                            yield return null;
                        }
                    }
                    else
                    {
                        nav.SetDestination(target.position);
                        yield return null;
                    }
                }
                else
                {
                    nav.SetDestination(target.position);
                    yield return null;
                }
                yield return null;
            }
           
            nav.isStopped = true;
            hitTime = 0;
            InvokeRepeating("Attack", 1f, fireRate);
            anim.SetBool("IsRunning", false);
            isRunning = false;
            anim.SetBool("IsAiming", true);
            StopCoroutine(ChaseTarget());
        }
        bool isRunning = false;
        void OnTargetDied()
        {
            if(!isDead)
            {
                isTargetDead = true;
                isAttacking = false;
                CancelInvoke("Attack");
                isReloading = false;
                currentBullets = maxAmmoPerMag;
                anim.SetBool("IsRunning", false);
                isRunning = false;
                anim.SetBool("IsCrouching", false);
                anim.SetBool("IsAiming", false);
                nav.isStopped = true;
                isTargetInRange = false;
                if (nationality == Nationality.British)
                {
                    if (target != null)
                    {
                        target.GetComponent<Act1Soldier>().onDead -= OnTargetDied;
                    }
                    target = japaneseSoldierParent.GetChild(Random.Range(0, japaneseSoldierParent.childCount));
                    target.GetComponent<Act1Soldier>().onDead += OnTargetDied; //verifies whether the target japanese soldier has died
                    isAttacking = true;
                    anim.SetBool("IsCrouching", false);
                    anim.SetBool("IsRunning", false);
                    isRunning = false;
                    anim.SetBool("IsAiming", true);
                    isTargetDead = false;
                    InvokeRepeating(nameof(Attack), 1f, fireRate);
                }

                else if (nationality == Nationality.Japanese)// because the britain soldiers cannot die in act1
                {
                    if (target.CompareTag("Player"))
                    {
                        Invoke(nameof(Suicide), 1f);
                    }
                }
            }
        }
        void Suicide()
        {
            Die(false, controller.transform);
        }
        public void CheckForPlayer()
        {
            StartCoroutine(ChaseTarget());
        }
        private void Update()
        {
            if (isAttacking && !isReloading && target != null)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position).normalized, 2f * Time.deltaTime);
            }

            if(isRunning)
            {
                if(nav.velocity.magnitude < 1f)
                {
                    anim.SetBool("IsRunning", false);
                    anim.SetBool("IsAiming", true);
                }
                else
                {
                    anim.SetBool("IsRunning", true);
                    anim.SetBool("IsAiming", false);
                }
            }
        }

        #endregion

        #region Attack and Reload
        Act1Soldier targetSoldier;
        public bool S93;
        public float heightForShot = 1f; 
        int hitTime = 0;
        public Transform mushkil;
        void Attack()
        {
            S93 = true;
            if(currentBullets != 0 && target != null)
            {
                isReloading = false;
                isAttacking = true;
                anim.SetBool("IsCrouching", false);
                anim.SetBool("IsAiming", true);
                anim.SetBool("IsRunning", false);
                isRunning = false;
                anim.SetTrigger("Shoot");

                foreach (ParticleSystem p in muzzleFlash)
                {
                    p.Play();
                }
                fireSource.Play();
                currentBullets -= 1;
                if (hitTime < maxHitTimePerMag)
                {
                    hitTime += 1;
                    if (Physics.Raycast(transform.position, target.position - transform.position, out RaycastHit hit, Mathf.Infinity, shootMask))
                    {
                        Transform hitCollider = hit.collider.transform;
                        if (nationality == Nationality.Japanese)
                        {      
                            if(hitCollider == target)
                            {
                                hit.collider.GetComponent<Player>().TakeDamage(Damage, transform);
                            }
                            else
                            {
                                mushkil = hit.collider.transform;
                            }
                        }
                        else if(nationality == Nationality.British)
                        {
                            if(hitCollider == target)
                            {
                                hitCollider.parent.GetComponent<Act1Soldier>().OnBulletShot(transform, Damage, false);
                            }
                        }

                    }
                   
                }

            }

            else if(currentBullets == 0 && target != null)
            {
                CancelInvoke("Attack");
               
                isAttacking = false;
                Reload();
            }

            else if(target == null)
            {
                if(nationality == Nationality.British)
                {
                    CancelInvoke("Attack");
                    isAttacking = false;
                   
                    if (japaneseSoldierParent.childCount != 0)
                    {
                        ResetTarget(japaneseSoldierParent.GetChild(Random.Range(0, japaneseSoldierParent.childCount)));
                    }
                }
            }
        }
      
        void Reload()
        {
            S93 = false;
            isReloading = true;
            isAttacking = false;
            anim.SetBool("IsAiming", false);
            anim.SetBool("IsCrouching", true);

            currentBullets = maxAmmoPerMag;
            hitTime = 0;
            if(target != null)
            {
                if (isTargetInRange)
                {
                    InvokeRepeating("Attack", Random.Range(3f, 5f), fireRate);
                }
                else
                {
                    if(nationality == Nationality.Japanese)
                    {
                        StartCoroutine(ChaseTarget());
                    }
                }
            }
            else
            {
                if(nationality == Nationality.British)
                {
                    target = japaneseSoldierParent.GetChild(Random.Range(0, japaneseSoldierParent.childCount));
                    targetSoldier = target.GetComponent<Act1Soldier>();
                    InvokeRepeating("Attack", Random.Range(3f, 6f), fireRate);
                }
                else
                {
                    int i = Random.Range(0, 2);
                    if(i == 1)
                    {
                        target = player;
                        isTargetDead = false;
                        isTargetInRange = false;
                        player.GetComponent<Player>().onPlayerDied += OnTargetDied;
                        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
                        if(colliders == null)
                        {
                            StartCoroutine(ChaseTarget());
                        }
                        else
                        {
                            foreach(Collider c in colliders)
                            {
                                if(c.transform == target)
                                {
                                    isTargetInRange = true;
                                    InvokeRepeating("Attack", 1.1f, fireRate);
                                    break;
                                }
                                
                            }
                        }
                    }

                    else
                    {
                        target = japaneseSoldierParent.GetChild(Random.Range(0, japaneseSoldierParent.childCount));
                        isTargetDead = false;
                        isTargetInRange = false;
                        target.GetComponent<Act1Soldier>().onDead += OnTargetDied;
                        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
                        if (colliders == null)
                        {
                            StartCoroutine(ChaseTarget());
                        }
                        else
                        {
                            foreach (Collider c in colliders)
                            {
                                if (c.transform == target)
                                {
                                    isTargetInRange = true;
                                    InvokeRepeating("Attack", 1.1f, fireRate);
                                    break;
                                }

                            }
                        }
                    }
                }
            }
        }
        void ResetTarget(Transform newTarget) //only for britain
        {
            if (target != null)
            {
                if (target.CompareTag("Soldier"))
                {
                    target.GetComponent<Act1Soldier>().onDead -= OnTargetDied;
                }
                else if (target.CompareTag("Player"))
                {
                    target.GetComponent<Player>().onPlayerDied -= OnTargetDied;
                }

                if (target != newTarget)
                {
                    target = newTarget;
                    targetSoldier = target.GetComponent<Act1Soldier>();
                    targetSoldier.onDead += OnTargetDied;
                    isAttacking = false;
                    isTargetDead = false;
                    isReloading = false;
                    hitTime = 0;
                    currentBullets = maxAmmoPerMag;
                    anim.SetBool("IsCrouching", false);
                    anim.SetBool("IsAiming", false);
                    anim.SetBool("IsRunning", false);
                    isRunning = false;
                    InvokeRepeating("Attack", 1f, fireRate);
                }
                else
                {
                    anim.SetBool("IsAiming", true);
                    if (japaneseSoldierParent.childCount != 0)
                    {
                        ResetTarget(japaneseSoldierParent.GetChild(Random.Range(0, japaneseSoldierParent.childCount)));
                    }
                    else
                    {
                        hitTime = 0;
                    }
                }
            }
            else
            {
                target = newTarget;
                targetSoldier = target.GetComponent<Act1Soldier>();
                targetSoldier.onDead += OnTargetDied;
                isAttacking = false;
                isTargetDead = false;
                isReloading = false;
                hitTime = 0;
                currentBullets = maxAmmoPerMag;
                anim.SetBool("IsCrouching", false);
                anim.SetBool("IsAiming", false);
                anim.SetBool("IsRunning", false);
                isRunning = false;
                InvokeRepeating("Attack", 1f, fireRate);
            }
        }
        #endregion

        #region Health

        public void OnBulletShot(Transform from, int damage, bool isHeadshot)
        {
            if(nationality == Nationality.British)
            {
                if(from.tag == "Player")
                {
                    Time.timeScale = 0.5f;
                    Time.fixedDeltaTime = 0.01f;
                    controller.Invoke("TreatPlayerAsTraitor", 0.6f);
                    
                }
            }
           
            else
            {
                if(Health > 0)
                {
                    if (isHeadshot)
                    {
                        Health -= damage * 2;
                        if (Health <= 0)
                        {
                            Die(true, from);
                        }
                    }
                    else
                    {
                        Health -= damage;
                        if (Health <= 0)
                        {
                            Die(false, from);
                        }
                    }
                }
            }
        }
       
        void Die(bool isHeadshot, Transform from)
        {
            isDead = true;
            isAttacking = false;
            isReloading = false;
            mouth.clip = hurtSounds[Random.Range(0, hurtSounds.Count)];
            mouth.Play();
            CancelInvoke(nameof(Attack));
            anim.SetBool("IsRunning", false);
            isRunning = false;
            anim.SetBool("IsHeadshot", isHeadshot);
            anim.SetBool("IsDead", true);
            nav.isStopped = true;
            controller.OnJapaneseSoldierDied(from);
            StopCoroutine(ChaseTarget());
            if(target.CompareTag("Player"))
            {
                target.GetComponent<Player>().onPlayerDied -= OnTargetDied;           
            }
            else
            {
                onDead?.Invoke();

            }
            GameObject bag = Instantiate(enemyBag, transform.position, Quaternion.identity, null);
            CollectablesManager weaponBox = bag.transform.GetChild(0).GetComponent<CollectablesManager>();
            weaponBox.iai = controller.GetComponent<ItemsAndIcons>();
            weaponBox.playerTransform = player;
            weaponBox.isEnemyBag = true;
            weaponBox.player = player.GetComponent<Player>();
            weaponBox.items.Add(WeaponName, 1);
            weaponBox.isReserved = true;
            weaponBox.reservedItemQuantity = 1;
            weaponBox.nearbyPanel = controller.nearbyPanel;
            weaponBox.nbAnim = controller.nearbyPanelAnim;
            weaponBox.reservedItemName = WeaponName;
            weaponBox.isUsedGun = true;
            weaponBox.magAmmoOnGun = Random.Range(Mathf.RoundToInt(maxAmmoPerMag / 2), maxAmmoPerMag);
            weaponBox.items.Add(gunType + " Ammo", 80);
            weaponBox.EnablePickHint();
            Destroy(gameObject, 5);
        }

        #endregion

        #region Specials
        public void PauseFiring()
        {
            CancelInvoke("Attack");
            isAttacking = false;
            isReloading = false;
            target = null;
        }
       

        #endregion

        #region NameDisplay

        public void ShowNameDisplay()
        {
            if (nationality == Nationality.British && !isIndicating)
            {
                nameTag.SetActive(true);
                nameTag.transform.rotation = Quaternion.LookRotation((player.position - transform.position).normalized);
                CancelInvoke("HideNameDisplay");
                Invoke("HideNameDisplay", 3f);
                isIndicating = true;
            }

        }

        public void HideNameDisplay()
        {
            if (nationality == Nationality.British)
            {
                nameTag.SetActive(false);
                isIndicating = false;
            }
        }

        #endregion

    }
}
