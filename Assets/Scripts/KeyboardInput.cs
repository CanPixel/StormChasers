using UnityEngine;

namespace KartGame.KartSystems {

    public class KeyboardInput : BaseInput
    {
        public VehicleMovement movement;

        public string TurnInputName = "Horizontal";
        public string AccelerateButtonName = "Accelerate";
        public string BrakeButtonName = "Brake";

        public override InputData GenerateInput() {
            return new InputData
            {
                Accelerate = movement.IsGassing() > 0,
                Brake = movement.IsBraking(),
                TurnInput = movement.IsSteering()
            };
        }
    }
}
