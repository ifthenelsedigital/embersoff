using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

#pragma warning disable 618, 649
namespace IfThenElse
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        public bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        public float m_RunSpeed;
        [SerializeField] [Range(0f, 5f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] public MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        public Camera m_Camera;
        public bool m_Jump;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        public CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        public bool m_PreviouslyGrounded = true;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        public bool m_Jumping;
        private AudioSource m_AudioSource;
    

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
        }
 
       
        private void Update()
        {
            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
          
                GetInput(out speed);
            
            if(speed != 0)
            {
                Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;


                m_MoveDir.x = desiredMove.x * speed;
                m_MoveDir.z = desiredMove.z * speed;


                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_Jump = false;
                        m_Jumping = true;
                    }
                }
                else
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }
                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

                ProgressStepCycle(speed);
                UpdateCameraPosition(speed);

            }
            // always move along the camera forward as it is the direction that it being aimed at


        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        public void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(0, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }

        public Vector2 inputs;
        public float crouchSpeed;
        public Animator Crosshair;
        public Animator playerWeaponAnim;
        private void GetInput(out float speed)
        {
            
            if (inputs == Vector2.zero)
            {
                if(Crosshair.isActiveAndEnabled)
                {
                    Crosshair.SetBool("isCrosshairLarge", false);
                }
                if(!player.isHoldingExplosive)
                {
                    playerWeaponAnim.SetBool("isRunning", false);
                }
                speed = 0;
                return;
            }

            // Read input
            float horizontal = inputs.x;
            float vertical = inputs.y;

            if (!m_IsWalking)
            {
                if (horizontal > 0 || horizontal < 0)
                {
                    //strafe
                    expectedSpeed = crouchSpeed;
                    Crosshair.SetBool("isCrosshairLarge", true);
                    playerWeaponAnim.SetBool("isRunning", false);
                }
                if (vertical < 0)
                {
                    //backwards
                    expectedSpeed = crouchSpeed;
                    Crosshair.SetBool("isCrosshairLarge", true);
                    playerWeaponAnim.SetBool("isRunning", false);

                }
                if (vertical > 0)
                {
                    //forwards
                    //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                    expectedSpeed = m_RunSpeed;
                    Crosshair.SetBool("isCrosshairLarge", true);
                    if (!playerWeaponAnim.GetBool("isScoped") && !player.currentWeaponScript.isScoping)
                        playerWeaponAnim.SetBool("isRunning", true);

                }
            }

            else
            {
                if(player.isProning)
                {
                    expectedSpeed = 1f;
                }
                else if(player.isCrouching)
                {
                    expectedSpeed = crouchSpeed;
                }
                else if(!player.isHoldingExplosive && !player.isUsingMelee && player.weaponsAnim.GetBool("isScoped"))
                {
                    expectedSpeed = crouchSpeed;
                }
                Crosshair.SetBool("isCrosshairLarge", true);
                if(player.isProning && !playerWeaponAnim.GetBool("isScoped"))
                {
                    playerWeaponAnim.SetBool("isRunning", true);
                }
            }
            speed = expectedSpeed;
            m_Input = new Vector2(horizontal, vertical);
         
        }

        public Player player;
        float expectedSpeed;

     
    }
}
