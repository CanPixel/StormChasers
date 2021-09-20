using UnityEngine;

namespace KartGame.KartSystems {

    public class GameInput : BaseInput
    {
        public VehicleMovement movement;

        public override InputData GenerateInput() {
            return new InputData
            {
                Accelerate = movement.IsGassing() > 0,
                Brake = movement.IsBraking(),
                Drift = movement.IsDrifting(),
                TurnInput = movement.IsSteering()
            };
        }
    }
}
