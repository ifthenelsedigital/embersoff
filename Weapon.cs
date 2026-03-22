using UnityEngine;
using TMPro;
using System.Collections.Generic;
namespace IfThenElse
{
    public class Weapon : MonoBehaviour
    {
        [Header("Main Things")]
        public bool isOfEnemy = false;
        public bool isMelee;
        public bool showMuzzleFlash;
        public Camera Cam;
        public int ActNumber;
   
        [Tooltip("Damage wala X")]
        public Animator damageX;
        public GameObject bulletTrailEffect;
        public enum UseType { Primary = 0, Secondary = 1 }
        public UseType useType;
        public enum GunType {AR = 0, SMG = 1, Sniper = 2, SG = 3, GrenadeLauncher = 4, HG = 5, LMG = 6, RocketLauncher = 7 }
        public GunType Type;
        public bool DebugHitInfo = false;
       
        [Space(5f)]

        [Header("Gun Info")]
        
        public string Name;
        public float playerSpeedWithGun;
        public int maxAmmoPerMag;
        public int currentMagAmmo;
        public int currentAmmo;
        public ParticleSystem[] muzzleFlash;
        public Animator cameraObject;
        public GameObject bullet;
        public Transform bulletSpawn;
        public int Damage;
        [Range(0f, 1f)]
        public float fireRate;
        public bool canFire = false;
        float timeLeftToFire;
        public AudioClip FireSound;
        public TMP_Text PrimaryAmmoText;
        public TMP_Text SecondaryAmmoText;
        AudioSource audioSource;
        public GameObject grenade;
        public GameObject RPGRocket;
        [Header("Bullet Impacts")]
        public GameObject dustyImpact;
        public GameObject groundImpact;
        public GameObject metalImpact;
        public GameObject woodImpact;
        public GameObject bloodImpact;
        ObjectPooler objectPooler;

        [Header("Bullet Hit SFX")]
        public AudioClip metalHit;
        public AudioClip woodHit;
        public AudioClip groundHit;
        public AudioClip dustyHit;
        public AudioSource hitSFXSource;

        public float ADSTime;
        public bool isScoping = false;

        public LayerMask _layerMask;
      
        void Start()
        {
            timeLeftToFire = fireRate;
            objectPooler = ObjectPooler.pooler;
           
            audioSource = GetComponent<AudioSource>();
            Physics.queriesHitBackfaces = true;
     
           
            if (Name != "Grenade")
            {
                audioSource.clip = FireSound;
            }
         
        }
      
        public void Fire()
        {
            if(name != "Grenade")
            {
                audioSource.clip = FireSound;
            }
            if (!isMelee && Type != GunType.GrenadeLauncher && Type != GunType.RocketLauncher)
            {
                if (currentMagAmmo != 0)
                {
                        timeLeftToFire = fireRate;
                        canFire = false;
                        ShowImpact();

                    foreach(ParticleSystem p in muzzleFlash)
                    {
                        if(!p.isPlaying)
                        {
                            p.Play();
                        }
                    }
                        audioSource.Play(); //plays fire sound
                        currentMagAmmo -= 1;
                    if(useType == UseType.Primary)
                    {
                        PrimaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                    }
                    else
                    {
                        SecondaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                    }
                }

                else if (currentAmmo != 0)
                {
                    Reload();
                }
                else
                {
                    PlayEmptyClipSound();
                    transform.parent.GetComponent<Animator>().SetBool("isScoped", false);
                    transform.parent.GetComponent<Animator>().SetBool("isShooting", false);
                }
            }

            if (Type == GunType.GrenadeLauncher && !isMelee)
            {
                if (currentMagAmmo != 0)
                {
                    timeLeftToFire = fireRate;
                    canFire = false;

                    foreach (ParticleSystem p in muzzleFlash)
                    {
                        if (!p.isPlaying)
                        {
                            p.Play();
                        }
                    }
                    cameraObject.SetTrigger("BulletShake");
                  
                        Bomb launchedGrenade = Instantiate(grenade, bulletSpawn.position, Quaternion.identity).GetComponent<Bomb>();
                        launchedGrenade.isLaunched = true;
                    launchedGrenade.ActNumber = ActNumber;
                    launchedGrenade.playerCam = cameraObject;
                    if (cameraObject.transform.localRotation.x < 0f)
                        {
                            launchedGrenade.GetComponent<Rigidbody>().AddForce(transform.forward * 40f, ForceMode.Impulse);
                        }
                        else if (cameraObject.transform.localRotation.x == 0f)
                        {
                            launchedGrenade.GetComponent<Rigidbody>().AddForce(transform.forward * 30f, ForceMode.Impulse);
                        }
                        else
                        {
                            launchedGrenade.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
                        }
                    

                    
                  
                    audioSource.Play(); //plays fire sound
                    currentMagAmmo -= 1;

                    if (useType == UseType.Primary)
                    {
                        PrimaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                    }
                    else
                    {
                        SecondaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                    }

                }

               
                else
                {
                    PlayEmptyClipSound();
                    transform.parent.GetComponent<Animator>().SetBool("isScoped", false);
                    transform.parent.GetComponent<Animator>().SetBool("isShooting", false);
                }
            }
           

            else if(isMelee)
            {
                canFire = false;
                timeLeftToFire = fireRate;
                Invoke("ShowImpact", 0.4f);
            }
        }
     
        public float range;
        void ShowImpact()
        {

            Ray line = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (isMelee)
            {
                if (Physics.Raycast(line, out RaycastHit hit, range, _layerMask, QueryTriggerInteraction.Ignore))
                {
                    Collider hitCollider = hit.collider;
                    if (DebugHitInfo)
                    {
                        Debug.Log(hit.collider.name);
                    }

                    if (hitCollider.tag == "Dusty")
                    {
                        objectPooler.SpawnFromPool("Dust", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = dustyHit;
                        hitSFXSource.Play();
                    }
                    else if (hitCollider.tag == "Ground")
                    {
                        objectPooler.SpawnFromPool("Ground", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = groundHit;
                        hitSFXSource.Play();
                    }
                    else if (hitCollider.tag == "Metal")
                    {
                        objectPooler.SpawnFromPool("Metal", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = metalHit;
                        hitSFXSource.Play();
                    }
                  
                    else if (hitCollider.tag == "Wood")
                    {
                        objectPooler.SpawnFromPool("Wood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = woodHit;
                        hitSFXSource.Play();
                    }
                    else if(hitCollider.tag == "Shootable")
                    {
                        ShootableObject so = hitCollider.GetComponent<ShootableObject>();
                        if(so.material == "Wood")
                        {
                            objectPooler.SpawnFromPool("Wood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                            hitSFXSource.clip = woodHit;
                            hitSFXSource.Play();
                        }
                        so.OnShot();
                    }
                    else if (hitCollider.tag == "Soldier")
                    {
                        objectPooler.SpawnFromPool("Blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        if (ActNumber == 1)
                        {
                            Act1Soldier act1Soldier;
                            if (hitCollider.transform.parent.name.Contains("Japanese"))
                            {
                                act1Soldier = hit.collider.transform.parent.GetComponent<Act1Soldier>();
                            }
                            else
                            {
                                act1Soldier = hit.collider.transform.parent.parent.parent.GetComponent<Act1Soldier>();
                            }

                            if (!act1Soldier.isDead)
                            {
                                if (hitCollider.GetType() == typeof(SphereCollider)) // A headshot
                                {
                                    act1Soldier.OnBulletShot(player.transform, Damage * 2, true);
                                    if (act1Soldier.Health <= 0)
                                    {
                                        damageX.SetTrigger("ShowDeathIndication");
                                        if (Type == GunType.Sniper)
                                        {
                                            System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                            killStats.Add(1);
                                            killStats.Add(0);
                                            killStats.Add(2);
                                            player.ShowKillStats(killStats);
                                        }
                                        else
                                        {
                                            System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                            killStats.Add(2);
                                            killStats.Add(0);
                                            player.ShowKillStats(killStats);
                                        }

                                    }

                                    else
                                    {
                                        damageX.SetTrigger("ShowDamageIndication");
                                    }
                                }

                                else if (hitCollider.GetType() == typeof(CapsuleCollider)) // a body shot
                                {
                                    act1Soldier.OnBulletShot(player.transform, Damage, false);
                                    if (act1Soldier.Health <= 0)
                                    {
                                        damageX.SetTrigger("ShowDeathIndication");
                                        if (Type == GunType.Sniper)
                                        {
                                            System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                            killStats.Add(1);
                                            killStats.Add(0);
                                            player.ShowKillStats(killStats);
                                        }
                                        else
                                        {
                                            System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                            killStats.Add(0);
                                            player.ShowKillStats(killStats);
                                        }
                                    }

                                    else
                                    {
                                        damageX.SetTrigger("ShowDamageIndication");
                                    }
                                }
                            }
                        }

                        else
                        {
                            Soldier soldier = hit.collider.transform.parent.GetComponent<Soldier>();
                            if (!soldier.isDead)
                            {
                                if (hitCollider.GetType() == typeof(SphereCollider)) // A headshot
                                {
                                    soldier.OnBulletShot(player.transform, Damage * 2, true);
                                    if (soldier.Health <= 0)
                                    {
                                        damageX.SetTrigger("ShowDeathIndication");
                                        if (Type == GunType.Sniper)
                                        {
                                            List<int> killStats = new List<int>();
                                            killStats.Add(1);
                                            killStats.Add(0);
                                            killStats.Add(2);
                                            player.ShowKillStats(killStats);
                                        }
                                        else
                                        {
                                            List<int> killStats = new List<int>();
                                            killStats.Add(2);
                                            killStats.Add(0);
                                            player.ShowKillStats(killStats);
                                        }

                                    }

                                    else
                                    {
                                        damageX.SetTrigger("ShowDamageIndication");
                                    }
                                }

                                else if (hitCollider.GetType() == typeof(CapsuleCollider)) // a body shot
                                {
                                    soldier.OnBulletShot(player.transform, Damage, false);
                                    if (soldier.Health <= 0)
                                    {
                                        damageX.SetTrigger("ShowDeathIndication");
                                        if (Type == GunType.Sniper)
                                        {
                                            System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                            killStats.Add(1);
                                            killStats.Add(2);
                                            killStats.Add(0);
                                            player.ShowKillStats(killStats);
                                        }
                                        else
                                        {
                                            System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                            killStats.Add(0);
                                            player.ShowKillStats(killStats);
                                        }
                                    }

                                    else
                                    {
                                        damageX.SetTrigger("ShowDamageIndication");
                                    }
                                }
                            }
                        }

                    }

                }

            }

            else
            {
                if (Physics.Raycast(Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out RaycastHit hit, Mathf.Infinity, _layerMask, QueryTriggerInteraction.Ignore))
                {
                    if (DebugHitInfo)
                    {
                        Debug.Log(hit.collider.name);
                    }
                    bulletTrailEffect.SetActive(true);
                    Invoke("HideBulletTrail", 0.1f);
                    Collider hitCollider = hit.collider;
                        if (hitCollider.tag == "Dusty")
                    {
                        objectPooler.SpawnFromPool("Dust", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = dustyHit;
                        hitSFXSource.Play();
                    }
                    else if (hitCollider.tag == "Ground")
                    {
                       
                       objectPooler.SpawnFromPool("Ground", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = groundHit;
                        hitSFXSource.Play();
                    }
                    else if (hitCollider.tag == "Metal")
                    {
                        objectPooler.SpawnFromPool("Metal", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = metalHit;
                        hitSFXSource.Play();
                    }
                    else if (hitCollider.tag == "Helicopter")
                    {
                        objectPooler.SpawnFromPool("Metal", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = metalHit;
                        hitSFXSource.Play();
                        Plane plane = hitCollider.GetComponent<Plane>();
                        if(plane != null)
                        {
                            plane.BulletHit(Damage);
                        }
                    }
                    else if (hitCollider.tag == "Wood")
                    {
                        objectPooler.SpawnFromPool("Wood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                        hitSFXSource.clip = woodHit;
                        hitSFXSource.Play();
                    }
                    else if (hitCollider.tag == "Shootable")
                    {
                        ShootableObject so = hitCollider.GetComponent<ShootableObject>();
                        so.OnShot();
                        if (so.material == "Wood")
                        {
                            objectPooler.SpawnFromPool("Wood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                            hitSFXSource.clip = woodHit;
                            hitSFXSource.Play();
                        }
                    }
                    else if (hitCollider.CompareTag("Soldier"))
                    {
                        if (hitCollider.GetType() == typeof(CapsuleCollider) || hitCollider.GetType() == typeof(SphereCollider))
                        {
                            {
                                objectPooler.SpawnFromPool("Blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), Vector3.zero);
                                if (ActNumber == 1)
                                {
                                    Act1Soldier act1Soldier;
                                    if (hitCollider.transform.parent.name.Contains("Japanese"))
                                    {
                                        act1Soldier = hit.collider.transform.parent.GetComponent<Act1Soldier>();
                                    }
                                    else
                                    {
                                        act1Soldier = hit.collider.transform.parent.parent.parent.GetComponent<Act1Soldier>();
                                    }

                                    if (!act1Soldier.isDead)
                                    {
                                        if (hitCollider.GetType() == typeof(SphereCollider)) // A headshot
                                        {
                                            act1Soldier.OnBulletShot(player.transform, Damage * 2, true);
                                            if (act1Soldier.Health <= 0)
                                            {
                                                damageX.SetTrigger("ShowDeathIndication");
                                                if (Type == GunType.Sniper)
                                                {
                                                    List<int> killStats = new List<int>();
                                                    killStats.Add(1);
                                                    killStats.Add(0);
                                                    killStats.Add(2);
                                                    player.ShowKillStats(killStats);
                                                }
                                                else
                                                {
                                                    List<int> killStats = new List<int>();
                                                    killStats.Add(2);
                                                    killStats.Add(0);
                                                    player.ShowKillStats(killStats);
                                                }

                                            }

                                            else
                                            {
                                                damageX.SetTrigger("ShowDamageIndication");
                                            }
                                        }

                                        else if (hitCollider.GetType() == typeof(CapsuleCollider)) // a body shot
                                        {
                                            act1Soldier.OnBulletShot(player.transform, Damage, false);
                                            if (act1Soldier.Health <= 0)
                                            {
                                                damageX.SetTrigger("ShowDeathIndication");
                                                if (Type == GunType.Sniper)
                                                {
                                                    System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                                    killStats.Add(1);
                                                    killStats.Add(2);
                                                    killStats.Add(0);
                                                    player.ShowKillStats(killStats);
                                                }
                                                else
                                                {
                                                    System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                                    killStats.Add(0);
                                                    player.ShowKillStats(killStats);
                                                }
                                            }

                                            else
                                            {
                                                damageX.SetTrigger("ShowDamageIndication");
                                            }
                                        }
                                    }
                                }

                                else
                                {
                                    Soldier soldier = hit.collider.transform.parent.GetComponent<Soldier>();
                                    if (!soldier.isDead)
                                    {
                                        if (hitCollider.GetType() == typeof(SphereCollider)) // A headshot
                                        {
                                            soldier.OnBulletShot(player.transform, Damage * 2, true);
                                            if (soldier.Health <= 0)
                                            {
                                                damageX.SetTrigger("ShowDeathIndication");
                                                if (Type == GunType.Sniper)
                                                {
                                                    List<int> killStats = new List<int>();
                                                    killStats.Add(1);
                                                    killStats.Add(0);
                                                    killStats.Add(2);
                                                    player.ShowKillStats(killStats);
                                                }
                                                else
                                                {
                                                    List<int> killStats = new List<int>();
                                                    killStats.Add(2);
                                                    killStats.Add(0);
                                                    player.ShowKillStats(killStats);
                                                }

                                            }

                                            else
                                            {
                                                damageX.SetTrigger("ShowDamageIndication");
                                            }
                                        }

                                        else if (hitCollider.GetType() == typeof(CapsuleCollider)) // a body shot
                                        {
                                            soldier.OnBulletShot(player.transform, Damage, false);
                                            if (soldier.Health <= 0)
                                            {
                                                damageX.SetTrigger("ShowDeathIndication");
                                                if (Type == GunType.Sniper)
                                                {
                                                    System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                                    killStats.Add(1);
                                                    killStats.Add(2);
                                                    killStats.Add(0);
                                                    player.ShowKillStats(killStats);
                                                }
                                                else
                                                {
                                                    System.Collections.Generic.List<int> killStats = new System.Collections.Generic.List<int>();
                                                    killStats.Add(0);
                                                    player.ShowKillStats(killStats);
                                                }
                                            }

                                            else
                                            {
                                                damageX.SetTrigger("ShowDamageIndication");
                                            }
                                        }
                                    }
                                }

                            }
                        }

                    }


                }

            }

            if (!cameraObject.enabled)
                {
                    cameraObject.enabled = true;
                }

            if(-(Cam.transform.localRotation.x) < 0.23f)
            {
                cameraObject.SetTrigger("BulletShake");
            }
            if (!isMelee)
            {
                objectPooler.SpawnFromPool("Bullet", bulletSpawn.position, Random.rotation, transform.TransformDirection(Vector3.right));
            }
                           
        }
        
        void HideBulletTrail()
        {
            bulletTrailEffect.SetActive(false);
        }
        public void PlayEmptyClipSound()
        {
            audioSource.clip = emptyGun;
            audioSource.Play();
            if(Type != GunType.GrenadeLauncher && Type != GunType.RocketLauncher)
            {
                bulletTrailEffect.SetActive(false);
            }
        }
        public void ShowDeathIndication()
        {
            damageX.SetTrigger("ShowDeathIndication");
        }

        public float reloadTime;
        public Player player;
        public AudioClip emptyGun;
        public bool isReloading = false;
        public void Reload()
        {
            if (currentMagAmmo != maxAmmoPerMag && currentAmmo > 0 && !isReloading)
            {
                isReloading = true;
                canFire = false;
                transform.parent.GetComponent<Animator>().SetTrigger("Reload");
                transform.parent.GetComponent<Animator>().SetBool("isShooting", false);
                transform.parent.GetComponent<Animator>().SetBool("isScoped", false);
                Invoke("FillAmmo", reloadTime);
            }
        }
        public TMP_Text warningText;
        public Animator warningTextAnim;
        void FillAmmo()
        {
            player.weaponsAnim.SetBool("isScoped", false);
            int requiredAmmo = maxAmmoPerMag - currentMagAmmo;
            if (currentAmmo >= requiredAmmo)
            {
                currentAmmo -= requiredAmmo;
                currentMagAmmo += requiredAmmo;
                if (useType == UseType.Primary)
                {
                    PrimaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                }
                else
                {
                    SecondaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                }
                canFire = true;
                if (currentAmmo == 0)
                {
                    warningText.text = "<color=red>Warning:</color>Last Mag!";
                    warningTextAnim.GetComponent<Animator>().SetTrigger("Display");
                }
               
                isReloading = false;

            }
            else if (currentAmmo != 0)
            {
                currentMagAmmo += currentAmmo;
                currentAmmo = 0;
                if (useType == UseType.Primary)
                {
                    PrimaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                }
                else
                {
                    SecondaryAmmoText.text = currentMagAmmo + "/" + currentAmmo;
                }
               
                canFire = true;
   
                warningText.text = "<color=red>WARNING:</color> LAST MAG!";
                warningTextAnim.GetComponent<Animator>().SetTrigger("Display");
                isReloading = false;

            }

        }

        void FixedUpdate()
        {
            if (!canFire && !isPaused)
            {
                if (timeLeftToFire > 0)
                {
                    timeLeftToFire -= Time.fixedDeltaTime;
                    canFire = false;
                }
                else
                {
                    canFire = true;
                }
            }
           
              
        }
        public bool isPaused = false;
       
        
    }
}
