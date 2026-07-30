using SniperStrategyGame.Main;
using SniperStrategyGame.SceneLoader;
using UnityEngine;
using UnityEngine.UI;

namespace SniperStrategyGame.UI.MainMenu
{
    public class MainMenuUIManager : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        private SceneLoaderService _sceneLoader;

        private void Awake()
        {
            _sceneLoader = GameManager.Instance.Services.Get<SceneLoaderService>();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void UnsubscribeToEvents()
        {
            _playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        public void OnPlayButtonClicked()
        {
            _sceneLoader.LoadScene(SceneNameEnum.StageSelection);
        }
    }
}