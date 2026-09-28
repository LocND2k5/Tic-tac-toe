using UnityEngine;
using Khang.Core;

namespace Khang.States
{
    public class MainPanelState : IMenuState
    {
        private readonly GameObject mainPanel;
        private readonly IUIAnimationStrategy animStrategy;

        public MainPanelState(GameObject mainPanel, IUIAnimationStrategy animStrategy)
        {
            this.mainPanel = mainPanel;
            this.animStrategy = animStrategy;
        }

        public void EnterState()
        {
            animStrategy?.Show(mainPanel);
        }

        public void ExitState()
        {
            animStrategy?.Hide(mainPanel);
        }
    }
}
