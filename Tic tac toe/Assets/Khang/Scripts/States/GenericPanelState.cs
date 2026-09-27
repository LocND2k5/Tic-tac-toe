using UnityEngine;
using Khang.Core;

namespace Khang.States
{
    // Một State dùng chung cho các Panel để tránh lặp code
    public class GenericPanelState : IMenuState
    {
        private readonly GameObject panel;
        private readonly IUIAnimationStrategy animStrategy;

        public GenericPanelState(GameObject panel, IUIAnimationStrategy animStrategy)
        {
            this.panel = panel;
            this.animStrategy = animStrategy;
        }

        public void EnterState()
        {
            if (panel != null) animStrategy?.Show(panel);
        }

        public void ExitState()
        {
            if (panel != null) animStrategy?.Hide(panel);
        }
    }
}
