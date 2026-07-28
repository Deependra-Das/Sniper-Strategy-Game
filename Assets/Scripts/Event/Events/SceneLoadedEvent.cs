using SniperStrategyGame.SceneLoader;

public class SceneLoadedEvent
{
    public SceneNameEnum SceneName { get; }

    public SceneLoadedEvent(SceneNameEnum sceneName)
    {
        SceneName = sceneName;
    }
}