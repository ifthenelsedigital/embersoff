using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace IfThenElse
{
    public class UIOptimizer : MonoBehaviour
    {
        [SerializeField] private SpriteAtlas Atlas;
        [SerializeField] private string SpriteName;
        [SerializeField] private Image Display;
        bool isSet = false;
        void Start()
        {
            if(Display == null)
            {
                Display = GetComponent<Image>();
            }
            if(SpriteName != null && !isSet)
            {
                Display.sprite = Atlas.GetSprite(SpriteName);
                isSet = true;
            }
        }
        public void SetSprite(string sName)
        {
            Display.sprite = Atlas.GetSprite(sName);
            isSet = true;
        }
    }
}
