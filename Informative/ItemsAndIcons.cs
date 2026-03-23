using System.Collections.Generic;
using UnityEngine;

namespace IfThenElse
{
    public class ItemsAndIcons : MonoBehaviour
    {
        [Header("Gun Icons")]
        public GameObject leeEnfieldIcon;
        public GameObject thompsonIcon;
        public GameObject winchesterIcon;
        public GameObject m1GarandIcon;
        public GameObject ak74Icon;
        public GameObject m1911Icon;
        public GameObject browningHPIcon;
        public GameObject SLRIcon;
        public GameObject M4CarbineIcon;
        public GameObject m79Icon;
        public GameObject brenIcon;


        [Space(5f)]

        [Header("Other Icons")]
        public Sprite grenadeIcon;
        public Sprite dynamiteIcon;
        public Sprite magIcon;
        public Sprite AxeIcon;
        public Sprite keyIcon;

        public Dictionary<string, GameObject> gunIcons = new Dictionary<string, GameObject>();
        public Dictionary<string, Sprite> otherIcons = new Dictionary<string, Sprite>();
        public Dictionary<string, string> useTypes = new Dictionary<string, string>();
        void Awake()
        {
            gunIcons.Add("LeeEnfield", leeEnfieldIcon);
            gunIcons.Add("Thompson", thompsonIcon);
            gunIcons.Add("M1911", m1911Icon);
            gunIcons.Add("AK74", ak74Icon);
            gunIcons.Add("Browning HP", browningHPIcon);
            gunIcons.Add("Winchester", winchesterIcon);
            gunIcons.Add("Bren", brenIcon);
            gunIcons.Add("M1 Garand", m1GarandIcon);
            gunIcons.Add("M4 Carbine", M4CarbineIcon);
            gunIcons.Add("M79", m79Icon);
            gunIcons.Add("SLR", SLRIcon);
            otherIcons.Add("Grenade", grenadeIcon);
            otherIcons.Add("Mag", magIcon);
            otherIcons.Add("Dynamite", dynamiteIcon);
            otherIcons.Add("Axe", AxeIcon);
            otherIcons.Add("Mortar Key", keyIcon);

            useTypes.Add("LeeEnfield", "P");
            useTypes.Add("M1 Garand", "P");
            useTypes.Add("M4 Carbine", "P");
            useTypes.Add("M79", "P");
            useTypes.Add("Thompson", "P");
            useTypes.Add("Winchester", "P");
            useTypes.Add("SLR", "P");
            useTypes.Add("AK74", "P");
            useTypes.Add("M1911", "S");
            useTypes.Add("Bren", "P");
            useTypes.Add("Browning HP", "S");
        }
      
    }
}
