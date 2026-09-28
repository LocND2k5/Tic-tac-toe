using Khang.Core;

namespace Khang.Commands
{
    public class ChangeStateCommand : ICommand
    {
        private readonly MainMenuSystem system;
        private readonly IMenuState targetState;

        public ChangeStateCommand(MainMenuSystem system, IMenuState targetState)
        {
            this.system = system;
            this.targetState = targetState;
        }

        public void Execute()
        {
            system.ChangeState(targetState);
        }
    }
}
