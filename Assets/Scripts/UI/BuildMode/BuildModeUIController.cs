using System;
using Buildables;
using UnityEngine;

namespace UI.BuildMode
{
    public class BuildModeUIController : MonoBehaviour
    {
        [SerializeField] GameBuildController buildController;
        [SerializeField] GameObject editModeCanvasMain;
        [SerializeField] GameObject buildModeCanvasMain;

        void Start()
        {
            EnterBuildMode();
        }

        public void EnterBuildMode()
        {
            if (buildController.TryEnterBuildMode())
            {
                UpdateCanvasState();   
            }
        }

        public void ExitBuildMode()
        {
            if (buildController.TryExitBuildMode())
            {
                UpdateCanvasState();
            }
        }

        void UpdateCanvasState()
        {
            if (buildController.BuildModeEnabled)
            {
                editModeCanvasMain.SetActive(false);
                buildModeCanvasMain.SetActive(true);
            }
            else
            {
                editModeCanvasMain.SetActive(true);
                buildModeCanvasMain.SetActive(false);
            }
        }
    }
}