using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

namespace IfThenElse
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Soldier : MonoBehaviour
    {
        #region Setup
        [Header("General Info")]
        public string SoldierName;
        public bool Invincible = false;
        public int AreaSecurityCode;
        public enum Nationality { Indian = 0, British = 1 }
        public Nationality nationality;
        public int Act;
        public enum FightMode { Attacking = 0, Defensive = 1 }
        public FightMode fightMode;


        public Transform target;
        public Transform player;
        public string weaponName;
        public Weapon.GunType weaponType;
        public float range;
        public int maxAmmoPerMag;
        public int Damage;
        public NavMeshAgent nav;
        public Animator anim;
        int currentBullets;

        public Transform britainSoldierParent;
        public Transform indianSoldierParent;

        public AudioSource fireSource;
        public AudioSource mouth;
        public AudioClip fireSound;
        public float fireRate;
        [Range(0, 100)]
        public int Health;
        public delegate void OnReachMeetupPoint();
        public OnReachMeetupPoint onReachMP;

        public ParticleSystem[] muzzleFlash;
        int SceneDifficulty;
        public AudioSource bloodSFX;
        public List<AudioClip> hurtSounds = new List<AudioClip>();
        public LayerMask shootMask;
        public bool isTargetInRange = false;
        bool isReloading = false;
        [HideInInspector]
        public bool isAttacking = false;
        public bool isDead;
        public delegate void OnTargetDead();
        public OnTargetDead onTargetDead;
        public delegate void OnDead();
        public OnDead onDead;
        public GameObject enemyBag;
        public Animator crosshair;
        public Image crosshairCenter;
        public bool isIndicating = false;

        public TriggerEnter coverPoint;
        public TriggerEnter coverPoint2;
        public TriggerEnter[] meetupPoints;

        [Header("Nametag")]
        public GameObject nameTag;
        public TMP_Text nameText;

        private void Start()
        {
            if (nationality == Nationality.Indian)
            {
                nameText.text = SoldierName;
            }
            objectPooler = ObjectPooler.pooler;
            SetDamageAccordingToDifficulty();
            Physics.reuseCollisionCallbacks = true;
            currentBullets = maxAmmoPerMag;
            fireSource.clip = fireSound;
          
        }
        int maxHitTimePerMag;
        void SetDamageAccordingToDifficulty()
        {
            SceneDifficulty = PlayerPrefs.GetInt("Difficulty");
            if (SceneDifficulty == 0)
            {
                if (weaponType == Weapon.GunType.Sniper)
                {
                    Damage = 40;
                    maxHitTimePerMag = 2;

                }
                else
                {
                    Damage = 6;
                    maxHitTimePerMag = 12;
                    fireRate = Mathf.Round(Random.Range(0.04f, 0.13f) * 10f) / 10f;
                }
            }
            else if (SceneDifficulty == 1)
            {
                if (weaponType == Weapon.GunType.Sniper)
                {
                    Damage = 50;
                    maxHitTimePerMag = 2;
                }
                else
                {
                    Damage = 10;
                    maxHitTimePerMag = 11;
                    fireRate = Mathf.Round(Random.Range(0.03f, 0.12f) * 10f) / 10f;
                }
            }
            else if (SceneDifficulty == 2)
            {
                if (weaponType == Weapon.GunType.Sniper)
                {
                    Damage = 80;
                    maxHitTimePerMag = 2;
                }
                else
                {
                    Damage = 25;
                    maxHitTimePerMag = 18;
                    fireRate = Mathf.Round(Random.Range(0.03f, 0.10f) * 10f) / 10f;
                }
            }
        }

        #endregion

        #region Attacking Mode
        public void InitTarget(Transform desiredTarget)
        {
            if (desiredTarget == null)
            {
                desiredTarget = britainSoldierParent.GetChild(Random.Range(0, britainSoldierParent.childCount));
            }
            target = desiredTarget;
            CancelInvoke(nameof(Attack));
            isTargetDead = false;
            isTargetInRange = false;
            isAttacking = false;
            isReloading = false;
            currentBullets = maxAmmoPerMag;
            if (target.CompareTag("Player"))
            {
                playerScript = target.GetComponent<Player>();
                playerScript.onPlayerDied += OnTargetDied;
            }
            else
            {
                Soldier s = target.GetComponent<Soldier>();
                s.onDead += OnTargetDied;
                engagedSoldier = s;
            }
            
                StartCoroutine(ChaseTarget());
            
          

        }
        public Transform gaddar;
        public bool isInLoop = false;
        public List<int> methods = new();
        public float ViewAngle = 90;
        public LayerMask obstacleMask;
        bool isRunning;
        public Transform smart;
        public IEnumerator ChaseTarget()  
        {
            methods.Add(3);
            anim.SetBool("IsRunning", true);
            isRunning = true;
            nav.isStopped = false;
            anim.SetBool("IsCrouching", false);
            anim.SetBool("IsAiming", false);

            while (!isTargetInRange)
            {
                Vector3 aim = (target.position - transform.position);

                if (Vector3.Angle(transform.forward, aim) < ViewAngle / 2)
                {
                    float distance = Vector3.Distance(transform.position, target.position);
                    if (distance <= range)
                    {
                        if (Physics.Raycast(transform.position, aim, distance, obstacleMask, QueryTriggerInteraction.Ignore) == false)
                        {
                            isTargetInRange = true;
                            methods.Add(99);
                          
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
        public void OnCollisionEnter(Collision collision)
        {
            if(target != null)
            {
                if (collision.transform == target)
                {
                    if (nationality == Nationality.British)
                    {
                        Health = 0;
                        methods.Add(10);
                        isAttacking = false;
                        isReloading = false;
                        CancelInvoke("Attack");
                        StopCoroutine(ChaseTarget());
                        anim.SetBool("IsAiming", false);
                        anim.SetBool("IsRunning", false);
                        Die(false, target);
                    }
                }
            }    
        }

        #endregion

        #region Defensive Mode

        public bool isReadyToDefend = false;
        [HideInInspector]
        public Transform lookAtTarget;
        public void ReachSpot(TriggerEnter spot, bool isCoverPoint)
        {
            CancelInvoke(nameof(Attack));
            methods.Add(0);
            if (isCoverPoint)
            {
                spot.onObjectTrigger += SoldierReachedCoverPoint;
            }
            else
            {
                spot.onObjectTrigger += SoldierReachedMeetupPoint;
            }
            spot.contact = anim.transform;
            nav.SetDestination(spot.transform.position);
            nav.updatePosition = true;
            nav.isStopped = false;
            anim.SetBool("IsCrouching", false);
            anim.SetBool("IsAiming", false);
            anim.SetBool("IsRunning", true);

            if(lookAtTarget != null)
            {
                transform.LookAt(lookAtTarget);
            }
        }
        public delegate void OnReachCoverPoint();
        public OnReachCoverPoint onReachCoverPoint;

        public bool path = false;
        void SoldierReachedCoverPoint()
        {
            methods.Add(1);
            if(onReachCoverPoint == null)
            {
                methods.Add(5);
                nav.isStopped = true;
                anim.SetBool("IsRunning", false);
                isReadyToDefend = true;
                if (target != null)
                {
                    isAttacking = true;
                    anim.SetBool("IsCrouching", false);
                    anim.SetBool("IsAiming", true);
                    InvokeRepeating("Attack", 1.1f, fireRate);
                }
                else
                {
                    if(nationality == Nationality.Indian)
                    {
                        target = britainSoldierParent.GetChild(Random.Range(0, britainSoldierParent.childCount));
                        isAttacking = true;
                        anim.SetBool("IsCrouching", false);
                        anim.SetBool("IsAiming", true);
                        InvokeRepeating("Attack", 1.1f, fireRate);
                    }
                   
                }

                if (nationality == Nationality.Indian)
                {
                    autoKiller.SetActive(true);
                }
            }
            else
            {
                methods.Add(4);
                anim.SetBool("IsRunning", false);
                coverPoint.onObjectTrigger -= SoldierReachedCoverPoint; //because of act6
                onReachCoverPoint();
            }
        }
        public void DisableSelf()
        {
            gameObject.SetActive(false);
            this.enabled = false;
        }
        void SoldierReachedMeetupPoint()
        {
            methods.Add(2);
            nav.isStopped = true;
            anim.SetBool("IsRunning", false);
            onReachMP?.Invoke();
        }
        public void SeekRevenge(Transform pl)
        {
            isReloading = false;
            isTargetDead = false;
            isTargetInRange = false;
            target = pl;
            CancelInvoke(nameof(Attack));
            currentBullets = maxAmmoPerMag;
            InvokeRepeating(nameof(Attack), 1f, fireRate);
        }
        #endregion]

        #region Commons
        Soldier engagedSoldier;
        Player playerScript;
        bool isTargetDead;
        public AudioSource hitSFXSource;
        public void OnTargetDied()
        {
            if (!isDead)
            {
                isTargetDead = true;
                methods.Add(18);
                if (fightMode == FightMode.Attacking)
                {
                    StopCoroutine(ChaseTarget());
                    
                }
                else
                {
                    CancelInvoke(nameof(Attack));
                    isAttacking = false;
                    isReloading = false;
                    anim.SetBool("IsShooting", false);
                    anim.SetBool("IsAiming", false);
                    hitTime = 0;

                    currentBullets = maxAmmoPerMag;
                    //check if any target is already in range
                    Collider[] colliders = Physics.OverlapSphere(transform.position, range);
                    if (colliders != null)
                    {
                        foreach (Collider c in colliders)
                        {
                            if (c.CompareTag("Soldier") && c.GetType() == typeof(CapsuleCollider))
                            {
                                Soldier s = c.GetComponent<Soldier>();
                                if (s.nationality != nationality)
                                {
                                    target = c.transform;
                                    engagedSoldier = s;
                                    engagedSoldier.onDead += OnTargetDied;
                                    
                                    isTargetInRange = true;
                                    anim.SetBool("IsCrouching", false);
                                    anim.SetBool("IsAiming", true);
                                    isAttacking = true;
                                    InvokeRepeating(nameof(Attack), 1.1f, fireRate);
                                    break;
                                }

                            }
                            else if (c.CompareTag("Player") && c.GetType() == typeof(CapsuleCollider))
                            {
                                if (nationality == Nationality.British)
                                {
                                    target = c.transform;
                                    
                                    isTargetInRange = true;
                                    playerScript = target.GetComponent<Player>();
                                    playerScript.onPlayerDied += OnTargetDied;
                                    anim.SetBool("IsCrouching", false);
                                    anim.SetBool("IsAiming", true);
                                    isAttacking = true;
                                    InvokeRepeating(nameof(Attack), 1.1f, fireRate);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        isTargetInRange = false;
                        if(nationality == Nationality.Indian)
                        {
                            if(britainSoldierParent.childCount != 0)
                            {
                                target = britainSoldierParent.GetChild(Random.Range(0, britainSoldierParent.childCount));
                                engagedSoldier = target.GetComponent<Soldier>();
                                anim.SetBool("IsCrouching", false);
                                anim.SetBool("IsAiming", true);
                            }
                        }
                    }
                }
            }

        }
        private void Update()
        {
            
            if (isAttacking && !isReloading && target != null)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position).normalized, 2f * Time.deltaTime);
            }
            if (isIndicating)
            {
                nameTag.transform.rotation = Quaternion.LookRotation((player.position - transform.position).normalized);
            }

            if(isReadyToDefend && target != null && !isTargetInRange)
            {
                Vector3 aim = (target.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position).normalized, 2f * Time.deltaTime);

                if (Vector3.Angle(transform.forward, aim) < ViewAngle / 2)
                {
                    float distance = Vector3.Distance(transform.position, target.position);
                    if (distance <= range)
                    {
                        if (Physics.Raycast(transform.forward, aim, distance, obstacleMask, QueryTriggerInteraction.Ignore) == false)
                        {
                            isTargetInRange = true;
                            anim.SetBool("IsCrouching", false);
                            anim.SetBool("IsAiming", true);
                            InvokeRepeating(nameof(Attack), 1.2f, fireRate);
                        }
                       
                    }
                   
                }
              
            }
        }
     

        int hitTime;
        public float heightForShot = 1f;
        ObjectPooler objectPooler;
        public GameObject autoKiller;
        [Header("Impacts")]
        public AudioClip metalHit;
        public AudioClip woodHit;
        public AudioClip groundHit;
        public AudioClip dustyHit;
        public void Attack()
        {
            if (currentBullets != 0 && target != null && !isDead)
            {
                isReloading = false;
                isAttacking = true;
                anim.SetBool("IsCrouching", false);
                anim.SetBool("IsAiming", true);
                anim.SetBool("IsRunning", false);
                anim.SetTrigger("Shoot");
                currentBullets -= 1;
                if (muzzleFlash != null)
                {
                    foreach (ParticleSystem p in muzzleFlash)
                    {
                        p.Play();
                    }
                }
                fireSource.Play();
                if (hitTime < maxHitTimePerMag)
                {
                    hitTime += 1;
                    if (Physics.Linecast(new Vector3(transform.position.x, transform.position.y + heightForShot, transform.position.z), target.position, out RaycastHit hit, shootMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform == target && hit.collider.GetType() == typeof(CapsuleCollider))
                        {
                            if (target.tag == "Player")
                            {
                                if (playerScript == null)
                                {
                                    playerScript = target.GetComponent<Player>();
                                }
                                playerScript.TakeDamage(Damage, transform);
                            }
                            else
                            {
                                if (engagedSoldier == null)
                                {
                                    engagedSoldier = target.GetComponent<Soldier>();
                                }
                                engagedSoldier.OnBulletShot(transform, Damage, false);
                            }
                        }

                        else
                        {
                            gaddar = hit.transform;
                            hitSFXSource.transform.position = hit.point;
                            if (hit.transform.tag == "Dusty")
                            {
                                objectPooler.SpawnFromPool("Dust", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                                hitSFXSource.clip = dustyHit;
                                hitSFXSource.volume = 1;
                                hitSFXSource.Play();

                            }
                            else if (hit.transform.tag == "Ground")
                            {
                                objectPooler.SpawnFromPool("Ground", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                                hitSFXSource.clip = groundHit;
                                hitSFXSource.volume = 1;
                                hitSFXSource.Play();
                            }
                            else if (hit.transform.tag == "Metal")
                            {
                                objectPooler.SpawnFromPool("Metal", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                                hitSFXSource.clip = metalHit;
                                hitSFXSource.volume = 0.02f;
                                hitSFXSource.Play();
                            }
                            else if (hit.transform.tag == "Wood")
                            {
                                objectPooler.SpawnFromPool("Wood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                                hitSFXSource.clip = woodHit;
                                hitSFXSource.volume = 1;
                                hitSFXSource.Play();
                            }
                        }
                    }
                }
            }

            else if (target != null && currentBullets == 0)
            {
                CancelInvoke("Attack");
                isAttacking = false;
                methods.Add(17);
                Reload();
            }
            else if (target == null)
            {
                CancelInvoke("Attack");
                isAttacking = false;
                
            }
        }

        void Reload()
        {
            methods.Add(6);
            isReloading = true;
            isAttacking = false;
            anim.SetBool("IsAiming", false);
            anim.SetBool("IsCrouching", true);

            currentBullets = maxAmmoPerMag;
            hitTime = 0;

            if (target != null)
            {
                if (fightMode == FightMode.Defensive)
                {
                    if (isTargetInRange)
                    {
                        InvokeRepeating(nameof(Attack), Random.Range(3f, 6f), fireRate);
                    }
                }
                else
                {
                    if (isTargetInRange)
                    {
                        InvokeRepeating(nameof(Attack), Random.Range(3f, 6f), fireRate);
                    }
                    else
                    {
                        StartCoroutine(ChaseTarget());
                    }
                }
            }

            else
            {
                
            }

        }


        #endregion

        #region Health
        [HideInInspector]
        public Act2Controller a2Controller;
        [HideInInspector]
        public Act3Controller a3Controller;
        [HideInInspector]
        public Act4Controller a4Controller;
        [HideInInspector]
        public Act5Controller a5Controller;
        [HideInInspector]
        public Act6Controller a6Controller;
        public void OnBulletShot(Transform from, int damage, bool isHeadshot)
        {
            if (nationality == Nationality.Indian)
            {
                if (from.CompareTag("Player"))
                {

                    if (Act == 2)
                    {
                        a2Controller.Invoke("TreatPlayerAsTraitor", 0.6f);
                    }
                    else if (Act == 3)
                    {
                        a3Controller.Invoke("TreatPlayerAsTraitor", 0.6f);
                    }
                    else if(Act == 4)
                    {
                        a4Controller.Invoke("TreatPlayerAsTraitor", 0.6f);
                    }
                    else if (Act == 5)
                    {
                        a5Controller.Invoke("TreatPlayerAsTraitor", 0.6f);
                    }
                    else if (Act == 6)
                    {
                        a6Controller.Invoke("TreatPlayerAsTraitor", 0.6f);
                    }
                }
                else if (!Invincible)
                {
                    Health -= damage;
                    if (Health <= 0)
                    {
                        Die(false, from);
                    }
                }
            }
            else
            {
                if (Health > 0)
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
        [HideInInspector]
        public ItemsAndIcons IAI;
        public bool LeaveEnemyBag = true;
        void Die(bool isHeadshot, Transform from)
        {
            if (autoKiller != null)
            {
                Destroy(autoKiller);
            }
            isDead = true;
            isAttacking = false;
            isReloading = false;
            mouth.clip = hurtSounds[Random.Range(0, hurtSounds.Count)];
            mouth.Play();
            CancelInvoke(nameof(Attack));
            StopCoroutine(ChaseTarget());
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsHeadshot", isHeadshot);
            anim.SetBool("IsDead", true);
            nav.isStopped = true;


            #region Enemy Bag
           
            CollectablesManager weaponBox = Instantiate(enemyBag, new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), Quaternion.identity, null).transform.GetChild(0).GetComponent<CollectablesManager>();

            weaponBox.iai = IAI;
            if(!LeaveEnemyBag)
            {
                weaponBox.gameObject.SetActive(false); //jugaad hehe
            }

            weaponBox.reservedItemQuantity = 1;
            weaponBox.items.Add(weaponName, 1);
            weaponBox.reservedItemName = weaponName;
            weaponBox.playerTransform = player;
            weaponBox.isUsedGun = true;
            weaponBox.EnablePickHint();
            weaponBox.magAmmoOnGun = Random.Range(Mathf.RoundToInt(maxAmmoPerMag / 2), maxAmmoPerMag);
            weaponBox.items.Add(weaponType + " Ammo", 80);

            #endregion
            if (Act == 2)
            {
                a2Controller.SoldierDied(this, nationality, from, AreaSecurityCode);
                weaponBox.nearbyPanel = a2Controller.nearbyPanel;
                weaponBox.nbAnim = a2Controller.nearbyPanelAnim;
            }
            else if (Act == 3)
            {
           
                a3Controller.SoldierDied(this, nationality, from);
                weaponBox.nearbyPanel = a3Controller.nearbyPanel;
                weaponBox.nbAnim = a3Controller.nearbyPanelAnim;
            }
            else if(Act == 4)
            {
                a4Controller.SoldierDied(this, nationality, from);
                weaponBox.nearbyPanel = a4Controller.nearbyPanel;
                weaponBox.nbAnim = a4Controller.nearbyPanelAnim;
            }
            else if (Act == 5)
            {
                a5Controller.SoldierDied(this, nationality, from, AreaSecurityCode);
                weaponBox.nearbyPanel = a5Controller.nearbyPanel;
                weaponBox.nbAnim = a5Controller.nearbyPanelAnim;
            }
            else if (Act == 6)
            {
                a6Controller.SoldierDied(this, nationality, from, AreaSecurityCode);
                weaponBox.nearbyPanel = a6Controller.nearbyPanel;
                weaponBox.nbAnim = a6Controller.nearbyPanelAnim;

            }
            if (target != null)
            {
                if (target.CompareTag("Player"))
                {
                    target.GetComponent<Player>().onPlayerDied -= OnTargetDied;
                }
                else
                {
                    onDead?.Invoke();
                    target.GetComponent<Soldier>().onDead -= OnTargetDied;
                }
            }

         

            if (from.CompareTag("Player") && nationality == Nationality.British)
            {
                //Seek revenge
                Collider[] colliders = Physics.OverlapSphere(transform.position, 8f);
                if (colliders != null)
                {
                    foreach (Collider c in colliders)
                    {
                        if (c.GetType() == typeof(CapsuleCollider) && c.CompareTag("Soldier"))
                        {
                            Soldier s = c.transform.parent.GetComponent<Soldier>();
                            if (s.nationality == Nationality.British)
                            {
                                s.SeekRevenge(from);
                            }
                        }
                    }
                }
            }
            Destroy(gameObject, 5f);
        }

        #endregion

        #region NameDisplay

        public void ShowNameDisplay()
        {
            if (nationality == Nationality.Indian && !isIndicating)
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
            if (nationality == Nationality.Indian)
            {
                nameTag.SetActive(false);
                isIndicating = false;
            }
        }

        #endregion
    }
}
