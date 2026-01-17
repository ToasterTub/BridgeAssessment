using Buildables.Interfaces;
using UnityEngine;

namespace Buildables
{
    public class EditableCollider : MonoBehaviour
    {
        [SerializeField] BoxCollider boxCollider;
        IEditable _parent;

        public BoxCollider BoxCollider => boxCollider;

        public void Setup(IEditable parent)
        {
            _parent = parent;
        }

        public bool TryGetEditable(out IEditable parent)
        {
            parent = _parent;
            return parent != null;
        }
    }
}