using UnityEngine;
public class Diode : ElectricComponent {
    [Endpoint(Polarity.POSITIVE)]
    [SerializeField] Endpoint positiveTerminal;
    [Endpoint(Polarity.NEGATIVE)]
    [SerializeField] Endpoint negativeTerminal;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem shortCircuitParticles;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { negativeTerminal };
    }

    public override void SetPowered(bool isPowered) {
        if (!animator) {
            return;
        }
        animator.SetBool("isPowered", isPowered);
    }

    public override void ShortCircuit(bool isShortCircuited) {
        base.ShortCircuit(isShortCircuited);
        if (shortCircuitParticles != null && isShortCircuited) {
            shortCircuitParticles.Play();
        } else if (shortCircuitParticles != null && !isShortCircuited) {
            shortCircuitParticles.Stop();
        }
    }
}