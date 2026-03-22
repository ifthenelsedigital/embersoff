using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IfThenElse
{
    public class CollectablesManager : MonoBehaviour
    {
        public bool isEnemyBag = false;
        public Animator nbAnim;
        public Dictionary<string, int> items = new();
        public List<string> availableGuns = new();
        public bool isReserved;
        public string reservedItemName;
        public int reservedItemQuantity;
        bool isItemSet = false;
        public GameObject nearbyPanel;
        public bool isItemVisible = false;
        public int magAmmoOnGun;
        public bool isUsedGun;
        public GameObject toHideGO;
        public Player player;
        public Transform playerTransform;

        void Awake()
        {
            nearbyPanel = GameObject.Find("NearbyPanel");
            if (!isEnemyBag)
            {
                if (!isReserved)
                {
                    SetFirstItem();
                }
                else
                {
                    items.Add(reservedItemName, reservedItemQuantity);
                }
            }
            else
            {
                Invoke(nameof(DestroySelf), 10f);
            }

            
        }
        public AlwaysLookAt pickHint;
    
        public void EnablePickHint()
        {
            pickHint.focus = playerTransform;
            pickHint.enabled = true;
        }
        void DestroySelf()
        {
            if(!isEngaged)
            {
                Destroy(gameObject);
            }
        }
        void SetFirstItem()
        {
            if(!isItemSet)
            {
                int randomOne = Random.Range(1, 5);

                if (randomOne == 1)
                {
                    int randomAmmo = Random.Range(1, 7);
                    if (randomAmmo == 1)
                    {
                        items.Add("Sniper Ammo", 30);
                    }
                    else if (randomAmmo == 2)
                    {
                        items.Add("AR Ammo", 120);
                    }
                    else if (randomAmmo == 3)
                    {
                        items.Add("SMG Ammo", 150);
                    }
                    else if (randomAmmo == 4)
                    {
                        items.Add("SG Ammo", 150);
                    }
                    else if(randomAmmo == 5)
                    {
                        items.Add("HG Ammo", 100);
                    }
                    else if (randomAmmo == 5)
                    {
                        items.Add("LMG Ammo", 180);
                    }
                }

              
                else if (randomOne == 2)
                {
                    items.Add("Grenade", Random.Range(1, 3));
                }
                else if (randomOne == 3)
                {
                    items.Add("Axe", 1);
                }
                else if (randomOne == 4)
                {
                    items.Add(availableGuns[Random.Range(0, availableGuns.Count)], 1);
                    isUsedGun = true;
                }
                isItemSet = true;
            }
          
        }

        public GameObject gunOverview;
        public GameObject itemOverview;
        public ItemsAndIcons iai;
        bool isEngaged = false;
        public void RemoveItem(string name)
        {
            items.Remove(name);

            if (toHideGO != null)
            {
                GetComponent<AudioSource>().Play();
                toHideGO.SetActive(false);
            }
            Invoke(nameof(CheckForEmptiness), 1.1f);
           
        }
        void CheckForEmptiness()
        {
            if (items.Count == 0 && isEnemyBag)
            {
                Destroy(gameObject);
                nbAnim.SetTrigger("Close");
                isEngaged = false;
                Transform container = nearbyPanel.transform.GetChild(0).GetChild(2);
                foreach (Transform f in container)
                {
                    Destroy(f.gameObject);
                }
            }
        }
        public bool isGunAvailable;
        public void OnTriggerEnter(Collider other)
        {
            if(!isEngaged)
            {
                if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider))
                {
                    isEngaged = true;
                    CancelInvoke(nameof(DestroySelf));
                    if (nearbyPanel != null)
                    {
                        nearbyPanel = GameObject.Find("NearbyPanel");
                    }
                    if (nbAnim == null)
                    {
                        nbAnim = nearbyPanel.GetComponent<Animator>();
                    }
                    nbAnim.ResetTrigger("Close");
                    nbAnim.SetTrigger("Open");
                    if (player == null)
                    {
                        player = GameObject.Find("Player").GetComponent<Player>();
                    }
                    player.currentWeaponBox = gameObject;

                    Transform container = nearbyPanel.transform.GetChild(0).GetChild(2);

                    foreach (string item in items.Keys)
                    {
                        if (availableGuns.Contains(item))
                        {
                            GameObject spawnedItem = Instantiate(gunOverview, container);
                            isGunAvailable = true;
                            spawnedItem.transform.GetChild(0).GetComponent<TMP_Text>().text = item;
                            spawnedItem.transform.GetChild(1).GetComponent<Image>().sprite = iai.gunIcons[item].GetComponent<Image>().sprite;
                            Overview overview = spawnedItem.GetComponent<Overview>();
                            overview.player = player;
                            overview.ItemName = item;
                            overview.ItemType = "Gun";
                            overview.nbpAnim = nbAnim;
                            overview.IsUsedGun = isUsedGun;
                            overview.magAmmo = magAmmoOnGun;
                            overview.value = 1;
                        }
                        else if (item != "Sniper Ammo" && item != "AR Ammo" && item != "SMG Ammo" && item != "HG Ammo" && item != "SG Ammo" && item != "LMG Ammo")
                        {
                            GameObject spawnedItem = Instantiate(itemOverview, container);
                            spawnedItem.transform.GetChild(0).GetComponent<TMP_Text>().text = item;
                            spawnedItem.transform.GetChild(1).GetComponent<Image>().sprite = iai.otherIcons[item];
                            Overview overview = spawnedItem.GetComponent<Overview>();
                            overview.player = player;
                            overview.value = items[item];
                            overview.nbpAnim = nbAnim;
                            overview.ItemName = item;
                            overview.ItemType = "Others";
                        }
                        else
                        {
                            GameObject spawnedItem = Instantiate(itemOverview, container);
                            spawnedItem.transform.GetChild(0).GetComponent<TMP_Text>().text = items[item] + " " + item;
                            spawnedItem.transform.GetChild(1).GetComponent<Image>().sprite = iai.otherIcons["Mag"];
                            Overview overview = spawnedItem.GetComponent<Overview>();
                            overview.player = player;
                            overview.value = items[item];
                            overview.nbpAnim = nbAnim;
                            overview.ItemName = item;
                            overview.ItemType = "Ammo";
                        }
                    }


                }

            }
        }
        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider))
            {
                isEngaged = false;
                nbAnim.ResetTrigger("Open");
                nbAnim.SetTrigger("Close");
                foreach (Transform c in nearbyPanel.transform.GetChild(0).GetChild(2))
                {
                    Destroy(c.gameObject);
                }

                if(isEnemyBag)
                {
                    Invoke(nameof(DestroySelf), 10f);
                }
            }
        }
        public void AddPlayerGun(string gunName, int magAmmo)
        {
            Transform container = nearbyPanel.transform.GetChild(0).GetChild(2);
            GameObject spawnedItem = Instantiate(gunOverview, container);
            spawnedItem.transform.GetChild(0).GetComponent<TMP_Text>().text = gunName;
            spawnedItem.transform.GetChild(1).GetComponent<Image>().sprite = iai.gunIcons[gunName].GetComponent<Image>().sprite;
            Overview overview = spawnedItem.GetComponent<Overview>();
            overview.player = player;
            overview.ItemName = gunName;
            overview.ItemType = "Gun";
            overview.nbpAnim = nbAnim;
            overview.IsUsedGun = true;
            overview.magAmmo = magAmmo;
            overview.value = 1;

          
        }

    }
}
