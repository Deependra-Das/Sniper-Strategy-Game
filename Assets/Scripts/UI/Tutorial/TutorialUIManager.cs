using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Tutorial;

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

        [SerializeField] private GameObject _tutorialInstructionContainer;
        [SerializeField] private TMP_Text _tutorialInstructionText;
        [SerializeField] private GameObject _tutorialNotificationContainer;
        [SerializeField] private TMP_Text _tutorialNotificationText;
        [SerializeField] private Button _continueTutorialButton;
        [SerializeField] private TMP_Text _continueTutorialButtonText;

        private EventBusService _eventBusServiceObj;

        private Material _overlayMaterial;

        private static readonly int HoleCenterID = Shader.PropertyToID("_HoleCenter");
        private static readonly int HoleSizeID = Shader.PropertyToID("_HoleSize");

        private void Awake()
        {
            var services = GameManager.Instance.Services;

            _eventBusServiceObj = services.Get<EventBusService>();
        }

        private void Start()
        {
            _overlayMaterial = new Material(_tutorialOverlayMaterial);
            _tutorialOverlayImage.material = _overlayMaterial;
            ToggleTutorialOverlay(false);
            ToggleTutorialInstructionContainer(false);
            ToggleTutorialNotificationContainer(false);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _continueTutorialButton.onClick.AddListener(OnContinueTutorialClicked);
            _eventBusServiceObj.Subscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Subscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);
            _eventBusServiceObj.Subscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);
        }

        private void UnsubscribeToEvents()
        {
            _continueTutorialButton.onClick.RemoveListener(OnContinueTutorialClicked);
            _eventBusServiceObj.Unsubscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Unsubscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);
            _eventBusServiceObj.Unsubscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);

        }

        private void OnContinueTutorialClicked()
        {
            _tutorialNotificationContainer.SetActive(false);
            _eventBusServiceObj.Publish(new ContinueTutorialEvent());
        }

        private void OnTutorialGroupCompleted(TutorialGroupCompletedEvent eventObj)
        {
            _tutorialNotificationText.text = $"{eventObj.CompletedGroupName} Tutorial completed!";
            _continueTutorialButtonText.text = $"Continue to {eventObj.NextGroupName} tutorial.";
            ToggleTutorialNotificationContainer(true);
        }

        private void ToggleTutorialNotificationContainer(bool value)
        {
            _tutorialNotificationContainer.SetActive(value);
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
            _tutorialInstructionText.text = eventObj.Instruction;
            ToggleTutorialInstructionContainer(true);
            ToggleTutorialOverlay(false);

            if (eventObj.ShowOverlay)
            {
                SetUpTutorialOverlay(eventObj.TutorialActionRequired);
                ToggleTutorialOverlay(true);
            }
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

        private void OnTutorialStepCompleted(TutorialStepCompletedEvent eventObj)
        {
            ToggleTutorialOverlay(false);
            ToggleTutorialInstructionContainer(false);
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();

            if (_overlayMaterial != null)
            {
                Destroy(_overlayMaterial);
                _overlayMaterial = null;
            }
        }
    }
}
