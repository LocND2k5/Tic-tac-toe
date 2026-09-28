using UnityEngine.SceneManagement;
using Khang.Core;

namespace Khang.Commands
{
    public class LoadSceneCommand : ICommand
    {
        private readonly string sceneName;

        public LoadSceneCommand(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public void Execute()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
