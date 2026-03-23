using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IfThenElse
{ 
    //ADVANCED ALGORITHMIC SCRIPT. RESERVED FOR IfThenElse Digital GAMES ONLY.
    //Written by Tanmay Kulkarni
    public class FaceController : MonoBehaviour
    {
        [Header("Controls")]
        public bool ManualControl = false;
        public bool AutoBlink = false;
        public bool AutoSpeak = false;
        public bool LiveUpdate = false;

        [Range(0f, 100f)]
        public float JawValue;
        [Range(0f, 120f)]
        public float EyeLidsValue;
        [Range(-100f, 100f)]
        public float EyeBrowsValue;
        [Range(0f, 100f)]
        public float LipsConvergeValue;
        [Range(0f, 100f)]
        public float LipsDivergeValue;

        [Range(0f, 100f)]
        public float EyeLeft;
        [Range(0f, 100f)]
        public float EyeRight;
        [Range(0f, 100f)]
        public float EyeUp;
        [Range(0f, 100f)]
        public float EyeDown;

        [Space(5)]

        [Header("Renderers")]
        public SkinnedMeshRenderer[] Face;
        public SkinnedMeshRenderer EyeLidsAndBrows;
        public SkinnedMeshRenderer[] Eyes;  //0 left, 1 right

        [Space(5)]
        [Header("Eye Defaults")]
        public float defaultTopConstL = 0;
        public float defaultBottomConstL = 0;
        public float defaultRightConstL = 0;
        public float defaultLeftConstL = 0;
        public float defaultTopConstR = 0;
        public float defaultBottomConstR = 0;
        public float defaultRightConstR = 0;
        public float defaultLeftConstR = 0;
        private void Start()
        {
            autoBlinking = false;
        }
        bool autoBlinking = false;
        bool isSpeaking = false;
        IEnumerator StartAutoBlinking()
        {
            if (autoBlinking)
            {
                yield return new WaitForSeconds(Random.Range(5.5f, 8f));
                float elapsedTime = 0;
                while (elapsedTime < 0.2f)
                {
                    elapsedTime += Time.deltaTime;
                    EyeLidsValue = Mathf.Clamp(EyeLidsValue + 12f, 0, 120);
                    EyeLidsAndBrows.SetBlendShapeWeight(3, EyeLidsValue);
                    yield return null;
                }

                elapsedTime = 0;
                while (elapsedTime < 0.2f)
                {
                    elapsedTime += Time.deltaTime;
                    EyeLidsValue = Mathf.Clamp(EyeLidsValue - 12f, 0, 120);
                    EyeLidsAndBrows.SetBlendShapeWeight(3, EyeLidsValue);
                    yield return null;
                }
                StopCoroutine(StartAutoBlinking());
                StartCoroutine(StartAutoBlinking());
            }

        }

        private void OnValidate()
        {
            if(LiveUpdate)
            {
                Eyes[0].SetBlendShapeWeight(0, Mathf.Clamp(EyeRight + defaultRightConstL, 0, 100));
                Eyes[0].SetBlendShapeWeight(1, Mathf.Clamp(EyeLeft + defaultLeftConstL, 0, 100));
                Eyes[0].SetBlendShapeWeight(2, Mathf.Clamp(EyeUp + defaultTopConstL, 0, 100));
                Eyes[0].SetBlendShapeWeight(3, Mathf.Clamp(EyeDown + defaultBottomConstL, 0, 100));

                Eyes[1].SetBlendShapeWeight(0, Mathf.Clamp(EyeRight + defaultRightConstR, 0, 100));
                Eyes[1].SetBlendShapeWeight(1, Mathf.Clamp(EyeLeft + defaultLeftConstR, 0, 100));
                Eyes[1].SetBlendShapeWeight(2, Mathf.Clamp(EyeUp + defaultTopConstR, 0, 100));
                Eyes[1].SetBlendShapeWeight(3, Mathf.Clamp(EyeDown + defaultBottomConstR, 0, 100));
                EyeLidsAndBrows.SetBlendShapeWeight(4, EyeBrowsValue);
                EyeLidsAndBrows.SetBlendShapeWeight(3, EyeLidsValue);

                foreach(SkinnedMeshRenderer f in Face)
                {
                    f.SetBlendShapeWeight(0, JawValue);
                    f.SetBlendShapeWeight(1, LipsDivergeValue);
                    f.SetBlendShapeWeight(2, LipsConvergeValue);
                }
             
               
            }  
        }

        private void Update()
        {
            if (AutoBlink && !autoBlinking)
            {
                autoBlinking = true;
                StartCoroutine(StartAutoBlinking());
            }
            if (!AutoBlink && autoBlinking)
            {
                autoBlinking = false;
                StopCoroutine(StartAutoBlinking());
            }

            if (ManualControl)
            {
                if (!AutoSpeak)
                {
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        f.SetBlendShapeWeight(0, JawValue);
                        f.SetBlendShapeWeight(1, LipsConvergeValue);
                        f.SetBlendShapeWeight(2, LipsDivergeValue);
                    }
                }

                Eyes[0].SetBlendShapeWeight(0, Mathf.Clamp(EyeRight + defaultRightConstL, 0, 100));
                Eyes[0].SetBlendShapeWeight(1, Mathf.Clamp(EyeLeft + defaultLeftConstL, 0, 100));
                Eyes[0].SetBlendShapeWeight(2, Mathf.Clamp(EyeUp + defaultTopConstL, 0, 100));
                Eyes[0].SetBlendShapeWeight(3, Mathf.Clamp(EyeDown + defaultBottomConstL, 0, 100));

                Eyes[1].SetBlendShapeWeight(0, Mathf.Clamp(EyeRight + defaultRightConstR, 0, 100));
                Eyes[1].SetBlendShapeWeight(1, Mathf.Clamp(EyeLeft + defaultLeftConstR, 0, 100));
                Eyes[1].SetBlendShapeWeight(2, Mathf.Clamp(EyeUp + defaultTopConstR, 0, 100));
                Eyes[1].SetBlendShapeWeight(3, Mathf.Clamp(EyeDown + defaultBottomConstR, 0, 100));

                EyeLidsAndBrows.SetBlendShapeWeight(4, EyeBrowsValue);
            }

            if (AutoSpeak && !isSpeaking)
            {
                isSpeaking = true;
                startTimeJaw = Time.time;
                startTimeLC = Time.time;
                startTimeLD = Time.time;
                StartCoroutine(MoveJaw());
                StartCoroutine(ConvergeLips());
                StartCoroutine(DivergeLips());
            }

            if (!AutoSpeak && isSpeaking)
            {
                if (JawValue != 0 || LipsConvergeValue != 0 || LipsDivergeValue != 0)
                {
                    StopCoroutine(MoveJaw());
                    StopCoroutine(ConvergeLips());
                    StopCoroutine(DivergeLips());

                    float desiredJawValue = JawValue - 1;
                    JawValue = Mathf.Clamp(desiredJawValue, 0, 100);
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        f.SetBlendShapeWeight(0, JawValue);
                    }
                    float desiredJLDValue = LipsDivergeValue - 1;
                    LipsDivergeValue = Mathf.Clamp(desiredJLDValue, 0, 100);
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        f.SetBlendShapeWeight(0, LipsDivergeValue);
                    }
                    float desiredJLCValue = LipsConvergeValue - 1;
                    LipsConvergeValue = Mathf.Clamp(desiredJLCValue, 0, 100);
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        f.SetBlendShapeWeight(0, LipsConvergeValue);
                    }
                }
                else if (LipsConvergeValue == 0 && LipsDivergeValue == 0 && JawValue == 0)
                {
                    isSpeaking = false;
                }
            }
        }
        public float SpeakSpeedInverse = 0.2f;
        float startTimeJaw;
        float startTimeLC;
        float startTimeLD;
        IEnumerator MoveJaw()
        {
            float targetValue = Random.Range(0, 50);
            while (JawValue != targetValue)
            {
                if (AutoSpeak)
                {
                    float t = (Time.time - startTimeJaw) / SpeakSpeedInverse;
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        JawValue = Mathf.SmoothStep(JawValue, targetValue, t);
                        f.SetBlendShapeWeight(0, JawValue);
                    }
                    yield return null;
                }
                else
                {
                    yield break;
                }

            }
            StopCoroutine(MoveJaw());
            startTimeJaw = Time.time;
            if (isSpeaking)
            {
                StartCoroutine(MoveJaw());
            }

        }
        IEnumerator ConvergeLips()
        {

            float targetValue = Random.Range(0, 40);
            while (LipsConvergeValue != targetValue)
            {
                if (AutoSpeak)
                {
                    float t = (Time.time - startTimeLC) / SpeakSpeedInverse;
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        LipsConvergeValue = Mathf.SmoothStep(LipsConvergeValue, targetValue, t);
                        f.SetBlendShapeWeight(2, LipsConvergeValue);
                    }
                    yield return null;
                }
                else
                {
                    yield break;
                }
            }
            StopCoroutine(ConvergeLips());
            startTimeLC = Time.time;
            if (isSpeaking)
            {
                StartCoroutine(ConvergeLips());
            }
        }
        IEnumerator DivergeLips()
        {

            float targetValue = Random.Range(0, 40);
            while (LipsDivergeValue != targetValue)
            {
                if (AutoSpeak)
                {
                    float t = (Time.time - startTimeLD) / SpeakSpeedInverse;
                    foreach (SkinnedMeshRenderer f in Face)
                    {
                        LipsDivergeValue = Mathf.SmoothStep(LipsDivergeValue, targetValue, t);
                        f.SetBlendShapeWeight(1, LipsDivergeValue);
                    }
                    yield return null;
                }
                else
                {
                    yield break;
                }
            }
            StopCoroutine(DivergeLips());
            startTimeLD = Time.time;
            if (isSpeaking)
            {
                StartCoroutine(DivergeLips());
            }



        }


    }

}

