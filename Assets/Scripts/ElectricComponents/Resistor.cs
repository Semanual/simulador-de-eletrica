using UnityEngine;
public class Resistor : ElectricComponent {
    [Endpoint(Polarity.POSITIVE)]
    [SerializeField] Endpoint positiveTerminal;
    [Endpoint(Polarity.NEGATIVE)]
    [SerializeField] Endpoint negativeTerminal;
    [SerializeField] Animator animator;
    public override bool HasResistance => true;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { negativeTerminal };
    }

    public override void SetPowered(bool isPowered, BaseGenerator by) {
        base.SetPowered(isPowered, by);
        Debug.Log(isPowered);
        Debug.Log(poweredBy.ToDebugString());
        bool isStillPowered = poweredBy.Count > 0;
        animator.SetBool("isPowered", isStillPowered);
    }
}