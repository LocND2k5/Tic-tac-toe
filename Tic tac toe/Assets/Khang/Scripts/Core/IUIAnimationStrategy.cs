using UnityEngine;

namespace Khang.Core
{
    public interface IUIAnimationStrategy
    {
        void Show(GameObject uiElement);
        void Hide(GameObject uiElement);
    }
}
