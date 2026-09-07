using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class UpdateTutorialInstructionEvent
    {
        public string Instruction { get; }
        public Sprite ButtonMapSprite { get; }

        public UpdateTutorialInstructionEvent(string instruction, Sprite buttonMapSprite)
        {
            Instruction = instruction;
            ButtonMapSprite = buttonMapSprite;
        }
    }
}
