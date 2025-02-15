using UnityEngine;
public class Resistor : ElectricComponent {
    [Endpoint(Polarity.POSITIVE)]
    [SerializeField] Endpoint positiveTerminal;
    [Endpoint(Polarity.NEGATIVE)]
    [SerializeField] Endpoint negativeTerminal;
    [SerializeField] Animator animator;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { negativeTerminal };
    }

    public override void SetPowered(bool isPowered) {
        animator.SetBool("isPowered", isPowered);
    }
}