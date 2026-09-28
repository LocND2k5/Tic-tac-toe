using UnityEngine;
using Khang.Core;

namespace Khang.Strategies
{
    public class InstantAnimationStrategy : IUIAnimationStrategy
    {
        public void Show(GameObject uiElement)
        {
            if (uiElement != null) uiElement.SetActive(true);
        }

        public void Hide(GameObject uiElement)
        {
            if (uiElement != null) uiElement.SetActive(false);
        }
    }
}
