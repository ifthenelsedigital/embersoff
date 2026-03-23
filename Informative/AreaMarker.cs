using UnityEngine;

namespace IfThenElse
{
    public class AreaMarker : MonoBehaviour
    {
        public Player player;
        public string markerArea;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
                player.SetCurrentArea(markerArea);
        }
    }
}
