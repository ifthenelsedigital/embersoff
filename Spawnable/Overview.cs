using UnityEngine;

namespace IfThenElse
{
    public class Overview : MonoBehaviour
    {
        public string ItemName;
        public string ItemType;
        public int value;
        public int magAmmo;
        public Animator nbpAnim;
        public bool IsUsedGun;
        public Player player;
        public TMPro.TMP_Text KeyText;
        int initialIndex = 0;

        private void Start()
        {
            initialIndex = transform.GetSiblingIndex();
            if(initialIndex == 0)
            {
                KeyText.text = "E";
            }
            else if (initialIndex == 1)
            {
                KeyText.text = "F";
            }
        }

        public void ItemClicked()
        {
            if(player != null)
            {
                player = GameObject.Find("Player").GetComponent<Player>();
            }
            player.PickUpObject(ItemName, value, ItemType, IsUsedGun, magAmmo);
            if (player.currentWeaponBox != null)
            {
                player.currentWeaponBox.GetComponent<CollectablesManager>().RemoveItem(ItemName);
                Destroy(gameObject);
            }
            
        }
        private void Update()
        {
            if(transform.GetSiblingIndex() != initialIndex)
            {
                if(transform.GetSiblingIndex() == 0)
                {
                    KeyText.text = "E";
                }
                else if(transform.GetSiblingIndex() == 1)
                {
                    KeyText.text = "F";
                }
            }
        }

    }
}
