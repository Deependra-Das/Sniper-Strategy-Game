using SniperStrategyGame.SceneLoader;

namespace SniperStrategyGame.Event
{
    public class SceneLoadedEvent
    {
        public SceneNameEnum SceneName { get; }

        public SceneLoadedEvent(SceneNameEnum sceneName)
        {
            SceneName = sceneName;
        }
    }
}