using System;
using System.Collections;
using PropertyAttributes;
using Buildables;
using UnityEngine;

namespace BridgePlacement
{
    public class BridgeSegment : BuildablePart
    {
        [SerializeField, DisableGUI] float length;
        [SerializeField] Transform startPoint;
        [SerializeField] Transform endPoint;
        [SerializeField] float animateInDuration = .2f;
        [SerializeField] float animateOutDuration = .2f;
        [SerializeField] AnimationCurve animateInScaleCurve;
        [SerializeField] AnimationCurve animateOutScaleCurve;

        public float Length => length;

        void OnDrawGizmosSelected()
        {
            if (startPoint != null && startPoint && endPoint != null && endPoint)
            {
                Color holdColor = Gizmos.color;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(startPoint.transform.position, endPoint.transform.position);
                Gizmos.color = holdColor;
            }
        }

        public override void StartCreateAnimation(Action onComplete = null)
        {
            ClearAnimation();
            _animationRoutine = StartCoroutine(CreateAnimation(onComplete));
        }

        public override void StartDestroyAnimation(Action onComplete = null)
        {
            ClearAnimation();
            _animationRoutine = StartCoroutine(DestroyAnimation(onComplete));
        }

        void ClearAnimation()
        {
            if (_animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
            }
        }

        Coroutine _animationRoutine;
        IEnumerator CreateAnimation(Action onComplete)
        {
            float time = 0f;

            while (time < animateInDuration)
            {
                float percent = time / animateInDuration;
                float value = animateInScaleCurve.Evaluate(Mathf.Clamp01(percent));

                MainRenderer.transform.localScale = Vector3.one * value;
                
                time += Time.deltaTime;

                yield return null;
            }

            MainRenderer.transform.localScale = Vector3.one;
            onComplete?.Invoke();
        }
        
        IEnumerator DestroyAnimation(Action onComplete)
        {
            float time = 0f;

            Vector3 startScale = MainRenderer.transform.localScale;

            while (time < animateInDuration)
            {
                float percent = time / animateInDuration;
                float value = animateOutScaleCurve.Evaluate(Mathf.Clamp01(percent));

                MainRenderer.transform.localScale = startScale * value;
                
                time += Time.deltaTime;

                yield return null;
            }

            MainRenderer.transform.localScale = Vector3.zero;
            
            onComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}