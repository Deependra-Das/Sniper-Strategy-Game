using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;

namespace SniperStrategyGame.UI
{
    public class TutorialUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _tutorialInstructionContainer;
        [SerializeField] private TMP_Text _tutorialInstructionText;
        [SerializeField] private GameObject _tutorialNotificationContainer;
        [SerializeField] private TMP_Text _tutorialNotificationText;
        [SerializeField] private Button _continueTutorialButton;
        [SerializeField] private TMP_Text _continueTutorialButtonText;

        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            var services = GameManager.Instance.Services;

            _eventBusServiceObj = services.Get<EventBusService>();
        }

        private void Start()
        {
            ToggleTutorialNotificationContainer(false);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _continueTutorialButton.onClick.AddListener(OnContinueTutorialClicked);
            _eventBusServiceObj.Subscribe<TutorialStepStartedEvent>(OnTutorialStepStarted);
            _eventBusServiceObj.Subscribe<TutorialGroupCompletedEvent>(OnTutorialGroupCompleted);
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

        private void OnTutorialStepStarted(TutorialStepStartedEvent eventObj)
        {
            _tutorialInstructionText.text = eventObj.Instruction;
        }

        private void OnDestroy()
        {
            _continueTutorialButton.onClick.RemoveListener(OnContinueTutorialClicked);
        }
    }
}
