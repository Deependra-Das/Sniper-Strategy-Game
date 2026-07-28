using SniperStrategyGame.Main;
using SniperStrategyGame.SceneLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectionButton : MonoBehaviour
{
    [SerializeField] private Button _stageButton;
    [SerializeField] private TMP_Text _stageLabel;

    private int _stageIndex;
    private SceneNameEnum _sceneName;

    public void Initialize(int stageIndex, SceneNameEnum sceneName)
    {
        _stageIndex = stageIndex;
        _sceneName = sceneName;
        _stageLabel.text = $"Stage {_stageIndex + 1}";
        _stageButton.onClick.AddListener(OnStageButtonClickedLoadStage);
    }

    private void OnStageButtonClickedLoadStage()
    {
        GameManager.Instance.Services.Get<SceneLoaderService>().LoadScene(_sceneName);
    }

    private void OnDestroy()
    {
        _stageButton.onClick.RemoveListener(OnStageButtonClickedLoadStage);
    }
}
