using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class TutorialStepStartedEvent
    {
        public readonly string Instruction;
        public readonly Sprite InstructionButtonMapSprite;

        public TutorialStepStartedEvent(string instruction, Sprite instructionButtonMapSprite)
        {
            Instruction = instruction;
            InstructionButtonMapSprite = instructionButtonMapSprite;
        }
    }
}
