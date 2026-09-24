using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Tutorial;
using TMPro;
using UnityEngine;

namespace SniperStrategyGame.UI
{
    public class TutorialWorldSpaceUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tutorialGlowText1;
        [SerializeField] private TMP_Text _tutorialGlowText2;

        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            var services = GameManager.Instance.Services;
            _eventBusServiceObj = services.Get<EventBusService>();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBusServiceObj.Subscribe<TutorialStepStartedEvent>(OnTutorialStepStartedTutorialGlowText);
            _eventBusServiceObj.Subscribe<TutorialStepCompletedEvent>(OnTutorialStepCompletedTutorialGlowText);
        }

        private void UnsubscribeToEvents()
        {
            _eventBusServiceObj.Unsubscribe<TutorialStepStartedEvent>(OnTutorialStepStartedTutorialGlowText);
            _eventBusServiceObj.Unsubscribe<TutorialStepCompletedEvent>(OnTutorialStepCompletedTutorialGlowText);
        }

        private void OnTutorialStepStartedTutorialGlowText(TutorialStepStartedEvent eventObj)
        {
            TutorialStepData tutorialStepData = eventObj.TutorialStepData;
            SetupTutorialGlowText(tutorialStepData.tutorialActionText.ToString());
            ToggleTutorialGlowText1(true);
            ToggleTutorialGlowText2(true);
        }

        private void SetupTutorialGlowText(string textValue)
        {
            _tutorialGlowText1.text = textValue;
            _tutorialGlowText2.text = textValue;
        }

        private void ToggleTutorialGlowText1(bool value)
        {
            _tutorialGlowText1.gameObject.SetActive(value);
        }

        private void ToggleTutorialGlowText2(bool value)
        {
            _tutorialGlowText2.gameObject.SetActive(value);
        }

        private void OnTutorialStepCompletedTutorialGlowText(TutorialStepCompletedEvent eventObj)
        {
            ToggleTutorialGlowText1(false);
            ToggleTutorialGlowText2(false);
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();
        }
    }
}
