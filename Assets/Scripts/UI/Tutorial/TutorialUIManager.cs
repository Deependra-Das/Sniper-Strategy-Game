using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
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
        [SerializeField] private TMP_Text _tutorialNameNotificationText;
        [SerializeField] private Button _continueTutorialNotification_Button;
        [SerializeField] private TMP_Text _nextStepTutorialNotification_Text;
        [SerializeField] private Button _restartTutorialNotification_Button;
        [SerializeField] private Button _exitTutorialNotification_Button;

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

        private bool _wasTutorialNotificationActiveBeforePause;
        private bool _wasTutorialInstructionActiveBeforePause;
        private bool _wasTutorialOverlayActiveBeforePause;

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
            _continueTutorialNotification_Button.onClick.AddListener(OnContinueTutorialNotificationButtonClicked);
            _restartTutorialNotification_Button.onClick.AddListener(OnRestartTutorialNotificationButtonClicked);
            _exitTutorialNotification_Button.onClick.AddListener(OnExitTutorialNotificationButtonClicked);

            _eventBusServiceObj.Subscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Subscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);
            _eventBusServiceObj.Subscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);
            _eventBusServiceObj.Subscribe<AllTutorialsCompletedEvent>(OnAllTutorialsCompleted);
            _eventBusServiceObj.Subscribe<PlayerShotEvent>(OnPlayerShotTutorialUI);
            _eventBusServiceObj.Subscribe<GameplayPausedEvent>(OnGameplayPaused_TutorialUI);
            _eventBusServiceObj.Subscribe<GameplayResumedEvent>(OnGameplayResumed_TutorialUI);
            _eventBusServiceObj.Subscribe<GameplayRestartedEvent>(OnGameplayRestarted_TutorialUI);
            _eventBusServiceObj.Subscribe<TutorialContinuedEvent>(OnTutorialContinued_TutorialUI);
        }

        private void UnsubscribeToEvents()
        {
            _continueTutorialNotification_Button.onClick.RemoveListener(OnContinueTutorialNotificationButtonClicked);
            _restartTutorialNotification_Button.onClick.RemoveListener(OnRestartTutorialNotificationButtonClicked);
            _exitTutorialNotification_Button.onClick.RemoveListener(OnExitTutorialNotificationButtonClicked);

            _eventBusServiceObj.Unsubscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Unsubscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);
            _eventBusServiceObj.Unsubscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);
            _eventBusServiceObj.Unsubscribe<AllTutorialsCompletedEvent>(OnAllTutorialsCompleted);
            _eventBusServiceObj.Unsubscribe<PlayerShotEvent>(OnPlayerShotTutorialUI);
            _eventBusServiceObj.Unsubscribe<GameplayPausedEvent>(OnGameplayPaused_TutorialUI);
            _eventBusServiceObj.Unsubscribe<GameplayResumedEvent>(OnGameplayResumed_TutorialUI);
            _eventBusServiceObj.Unsubscribe<GameplayRestartedEvent>(OnGameplayRestarted_TutorialUI);
            _eventBusServiceObj.Unsubscribe<TutorialContinuedEvent>(OnTutorialContinued_TutorialUI);
        }

        private void OnContinueTutorialNotificationButtonClicked()
        {
            _eventBusServiceObj.Publish(new RequestContinueToNextTutorialEvent());
        }

        private void OnRestartTutorialNotificationButtonClicked()
        {
            RaiseRequestRestartGameplayEvent();
        }
        
        private void OnExitTutorialNotificationButtonClicked()
        {
            RaiseRequestExitGameplayEvent();
        }

        private void OnTutorialGroupCompleted(TutorialGroupCompletedEvent eventObj)
        {
            SetupTutorialNotificationText($"{eventObj.CompletedGroupName}");
            SetupNextStepTutorialNotificationText($"Next Step : {eventObj.NextGroupName} Tutorial");
            ToggleContinueTutorialButton(true);
            ToggleBacktToStageSelectionButton(false);
            ToggleTutorialNotificationContainer(true);
        }

        private void OnAllTutorialsCompleted(AllTutorialsCompletedEvent eventObj)
        {
            SetupTutorialNotificationText("");
            SetupNextStepTutorialNotificationText("You are ready to take up challenges");
            ToggleContinueTutorialButton(false);
            ToggleBacktToStageSelectionButton(true);
            ToggleTutorialNotificationContainer(true);
        }        

        private void SetupTutorialNotificationText(string value)
        {
            _tutorialNameNotificationText.text = value;
        }

        private void SetupNextStepTutorialNotificationText(string value)
        {
            _nextStepTutorialNotification_Text.text = value;
        }

        private void ToggleTutorialNotificationContainer(bool value)
        {
            _tutorialNotificationContainer.SetActive(value);
        }

        private void ToggleContinueTutorialButton(bool value)
        {
            _continueTutorialNotification_Button.gameObject.SetActive(value);
        }

        private void ToggleBacktToStageSelectionButton(bool value)
        {
            _exitTutorialNotification_Button.gameObject.SetActive(value);
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

        private void OnGameplayPaused_TutorialUI(GameplayPausedEvent eventObj)
        {
            _wasTutorialOverlayActiveBeforePause = _tutorialOverlayImage.gameObject.activeSelf;
            _wasTutorialInstructionActiveBeforePause = _tutorialInstructionContainer.gameObject.activeSelf;
            _wasTutorialNotificationActiveBeforePause = _tutorialNotificationContainer.activeSelf;

            ToggleTutorialOverlay(false);
            ToggleTutorialInstructionContainer(false);
            ToggleTutorialNotificationContainer(false);
        }

        private void OnGameplayResumed_TutorialUI(GameplayResumedEvent eventObj)
        {
            ToggleTutorialOverlay(_wasTutorialOverlayActiveBeforePause);
            ToggleTutorialInstructionContainer(_wasTutorialInstructionActiveBeforePause);
            ToggleTutorialNotificationContainer(_wasTutorialNotificationActiveBeforePause);

            _wasTutorialOverlayActiveBeforePause = false;
            _wasTutorialInstructionActiveBeforePause = false;
            _wasTutorialNotificationActiveBeforePause = false;
        }

        private void OnGameplayRestarted_TutorialUI(GameplayRestartedEvent eventObj)
        {
            StopAllCoroutines();

            ToggleTutorialOverlay(false);
            ToggleTutorialInstructionContainer(false);
            ToggleTutorialNotificationContainer(false);

            _tutorialInstructionBackground.gameObject.SetActive(false);

            _tutorialInstructionText.color = _originalTextColor;
            _tutInstructionBGMaterial.SetFloat(ProgressID, 0f);

            _wasTutorialOverlayActiveBeforePause = false;
            _wasTutorialNotificationActiveBeforePause = false;
        }

        private void OnTutorialContinued_TutorialUI(TutorialContinuedEvent eventbj)
        {
            ToggleTutorialNotificationContainer(false);
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
