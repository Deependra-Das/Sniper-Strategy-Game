using SniperStrategyGame.SceneLoader;
using UnityEngine;

public class StageSelectionUIManager : MonoBehaviour
{
    [SerializeField] private StageSelectionButton _stageSelectionButtonPrefab;
    [SerializeField] private Transform _stageSelectionButtonContainer;
    [SerializeField] private SceneNameEnum[] _gameplayScenes;

    private void Start()
    {
        CreateStageButtons();
    }

    private void CreateStageButtons()
    {
        for (int stageIndex = 0; stageIndex < _gameplayScenes.Length; stageIndex++)
        {
            StageSelectionButton stageSelectionButton = Instantiate(_stageSelectionButtonPrefab, _stageSelectionButtonContainer);
            stageSelectionButton.Initialize(stageIndex, _gameplayScenes[stageIndex]);
        }
    }
}
