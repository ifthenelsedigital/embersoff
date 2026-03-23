using System;
using UnityEngine;

namespace IfThenElse
{
    [Serializable]
    public class MouseLook
    {
        public float Sensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;
        public bool autoLockCursor = true;

        Transform characterTransform;
        public Transform cameraTransform;


        public Quaternion m_CharacterTargetRot;
        public Quaternion m_CameraTargetRot;

       
        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            characterTransform = character;
            cameraTransform = camera;
        }

       
        public float sensitivityConstInEditor;
        public void LookRotation()
        {
            
            float yRot = Input.GetAxis("Mouse X") * Sensitivity * 10;
            float xRot = Input.GetAxis("Mouse Y") * Sensitivity * 10;
            if(xRot != 0 || yRot != 0)
            {
                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

                if (clampVerticalRotation)
                    m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

                if (smooth)
                {
                    characterTransform.localRotation = Quaternion.Slerp(characterTransform.localRotation, m_CharacterTargetRot,
                        smoothTime * Time.deltaTime);
                    cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, m_CameraTargetRot,
                        smoothTime * Time.deltaTime);
                }
                else
                {
                    characterTransform.localRotation = m_CharacterTargetRot;
                    cameraTransform.localRotation = m_CameraTargetRot;
                }
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
