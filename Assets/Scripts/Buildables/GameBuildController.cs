using PropertyAttributes;
using Extensions;
using ScriptableObjects;
using UnityEngine;

namespace Buildables
{
    public class GameBuildController : MonoBehaviour
    {
        [SerializeField] BuildablePreset buildablePreset;

        BuildableHandlerBase _activeBuildableHandler = null;
        bool _buildModeEnabled = false;
        bool _isBuilding = false;

        public bool BuildModeEnabled => _buildModeEnabled;

        public bool TryEnterBuildMode()
        {
            if (_buildModeEnabled)
            {
                return false;
            }
            
            _buildModeEnabled = true;
            return true;
        }

        public bool TryExitBuildMode()
        {
            if (!_buildModeEnabled)
            {
                return false;
            }
            
            _buildModeEnabled = false;
            if (_isBuilding)
            {
                CancelBuild();
            }

            return true;
        }
        
        public void SetBuildable(BuildablePreset buildable)
        {
            if (_isBuilding)
            {
                CancelBuild();
            }
        }

        public void BuildInputStart(Vector3 worldPosition)
        {
            if (_isBuilding)
            {
                CancelBuild();
            }
            _isBuilding = true;
            InstantiateNewBuildable(worldPosition);
            _activeBuildableHandler?.OnBuildStart(worldPosition);
        }

        public void BuildInputDrag(Vector3 worldPosition)
        {
            _activeBuildableHandler?.OnBuildDrag(worldPosition);
        }
        
        public void BuildInputEnd(Vector3 worldPosition)
        {
            if (_activeBuildableHandler.IsValid())
            {
                _activeBuildableHandler.OnBuildEnd(worldPosition, out bool buildIsValid);
                if (!buildIsValid)
                {
                    CancelBuild();
                    return;
                }    
            }
            
            _isBuilding = false;
            _activeBuildableHandler = null;
        }
        
        void InstantiateNewBuildable(Vector3 position)
        {
            _activeBuildableHandler = Instantiate(buildablePreset.BuildableHandlerPrefab);
            _activeBuildableHandler.transform.position = position;
            _activeBuildableHandler.transform.eulerAngles = Vector3.zero;
        }
        
        void CancelBuild()
        {
            _activeBuildableHandler.OnBuildCancel();
            Destroy(_activeBuildableHandler.gameObject);
            _isBuilding = false;
        }
    }
}