using UnityEngine;
using UnityEngine.AI;

public class NavMeshStopCheck : MonoBehaviour
{
    public NavMeshAgent agent;
    public delegate void OnStopped();
    public OnStopped onStopped;
    public float dist;
    void Update()
    {
        dist = agent.remainingDistance;
        if (agent.remainingDistance < 0.1f)
        {
            onStopped();
        }
        
    }
}
