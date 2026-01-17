using UI.EditMode;

namespace Buildables.Interfaces
{
    public interface IEditable
    {
        public void StartEdit(EditModeController editor);
        public void EndEdit();
        public void EditModeHoverUpdate();
        public void EditModeHoverExit();
    }
}