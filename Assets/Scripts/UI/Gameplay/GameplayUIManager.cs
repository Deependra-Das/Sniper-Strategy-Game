using SniperStrategyGame.Event;
using SniperStrategyGame.Gameplay;
using SniperStrategyGame.Main;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SniperStrategyGame.UI
{
    public class GameplayUIManager : MonoBehaviour
    {
        [Header("Gameplay Buttons")]
        [SerializeField] private GameObject _scopeOverlay;
        [SerializeField] private Button _shootButton;
        [SerializeField] private Button _scopeInButton;
        [SerializeField] private Button _scopeOutButton;
        [SerializeField] private GameObject _scopeInButtonContainer;
        [SerializeField] private GameObject _scopeOutButtonContainer;

        [Header("Mission Info Name & Goal")]
        [SerializeField] private GameObject _missionInfoContainer;
        [SerializeField] private Image _missionNameBackground;
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

        [Header("Gameplay Pause Menu")]
        [SerializeField] private GameObject _pauseMenuContainer;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton_PauseMenu;
        [SerializeField] private Button _restartButton_PauseMenu;
        [SerializeField] private Button _exitButton_PauseMenu;

        [SerializeField] private float _slideAnimationDuration;
        [SerializeField] private float _revealDuration = 1f;
        [SerializeField] private float _dissolveDuration = 0.5f;

        private Color _missionNameTextOriginalColor;
        private Material _missionBGMaterial;
        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
            _missionBGMaterial = new Material(_missionNameBackground.material);
            _missionNameBackground.material = _missionBGMaterial;
            _missionNameTextOriginalColor = _missionNameText.color;
        }

        private void OnEnable() => SubscribeToEvents();
        private void OnDisable() => UnsubscribeToEvents();

        private void SubscribeToEvents()
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClicked);
            _resumeButton_PauseMenu.onClick.AddListener(OnResumeButtonClicked);
            _restartButton_PauseMenu.onClick.AddListener(OnRestartButtonClicked);
            _exitButton_PauseMenu.onClick.AddListener(OnExitButtonClicked);

            _eventBusServiceObj.Subscribe<PlayerScopeInEvent>(OnPlayerScopeIn);
            _eventBusServiceObj.Subscribe<PlayerScopeOutEvent>(OnPlayerScopeOut);
            _eventBusServiceObj.Subscribe<PlayerShotEvent>(OnPlayerShot);
            _eventBusServiceObj.Subscribe<UpdateMissionInfoEvent>(OnUpdateMissionInfo);
            _eventBusServiceObj.Subscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompletedGameplayUI);
            _eventBusServiceObj.Subscribe<AllTutorialsCompletedEvent>(OnAllTutorialsCompletedGameplayUI);
            _eventBusServiceObj.Subscribe<TogglePauseButtonVisibilityEvent>(OnTogglePauseButtonVisibility);
            _eventBusServiceObj.Subscribe<GameplayPausedEvent>(OnGameplayPaused_GameplayUI);
            _eventBusServiceObj.Subscribe<GameplayResumedEvent>(OnGameplayResumed_GameplayUI);
            _eventBusServiceObj.Subscribe<GameplayRestartedEvent>(OnGameplayRestarted_GameplayUI);
            _eventBusServiceObj.Subscribe<GameplayExitedEvent>(OnGameplayExited_GameplayUI);
        }

        private void UnsubscribeToEvents()
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            _resumeButton_PauseMenu.onClick.RemoveListener(OnResumeButtonClicked);
            _restartButton_PauseMenu.onClick.RemoveListener(OnRestartButtonClicked);
            _exitButton_PauseMenu.onClick.RemoveListener(OnExitButtonClicked);

            _eventBusServiceObj.Unsubscribe<PlayerScopeInEvent>(OnPlayerScopeIn);
            _eventBusServiceObj.Unsubscribe<PlayerScopeOutEvent>(OnPlayerScopeOut);
            _eventBusServiceObj.Unsubscribe<PlayerShotEvent>(OnPlayerShot);
            _eventBusServiceObj.Unsubscribe<UpdateMissionInfoEvent>(OnUpdateMissionInfo);
            _eventBusServiceObj.Unsubscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompletedGameplayUI);
            _eventBusServiceObj.Unsubscribe<AllTutorialsCompletedEvent>(OnAllTutorialsCompletedGameplayUI);
            _eventBusServiceObj.Unsubscribe<TogglePauseButtonVisibilityEvent>(OnTogglePauseButtonVisibility);
            _eventBusServiceObj.Unsubscribe<GameplayPausedEvent>(OnGameplayPaused_GameplayUI);
            _eventBusServiceObj.Unsubscribe<GameplayResumedEvent>(OnGameplayResumed_GameplayUI);
            _eventBusServiceObj.Unsubscribe<GameplayRestartedEvent>(OnGameplayRestarted_GameplayUI);
            _eventBusServiceObj.Unsubscribe<GameplayExitedEvent>(OnGameplayExited_GameplayUI);
        }

        private void Start()
        {
            ToggleMissionNameContainer(false);
            ToggleMissionGoalContainer(false);
            TogglePauseMenuContainer(false);
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

        private void OnTutorialGroupCompletedGameplayUI(TutorialGroupCompletedEvent eventObj)
        {
            StartCoroutine(HideMissionInfo());
        }

        private void OnAllTutorialsCompletedGameplayUI(AllTutorialsCompletedEvent eventObj)
        {
            StartCoroutine(HideMissionInfo());
        }

        private IEnumerator ShowMissionInfo()
        {
            StartCoroutine(RevealMissionNameBackground());
            StartCoroutine(AnimateSlideFadeUI(_missionNameContainer, _missionNameContainerCanvasGroup,
                _missionNameContainerStartPosition, _missionNameContainerEndPosition, FadeTypeEnum.FadeIn, _slideAnimationDuration));

            yield return new WaitForSeconds(_slideAnimationDuration);

            StartCoroutine(AnimateSlideFadeUI(_missionGoalContainer, _missionGoalContainerCanvasGroup,
                _missionGoalContainerStartPosition, _missionGoalContainerEndPosition, FadeTypeEnum.FadeIn, _slideAnimationDuration));
        }

        private IEnumerator HideMissionInfo()
        {
            StartCoroutine(DissolveMissionNameBackground());
            StartCoroutine(AnimateSlideFadeUI(_missionGoalContainer, _missionGoalContainerCanvasGroup,
                _missionGoalContainerEndPosition, _missionGoalContainerStartPosition, FadeTypeEnum.FadeOut, _slideAnimationDuration));

            yield return new WaitForSeconds(_slideAnimationDuration);

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

        private IEnumerator RevealMissionNameBackground()
        {
            yield return AnimateMissionBackground(0f, 1f, _revealDuration);
        }

        private IEnumerator DissolveMissionNameBackground()
        {
            yield return AnimateMissionBackground(1f, 0f, _dissolveDuration);
        }

        private IEnumerator AnimateMissionBackground(float from, float to, float duration)
        {
            if (_missionBGMaterial == null)
                yield break;

            int progressID = Shader.PropertyToID("_Progress");

            _missionBGMaterial.SetFloat(progressID, from);
            SetMissionNameTextAlpha(from);

            if (duration <= 0f)
            {
                _missionBGMaterial.SetFloat(progressID, to);
                SetMissionNameTextAlpha(to);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                _missionBGMaterial.SetFloat(progressID, progress);
                SetMissionNameTextAlpha(progress);

                yield return null;
            }

            _missionBGMaterial.SetFloat(progressID, to);
            SetMissionNameTextAlpha(to);
        }

        private void SetMissionNameTextAlpha(float alpha)
        {
            if (_missionNameText == null)
                return;

            Color color = _missionNameTextOriginalColor;
            color.a *= alpha;
            _missionNameText.color = color;
        }

        public void OnPauseButtonClicked()
        {
            RaiseRequestPauseGameplayEvent();
        }

        public void OnResumeButtonClicked()
        {
            RaiseRequestResumeGameplayEvent();
        }

        public void OnRestartButtonClicked()
        {
            RaiseRequestRestartGameplayEvent();
        }

        public void OnExitButtonClicked()
        {
            RaiseRequestExitGameplayEvent();
        }

        private void OnTogglePauseButtonVisibility(TogglePauseButtonVisibilityEvent eventObj)
        {
            _pauseButton.gameObject.SetActive(eventObj.IsVisible);
        }

        private void OnGameplayPaused_GameplayUI(GameplayPausedEvent eventObj)
        {
            TogglePauseMenuContainer(true);
        }

        private void OnGameplayResumed_GameplayUI(GameplayResumedEvent eventObj)
        {
            TogglePauseMenuContainer(false);
        }

        private void OnGameplayRestarted_GameplayUI(GameplayRestartedEvent eventObj)
        {
            TogglePauseMenuContainer(false);
        }

        private void OnGameplayExited_GameplayUI(GameplayExitedEvent eventObj)
        {
            TogglePauseMenuContainer(false);
        }

        private void TogglePauseMenuContainer(bool value)
        {
            _pauseMenuContainer.gameObject.SetActive(value);
        }

        private void RaiseRequestPauseGameplayEvent()
        {
            _eventBusServiceObj.Publish(new RequestPauseGameplayEvent());
        }

        private void RaiseRequestResumeGameplayEvent()
        {
            _eventBusServiceObj.Publish(new RequestResumeGameplayEvent());
        }

        private void RaiseRequestRestartGameplayEvent()
        {
            _eventBusServiceObj.Publish(new RequestRestartGameplayEvent());
        }

        private void RaiseRequestExitGameplayEvent()
        {
            _eventBusServiceObj.Publish(new RequestExitGameplayEvent());
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();

            if (_missionBGMaterial != null)
            {
                Destroy(_missionBGMaterial);
                _missionBGMaterial = null;
            }
        }
    }
}