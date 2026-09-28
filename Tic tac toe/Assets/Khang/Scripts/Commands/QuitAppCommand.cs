using UnityEngine;
using Khang.Core;

namespace Khang.Commands
{
    public class QuitAppCommand : ICommand
    {
        public void Execute()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
