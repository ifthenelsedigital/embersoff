using UnityEngine;
namespace IfThenElse
{
    public class AutoKill : MonoBehaviour
    {
        public bool isPlayer = false;
        public bool isAct1 = false;
        public Transform player;
        public void OnTriggerEnter(Collider other)
        {
            if (isAct1)
            {
                if (other.CompareTag("Soldier") && other.GetType() == typeof(CapsuleCollider) && other.transform.parent.name.Contains("Japanese"))
                {
                    Act1Soldier a = other.transform.parent.GetComponent<Act1Soldier>();
                    if (a.nationality == Act1Soldier.Nationality.Japanese && !Physics.Linecast(player.position, other.transform.position))
                    {
                        if (isPlayer && a.target.CompareTag("Player"))
                        {
                            a.StopCoroutine("ChaseTarget");
                            a.isTargetInRange = true;
                            a.anim.SetBool("IsRunning", false);
                            a.anim.SetBool("IsAiming", true);
                            a.InvokeRepeating("Attack", 1f, a.fireRate);
                        }
                        else
                        {
                            a.OnBulletShot(transform.parent, 100, false);
                        }
                    }
                }
            }
            else
            {
                if (other.CompareTag("Soldier") && other.GetType() == typeof(CapsuleCollider))
                {
                    Soldier s = other.transform.parent.GetComponent<Soldier>();
                    if (s.nationality == Soldier.Nationality.British && s.target != null)
                    {
                        if (isPlayer && s.target.CompareTag("Player") && !s.isTargetInRange && s.fightMode == Soldier.FightMode.Attacking)
                        {
                            s.StopCoroutine("ChaseTarget");
                            s.isTargetInRange = true;
                            s.anim.SetBool("IsRunning", false);
                            s.anim.SetBool("IsAiming", true);
                            s.InvokeRepeating("Attack", 1f, s.fireRate);
                        }
                        else if(!isPlayer)
                        {
                            s.OnBulletShot(transform.parent, 100, false);
                        }
                    }
                }

            }
        }
    }
}
