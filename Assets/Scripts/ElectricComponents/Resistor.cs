using UnityEngine;
public class Resistor : ElectricComponent {
    [SerializeField] Endpoint positiveTerminal;
    [SerializeField] Endpoint negativeTerminal;
    [SerializeField] Animator animator;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { negativeTerminal };
    }

    public override void SetPowered(bool isPowered) {
        animator.SetBool("isPowered", isPowered);
    }
}