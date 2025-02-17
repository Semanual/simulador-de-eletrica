using UnityEngine;
public class Parallelizer : ElectricComponent {
    [Endpoint(Polarity.NONE)]
    [SerializeField] Endpoint[] terminals;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return terminals;
    }
}