using NUnit.Framework;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.SceneLoader;
using SniperStrategyGame.Tutorial;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SniperStrategyGame.UI
{
    public class TutorialUIManager : MonoBehaviour
    {
        [Header("Tutorial Overlay")]
        [SerializeField] private Image _tutorialOverlayImage;
        [SerializeField] private Material _tutorialOverlayMaterial;

        [Header("Button Containers")]
        [SerializeField] private RectTransform _scopeInButtonContainer;
        [SerializeField] private RectTransform _scopeOutButtonContainer;
        [SerializeField] private RectTransform _shootButtonContainer;

        [Header("Tutorial Instruction")]
        [SerializeField] private Image _tutorialInstructionBackground;
        [SerializeField] private GameObject _tutorialInstructionContainer;
        [SerializeField] private TMP_Text _tutorialInstructionText;

        [Header("Tutorial Notification")]
        [SerializeField] private GameObject _tutorialNotificationContainer;
        [SerializeField] private TMP_Text _tutorialNotificationText;
        [SerializeField] private Button _continueTutorialButton;
        [SerializeField] private TMP_Text _continueTutorialButtonText;
        [SerializeField] private Button _backtToStageSelectionButton;

        [Header("Tutorial Pause Menu")]
        [SerializeField] private GameObject _tutorialPauseMenuContainer;
        [SerializeField] private Button _pauseTutorialButton;
        [SerializeField] private Button _resumeTutorialButton;
        [SerializeField] private Button _restartTutorialButton;
        [SerializeField] private Button _exitTutorialButton;

        [SerializeField] private float _revealDuration = 0.5f;
        [SerializeField] private float _dissolveDuration = 0.25f;

        private EventBusService _eventBusServiceObj;
        private RectTransform _tutorialInstructionContainerRectTransform;
        private Material _overlayMaterial;
        private Material _tutInstructionBGMaterial;
        private Color _originalTextColor;

        private static readonly int HoleCenterID = Shader.PropertyToID("_HoleCenter");
        private static readonly int HoleSizeID = Shader.PropertyToID("_HoleSize");
        private static readonly int ProgressID = Shader.PropertyToID("_Progress");

        private void Awake()
        {
            var services = GameManager.Instance.Services;

            _eventBusServiceObj = services.Get<EventBusService>();
            _tutorialInstructionContainerRectTransform = _tutorialInstructionContainer.GetComponent<RectTransform>();
            _overlayMaterial = new Material(_tutorialOverlayMaterial);
            _tutorialOverlayImage.material = _overlayMaterial;
            _tutInstructionBGMaterial = new Material(_tutorialInstructionBackground.material);
            _tutorialInstructionBackground.material = _tutInstructionBGMaterial;
            _originalTextColor = _tutorialInstructionText.color;
        }

        private void Start()
        {
            ToggleTutorialOverlay(false);
            ToggleTutorialInstructionContainer(false);
            ToggleTutorialNotificationContainer(false);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _continueTutorialButton.onClick.AddListener(OnContinueTutorialButtonClicked);
            _backtToStageSelectionButton.onClick.AddListener(OnBackToStageSelectionButtonClicked);
            _pauseTutorialButton.onClick.AddListener(OnPauseTutorialButtonClicked);
            _resumeTutorialButton.onClick.AddListener(OnResumeTutorialButtonClicked);
            _restartTutorialButton.onClick.AddListener(OnRestartTutorialButtonClicked);
            _exitTutorialButton.onClick.AddListener(OnExitTutorialButtonClicked);

            _eventBusServiceObj.Subscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Subscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);
            _eventBusServiceObj.Subscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);
            _eventBusServiceObj.Subscribe<AllTutorialsCompletedEvent>(OnAllTutorialsCompleted);
            _eventBusServiceObj.Subscribe<PlayerShotEvent>(OnPlayerShotTutorialUI);
            _eventBusServiceObj.Subscribe<TutorialPauseEvent>(OnTutorialPauseUI);
        }

        private void UnsubscribeToEvents()
        {
            _continueTutorialButton.onClick.RemoveListener(OnContinueTutorialButtonClicked);
            _backtToStageSelectionButton.onClick.RemoveListener(OnBackToStageSelectionButtonClicked);
            _pauseTutorialButton.onClick.RemoveListener(OnPauseTutorialButtonClicked);
            _resumeTutorialButton.onClick.RemoveListener(OnResumeTutorialButtonClicked);
            _restartTutorialButton.onClick.RemoveListener(OnRestartTutorialButtonClicked);
            _exitTutorialButton.onClick.RemoveListener(OnExitTutorialButtonClicked);

            _eventBusServiceObj.Unsubscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Unsubscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);
            _eventBusServiceObj.Unsubscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);
            _eventBusServiceObj.Unsubscribe<AllTutorialsCompletedEvent>(OnAllTutorialsCompleted);
            _eventBusServiceObj.Unsubscribe<PlayerShotEvent>(OnPlayerShotTutorialUI);
            _eventBusServiceObj.Unsubscribe<TutorialPauseEvent>(OnTutorialPauseUI);
        }

        private void OnContinueTutorialButtonClicked()
        {
            _tutorialNotificationContainer.SetActive(false);
            _eventBusServiceObj.Publish(new ContinueTutorialEvent());
        }

        private void OnBackToStageSelectionButtonClicked()
        {
            _tutorialNotificationContainer.SetActive(false);
            GameManager.Instance.Services.Get<SceneLoaderService>().LoadScene(SceneNameEnum.StageSelection);
        }

        private void OnTutorialGroupCompleted(TutorialGroupCompletedEvent eventObj)
        {
            SetupTutorialNotificationText($"{eventObj.CompletedGroupName} Tutorial completed!");
            SetupContinueTutorialButtonText($"Continue to {eventObj.NextGroupName} tutorial.");
            ToggleContinueTutorialButton(true);
            ToggleBacktToStageSelectionButton(false);
            ToggleTutorialNotificationContainer(true);
        }

        private void OnAllTutorialsCompleted(AllTutorialsCompletedEvent eventObj)
        {
            SetupTutorialNotificationText("All Tutorials completed!");
            ToggleContinueTutorialButton(false);
            ToggleBacktToStageSelectionButton(true);
            ToggleTutorialNotificationContainer(true);
        }        

        private void SetupTutorialNotificationText(string value)
        {
            _tutorialNotificationText.text = value;
        }

        private void SetupContinueTutorialButtonText(string value)
        {
            _continueTutorialButtonText.text = value;
        }

        private void ToggleTutorialNotificationContainer(bool value)
        {
            _tutorialNotificationContainer.SetActive(value);
        }

        private void ToggleContinueTutorialButton(bool value)
        {
            _continueTutorialButton.gameObject.SetActive(value);
        }

        private void ToggleBacktToStageSelectionButton(bool value)
        {
            _backtToStageSelectionButton.gameObject.SetActive(value);
        }

        private void ToggleTutorialOverlay(bool value)
        {
            _tutorialOverlayImage.gameObject.SetActive(value);
        }

        private void ToggleTutorialInstructionContainer(bool value)
        {
            _tutorialInstructionContainer.gameObject.SetActive(value);
        }

        private void OnTutorialStepStarted(TutorialStepStartedEvent eventObj)
        {
            TutorialStepData tutorialStepData = eventObj.TutorialStepData;
            SetupTutorialInstructionContent(tutorialStepData);
            StartCoroutine(RevealInstructionCoroutine());
            ToggleTutorialOverlay(false);

            if (tutorialStepData.showOverlay)
            {
                SetUpTutorialOverlay(tutorialStepData.tutorialAction);
                ToggleTutorialOverlay(true);
            }
        }

        private void SetupTutorialInstructionContent(TutorialStepData tutorialStepData)
        {
            _tutorialInstructionContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tutorialStepData.instructionContainerSize);
            _tutorialInstructionText.text = tutorialStepData.instruction;
        }
        
        public void SetUpTutorialOverlay(TutorialActionEnum action)
        {
            RectTransform buttonContainer = GetTutorialButton(action);
            SetHole(buttonContainer);
        }

        private RectTransform GetTutorialButton(TutorialActionEnum action)
        {
            switch (action)
            {
                case TutorialActionEnum.ScopeIn:
                    return _scopeInButtonContainer;

                case TutorialActionEnum.ScopeOut:
                    return _scopeOutButtonContainer;

                case TutorialActionEnum.Shoot:
                    return _shootButtonContainer;

                default:
                    return null;
            }
        }

        private void SetHole(RectTransform target)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            Vector2 center = (bottomLeft + topRight) * 0.5f;
            Vector2 size = topRight - bottomLeft;

            center.x /= Screen.width;
            center.y /= Screen.height;

            size.x /= Screen.width;
            size.y /= Screen.height;

            _overlayMaterial.SetVector(HoleCenterID, center);
            _overlayMaterial.SetVector(HoleSizeID, size);
        }

        private IEnumerator RevealInstructionCoroutine()
        {
            ToggleTutorialInstructionContainer(true);
            _tutorialInstructionBackground.gameObject.SetActive(true);
            
            yield return AnimateProgress(0f, 1f, _revealDuration);
        }

        private IEnumerator DissolveInstructionCoroutine()
        {
            yield return AnimateProgress(1f, 0f, _dissolveDuration);

            _tutorialInstructionBackground.gameObject.SetActive(false);
            ToggleTutorialInstructionContainer(false);
        }

        private IEnumerator AnimateProgress(float from, float to, float duration)
        {
            _tutInstructionBGMaterial.SetFloat(ProgressID, from);
            SetIntructionTextAlpha(from);

            if (duration <= 0f)
            {
                _tutInstructionBGMaterial.SetFloat(ProgressID, to);
                SetIntructionTextAlpha(to);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));

                _tutInstructionBGMaterial.SetFloat(ProgressID, progress);
                SetIntructionTextAlpha(progress);

                yield return null;
            }

            _tutInstructionBGMaterial.SetFloat(ProgressID, to);
            SetIntructionTextAlpha(to);
        }

        private void SetIntructionTextAlpha(float alpha)
        {
            Color color = _originalTextColor;
            color.a *= alpha;
            _tutorialInstructionText.color = color;
        }

        private void OnTutorialStepCompleted(TutorialStepCompletedEvent eventObj)
        {
            ToggleTutorialOverlay(false);
            StartCoroutine(DissolveInstructionCoroutine());
        }

        private void OnPlayerShotTutorialUI(PlayerShotEvent eventObj)
        {
            ToggleTutorialOverlay(false);
        }

        private void OnTutorialPauseUI(TutorialPauseEvent eventObj)
        {
            ToggleTutorialPauseMenuContainer(true);
        }

        private void ToggleTutorialPauseMenuContainer(bool value)
        {
            _tutorialPauseMenuContainer.gameObject.SetActive(value);
        }

        private void OnPauseTutorialButtonClicked() { }

        private void OnResumeTutorialButtonClicked()
        {
            ToggleTutorialPauseMenuContainer(false);
            RaiseTutorialResumeEventEvent();
        }

        private void OnRestartTutorialButtonClicked()
        {
            ToggleTutorialPauseMenuContainer(false);
        }

        private void OnExitTutorialButtonClicked()
        {
            ToggleTutorialPauseMenuContainer(false);
        }

        private void RaiseTutorialResumeEventEvent()
        {
            _eventBusServiceObj.Publish(new TutorialResumeEvent());
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();

            if (_overlayMaterial != null)
            {
                Destroy(_overlayMaterial);
                _overlayMaterial = null;
            }

            if (_tutInstructionBGMaterial != null)
            {
                Destroy(_tutInstructionBGMaterial);
                _tutInstructionBGMaterial = null;
            }
        }
    }
}
