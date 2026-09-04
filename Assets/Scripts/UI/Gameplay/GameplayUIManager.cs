using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;

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
        [SerializeField] private GameObject _missionInfoContainer;
        [SerializeField] private TMP_Text _missionNameText;
        [SerializeField] private TMP_Text _missionGoalInfoText;


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
            _eventBusServiceObj.Subscribe<UpdateMissionInfoEvent>(OnUpdateMissionInfo);
        }

        private void UnsubscribeToEvents()
        {
            _eventBusServiceObj.Unsubscribe<PlayerScopeInEvent>(OnPlayerScopeIn);
            _eventBusServiceObj.Unsubscribe<PlayerScopeOutEvent>(OnPlayerScopeOut);
            _eventBusServiceObj.Unsubscribe<PlayerShotEvent>(OnPlayerShot);
            _eventBusServiceObj.Unsubscribe<UpdateMissionInfoEvent>(OnUpdateMissionInfo);
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

        private void SetMissionNameText(string missionName)
        {
            _missionNameText.text = missionName;
        }

        private void SetMissionGoalInfoText(string missionGoalInfo)
        {
            _missionGoalInfoText.text = missionGoalInfo;
        }

        private void OnUpdateMissionInfo(UpdateMissionInfoEvent eventObj)
        {
            SetMissionNameText(eventObj.MissionName);
            SetMissionGoalInfoText(eventObj.MissionGoal);
        }
    }
}