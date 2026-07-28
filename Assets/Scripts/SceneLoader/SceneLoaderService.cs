using SniperStrategyGame.Event;
using UnityEngine.SceneManagement;

namespace SniperStrategyGame.SceneLoader
{
    public class SceneLoaderService
    {
        private readonly EventBusService _eventBusServiceObj;

        public SceneLoaderService(EventBusService eventBus)
        {
            _eventBusServiceObj = eventBus;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void LoadScene(SceneNameEnum sceneName)
        {
            SceneManager.LoadScene(sceneName.ToString());
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RaiseSceneLoadedEvent(scene);
        }

        private void RaiseSceneLoadedEvent(Scene scene)
        {
            if (System.Enum.TryParse(scene.name, out SceneNameEnum result))
            {
                _eventBusServiceObj.Publish(new SceneLoadedEvent(result));
            }
        }
    }
}