using SniperStrategyGame.Tutorial;
using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class TutorialStepStartedEvent
    {
        public readonly TutorialActionEnum TutorialActionRequired;
        public readonly string Instruction;
        public readonly Sprite InstructionButtonMapSprite;
        public readonly bool ShowOverlay;

        public TutorialStepStartedEvent(TutorialActionEnum tutorialActionRequired, string instruction, Sprite instructionButtonMapSprite, bool showOverlay)
        {
            TutorialActionRequired = tutorialActionRequired;
            Instruction = instruction;
            InstructionButtonMapSprite = instructionButtonMapSprite;
            ShowOverlay = showOverlay;
        }
    }
}
