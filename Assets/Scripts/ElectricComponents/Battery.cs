using TMPro;
using UnityEngine;
public class Battery : BaseGenerator {
    // Em uma bateria, a saída de energia (representada por terminal negativo no código) 
    // é considerada positiva, e a entrada de energia (representada por terminal positivo no código)
    // é considerada negativa. Por isso, a polaridade do terminal negativo é definida como 
    // positiva e vice versa.
    [Endpoint(Polarity.NEGATIVE)]
    [SerializeField] Endpoint positiveTerminal;
    [Endpoint(Polarity.POSITIVE)]
    [SerializeField] Endpoint negativeTerminal;
    [SerializeField] ParticleSystem shortCircuitParticles;
    [SerializeField] TextMeshPro tensionDisplay;
    [SerializeField] TextMeshPro currentDisplay;
    [SerializeField] int displayMaxLength = 5;

    protected override void Awake() {
        base.Awake();
        UpdateTensionDisplay();
    }

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { positiveTerminal };
    }

    public override void Refresh() {
        Circuit updatedCircuit = GetCircuitFromStartingConductors(GetPoweredOutputEndpoints(), positiveTerminal, negativeTerminal, out _, true);
        Debug.Log(updatedCircuit.ToDebugString());
        // Limpe os estados dos componentes antigos
        circuit?.Dispose(true);

        if (updatedCircuit.electrics.Count == 0) {
            // Circuito aberto, não continue
            circuit = null;
            return;
        }

        circuit = updatedCircuit;

        circuit.receivingCurrent = circuit.electromotiveForce / circuit.resistance;

        circuit.UpdateValues();
    }

    public override void ShortCircuit(bool isShortCircuited) {
        if (isShortCircuited) {
            shortCircuitParticles.Play();
        } else {
            shortCircuitParticles.Stop();
        }
    }

    public override void UpdateValues() {
        base.UpdateValues();
        UpdateTensionDisplay();
    }

    void UpdateTensionDisplay() {
        if (tensionDisplay) {
            string str = electromotiveForce.ToString();
            tensionDisplay.text = str.Length > displayMaxLength ? str[..displayMaxLength] : str;
        }

        if (currentDisplay) {
            string str = receivingCurrent.ToString();
            currentDisplay.text = str.Length > displayMaxLength ? str[..displayMaxLength] : str;
        }
    }
}