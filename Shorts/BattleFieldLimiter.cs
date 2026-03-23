using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IfThenElse
{
    public class BattleFieldLimiter : MonoBehaviour
    {
        public bool isEdgeLimiter = true;
        public Player playerScript;
        public string Dialogue = "YOU CANNOT ESCAPE THE BATTLE FIELD!";
        public void OnTriggerEnter(Collider other)
        {
            if (other.transform.tag == "Player")
            {
                if (!playerScript.warningTextGO.GetCurrentAnimatorStateInfo(0).IsName("Display"))
                {                 
                    playerScript.ShowBattleFieldLimitWarning(Dialogue, isEdgeLimiter);                  
                }
            }
        }
        public void OnTriggerExit(Collider other)
        {
            if (other.transform.CompareTag("Player"))
            {
                if (playerScript.warningTextGO.GetCurrentAnimatorStateInfo(0).IsName("Display"))
                {
                    playerScript.warningTextGO.ResetTrigger("Display");
                    playerScript.warningTextGO.Play("Idle");
                }
            }
        }
    }
}