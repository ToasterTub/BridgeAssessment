using System;
using Buildables.Interfaces;
using UnityEngine;

namespace Buildables
{
    public abstract class BuildablePart : MonoBehaviour
    {
        static readonly int ShaderColorID = Shader.PropertyToID("_Color");
        
        [SerializeField] BoxCollider buildCollider;
        [SerializeField] MeshRenderer mainRenderer;
        
        Material _mainRendererMaterial;
        Color _validColor;

        public BoxCollider BuildCollider => buildCollider;
        protected MeshRenderer MainRenderer => mainRenderer;

        void Awake()
        {
            _mainRendererMaterial = mainRenderer.material;
            _validColor = _mainRendererMaterial.color;
        }

        public void SetMaterialInvalid()
        {
            _mainRendererMaterial.color = Color.red;
        }

        public void SetMaterialValid()
        {
            _mainRendererMaterial.color = _validColor;
        }

        public abstract void StartCreateAnimation(Action onComplete = null);

        public abstract void StartDestroyAnimation(Action onComplete = null);
    }
}