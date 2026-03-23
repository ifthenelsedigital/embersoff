using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IfThenElse
{
    public class PathTracer : MonoBehaviour
    {
        public Transform[] pathPoints;
        public int pathIndex = 0;
        public NavMeshAgent nav;
        public float distance;

        public GameObject waterSplashes;

        public float minSpeed = 10; //angular
        public float maxSpeed = 80; //angular

        public bool isPlayerBoat;
        public float deccelerationRate = 0.02f;

        public bool IsDetonated = false;
        public delegate void OnReachDetonateSpot();
        public OnReachDetonateSpot onReachDetonateSpot;
        public AudioSource engineSound;
        public bool Reached = false;
        private void Start()
        {
            nav.SetDestination(pathPoints[pathIndex].position);
            if(engineSound != null)
            {
                StartCoroutine(FadeInAudio());
            }
            waterSplashes.SetActive(false);
            Invoke(nameof(EnableWaterSplashes), isPlayerBoat ? 0 : Random.Range(1, 3));
        }
        void EnableWaterSplashes()
        {
            waterSplashes.SetActive(true);
        }
        IEnumerator FadeInAudio()
        {
            while(engineSound.pitch < 2)
            {
                engineSound.pitch += 0.01f;
                yield return null;
            }
            StopCoroutine(FadeInAudio());
        }
        public delegate void OnReachEndPoint();
        public OnReachEndPoint onReachEndPoint;
        IEnumerator FadeOutAudio()
        {
            while (engineSound.pitch > 0)
            {
                engineSound.pitch -= 0.001f;
                yield return null;
            }
            StopCoroutine(FadeOutAudio());
        }
        private void Update()
        {
            distance = Vector3.Distance(transform.position, pathPoints[pathIndex].position);
            if(distance <= 1 && pathIndex < pathPoints.Length - 1)
            {
                if(pathIndex == 15)
                {
                    if(!IsDetonated)
                    {
                        onReachDetonateSpot?.Invoke();
                    }
                    pathIndex++;
                }
                else if(pathIndex == 20)
                {
                    if(IsDetonated)
                    {
                        pathIndex++;
                    }
                    else
                    {
                        pathIndex = 0;  //repeat the round
                    }
                }
              
                else
                {
                    pathIndex++;
                }

                nav.SetDestination(pathPoints[pathIndex].position);
            }

            
        }
        bool isFadingOut = false;
        private void FixedUpdate()
        {
            if (pathIndex == pathPoints.Length - 1)
            {
                if(!isFadingOut)
                {
                    if(engineSound != null)
                    {
                        StartCoroutine(FadeOutAudio());
                        isFadingOut = true;
                    }
               
                }
                if (nav.velocity.magnitude > 0)
                {
                    nav.speed -= deccelerationRate;
                }
                else
                {
                    nav.isStopped = true;
                    waterSplashes.SetActive(false);
                    StopCoroutine(FadeOutAudio());
                    if(engineSound != null)
                    {
                        engineSound.pitch = 0;
                    }
                    if(!Reached)
                    {
                        onReachEndPoint?.Invoke();
                        Reached = true;
                    }
                 
                    this.enabled = false;
                    
                }
            }
        }
    }
}
