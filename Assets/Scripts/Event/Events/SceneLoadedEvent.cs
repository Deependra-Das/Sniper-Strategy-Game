using SniperStrategyGame.SceneLoader;

namespace SniperStrategyGame.Event
{
    public struct SceneLoadedEvent
    {
        public SceneNameEnum SceneName { get; }

        public SceneLoadedEvent(SceneNameEnum sceneName)
        {
            SceneName = sceneName;
        }
    }
}