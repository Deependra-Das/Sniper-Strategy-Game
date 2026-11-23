using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Tutorial;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SniperStrategyGame.UI
{
    public class TutorialWorldSpaceUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tutorialGlowText1;
        [SerializeField] private TMP_Text _tutorialGlowText2;
        [SerializeField] private float _charactersPerSecond = 30f;

        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            var services = GameManager.Instance.Services;
            _eventBusServiceObj = services.Get<EventBusService>();
        }

        private void Start()
        {
            SubscribeToEvents();
            ToggleTutorialGlowText1(true);
            ToggleTutorialGlowText2(true);
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
        }

        private void SetupTutorialGlowText(string textValue)
        {
            SetText(_tutorialGlowText1, textValue);
            SetText(_tutorialGlowText2, textValue);
            Type(_tutorialGlowText1);
            Type(_tutorialGlowText2);
        }

        private void ToggleTutorialGlowText1(bool value)
        {
            _tutorialGlowText1.gameObject.SetActive(value);
        }

        private void ToggleTutorialGlowText2(bool value)
        {
            _tutorialGlowText2.gameObject.SetActive(value);
        }

        private void SetText(TMP_Text target, string value)
        {
            if (target == null)
                return;

            target.text = value;
            target.ForceMeshUpdate();
            target.maxVisibleCharacters = 0;
        }

        private void Type(TMP_Text target)
        {
            if (target == null)
                return;

            StartCoroutine(TypeRoutine(target));
        }

        private void Erase(TMP_Text target)
        {
            if (target == null)
                return;

            StartCoroutine(EraseRoutine(target));
        }

        private IEnumerator TypeRoutine(TMP_Text target)
        {
            target.ForceMeshUpdate();

            int characterCount = target.textInfo.characterCount;
            target.maxVisibleCharacters = 0;
            float interval = 1f / _charactersPerSecond;
            float timer = 0f;
            int visibleCharacters = 0;

            while (visibleCharacters < characterCount)
            {
                timer += Time.deltaTime;
                int charactersToShow = Mathf.FloorToInt(timer / interval);

                if (charactersToShow > 0)
                {
                    visibleCharacters = Mathf.Min(visibleCharacters + charactersToShow, characterCount);
                    target.maxVisibleCharacters = visibleCharacters;
                    timer -= charactersToShow * interval;
                }

                yield return null;
            }
        }

        private IEnumerator EraseRoutine(TMP_Text target)
        {
            int visibleCharacters = target.maxVisibleCharacters;
            float interval = 1f / _charactersPerSecond;
            float timer = 0f;

            while (visibleCharacters > 0)
            {
                timer += Time.deltaTime;
                int charactersToRemove = Mathf.FloorToInt(timer / interval);

                if (charactersToRemove > 0)
                {
                    visibleCharacters = Mathf.Max(visibleCharacters - charactersToRemove, 0);
                    target.maxVisibleCharacters = visibleCharacters;
                    timer -= charactersToRemove * interval;
                }

                yield return null;
            }
        }

        private void OnTutorialStepCompletedTutorialGlowText(TutorialStepCompletedEvent eventObj)
        {
            Erase(_tutorialGlowText1);
            Erase(_tutorialGlowText2);
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();
        }
    }
}
