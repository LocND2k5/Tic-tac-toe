using UnityEngine;
using Khang.Core;

namespace Khang.Commands
{
    public class LogCommandDecorator : ICommand
    {
        private readonly ICommand wrappedCommand;
        private readonly string logMessage;

        public LogCommandDecorator(ICommand command, string logMessage)
        {
            this.wrappedCommand = command;
            this.logMessage = logMessage;
        }

        public void Execute()
        {
            Debug.Log($"[Khang]: {logMessage}");
            wrappedCommand?.Execute();
        }
    }
}
