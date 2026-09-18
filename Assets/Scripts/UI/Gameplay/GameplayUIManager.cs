using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using System.Collections;

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
        [SerializeField] private GameObject _missionNameContainer;
        [SerializeField] private CanvasGroup _missionNameContainerCanvasGroup;
        [SerializeField] private Vector2 _missionNameContainerStartPosition;
        [SerializeField] private Vector2 _missionNameContainerEndPosition;
        [SerializeField] private GameObject _missionGoalContainer;
        [SerializeField] private CanvasGroup _missionGoalContainerCanvasGroup;
        [SerializeField] private Vector2 _missionGoalContainerStartPosition;
        [SerializeField] private Vector2 _missionGoalContainerEndPosition;
        [SerializeField] private float _slideAnimationDuration;


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
            ToggleMissionNameContainer(false);
            ToggleMissionGoalContainer(false);
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
            StartCoroutine(ShowMissionInfo());
        }

        private IEnumerator ShowMissionInfo()
        {
            StartCoroutine(AnimateSlideFadeUI(_missionNameContainer, _missionNameContainerCanvasGroup,
                _missionNameContainerStartPosition, _missionNameContainerEndPosition, FadeTypeEnum.FadeIn, _slideAnimationDuration));

            yield return new WaitForSeconds(_slideAnimationDuration);

            StartCoroutine(AnimateSlideFadeUI(_missionGoalContainer, _missionGoalContainerCanvasGroup,
                _missionGoalContainerStartPosition, _missionGoalContainerEndPosition, FadeTypeEnum.FadeIn, _slideAnimationDuration));
        }

        private void HideMissionInfo()
        {
            StartCoroutine(AnimateSlideFadeUI(_missionNameContainer, _missionNameContainerCanvasGroup,
                _missionNameContainerEndPosition, _missionNameContainerStartPosition, FadeTypeEnum.FadeOut, _slideAnimationDuration));
        }

        private IEnumerator AnimateSlideFadeUI(GameObject gameObj, CanvasGroup canvasGroup, Vector2 startPosition, Vector2 endPosition, FadeTypeEnum fadeType, float duration)
        {
            if (gameObj == null || canvasGroup == null)
                yield break;

            RectTransform rectTransform = gameObj.GetComponent<RectTransform>();

            if (rectTransform == null)
                yield break;

            if (fadeType == FadeTypeEnum.FadeIn)
                gameObj.SetActive(true);

            float startAlpha = fadeType == FadeTypeEnum.FadeIn ? 0f : 1f;
            float endAlpha = fadeType == FadeTypeEnum.FadeIn ? 1f : 0f;

            rectTransform.anchoredPosition = startPosition;
            canvasGroup.alpha = startAlpha;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float timer = Mathf.Clamp01(elapsed / duration);
                timer = Mathf.SmoothStep(0f, 1f, timer);

                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, timer);

                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer);

                yield return null;
            }

            rectTransform.anchoredPosition = endPosition;
            canvasGroup.alpha = endAlpha;

            if (fadeType == FadeTypeEnum.FadeOut)
                gameObj.SetActive(false);
        }

        private void ToggleMissionNameContainer(bool value)
        {
            _missionNameContainer.SetActive(value);
        }

        private void ToggleMissionGoalContainer(bool value)
        {
            _missionGoalContainer.SetActive(value);
        }
    }
}