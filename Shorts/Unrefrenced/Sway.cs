using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IfThenElse
{
    public class Sway : MonoBehaviour
    {
        public float rotationalAmount;
        public float maxRotationalAmount;
        public float smoothRotation;
        public float angularSpeed;

        Quaternion initialRot;
        float inputX;
        float inputY;
        private void Start()
        {
            initialRot = transform.localRotation;
        }
        void Update()
        {
            inputX = (Input.GetAxis("Mouse Y"));
            inputY = -(Input.GetAxis("Mouse X"));

            float finalX = Mathf.Clamp(inputX, -maxRotationalAmount, maxRotationalAmount);
            float finalY = Mathf.Clamp(inputY, -maxRotationalAmount, maxRotationalAmount);

            Quaternion finalRot = Quaternion.Euler(finalX, finalY, 0);

            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRot * finalRot, smoothRotation * Time.deltaTime);
        }


    }
}
