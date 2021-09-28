using UnityEngine;

namespace KartGame.KartSystems {

    public class GameInput : BaseInput
    {
        public CarMovement movement;

        public override InputData GenerateInput() {
            return new InputData
            {
                AccelerateInput = movement.IsGassing(),
                Brake = movement.IsBraking(),
                Drift = movement.IsDrifting(),
                TurnInput = movement.IsSteering()
            };
        }
    }
}
