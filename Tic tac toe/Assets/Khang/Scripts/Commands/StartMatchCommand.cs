using Khang.Core;
using UnityEngine.SceneManagement;

namespace Khang.Commands
{
    public class StartMatchCommand : ICommand
    {
        private readonly GameMode mode;
        private readonly int gridSize;
        private readonly string sceneName;

        public StartMatchCommand(GameMode mode, int gridSize, string sceneName = "SampleScene")
        {
            this.mode = mode;
            this.gridSize = gridSize;
            this.sceneName = sceneName;
        }

        public void Execute()
        {
            GameConfig.SelectedMode = mode;
            GameConfig.GridSize = gridSize;
            SceneManager.LoadScene(sceneName);
        }
    }
}
