using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using UnityEngine;
using UnityEngine.UI;

namespace SniperStrategyGame.UI
{
    public class GameplayUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _scopeOverlay;
        [SerializeField] private Button _shootButton;
        [SerializeField] private Button _scopeInButton;
        [SerializeField] private Button _scopeOutButton;
        [SerializeField] private GameObject _scopeInButtonContainer;
        [SerializeField] private GameObject _scopeOutButtonContainer;

        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
        }

        private void OnEnable() => SubscribeToEvents();
        private void OnDisable() => UnsubscribeToEvents();

        private void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<PlayerScopeInEvent>(OnPlayerScopeIn);
            _eventBusServiceObj.Subscribe<PlayerScopeOutEvent>(OnPlayerScopeOut);
            _eventBusServiceObj.Subscribe<PlayerShotEvent>(OnPlayerShot);
        }

        private void UnsubscribeToEvents()
        {
            _eventBusServiceObj.Unsubscribe<PlayerScopeInEvent>(OnPlayerScopeIn);
            _eventBusServiceObj.Unsubscribe<PlayerScopeOutEvent>(OnPlayerScopeOut);
            _eventBusServiceObj.Unsubscribe<PlayerShotEvent>(OnPlayerShot);
        }

        private void Start()
        {
            ShowScopeIn();
        }

        private void OnPlayerScopeIn(PlayerScopeInEvent eventObj)
        {
            _scopeOverlay.SetActive(true);
            ShowScopeOut();
        }

        private void OnPlayerScopeOut(PlayerScopeOutEvent eventObj)
        {
            _scopeOverlay.SetActive(false);
            ShowScopeIn();
        }

        private void ShowScopeIn()
        {
            _scopeInButtonContainer.SetActive(true);
            _scopeOutButtonContainer.SetActive(false);
        }

        private void ShowScopeOut()
        {
            _scopeInButtonContainer.SetActive(false);
            _scopeOutButtonContainer.SetActive(true);
        }

        private void OnPlayerShot(PlayerShotEvent eventObj)
        {
            
        }
    }
}