using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
        Circuit updatedCircuit = GetCircuitFromStartingConductors(GetPoweredOutputEndpoints(), negativeTerminal, true);
        if (circuit != null) {
            // Limpe os estados dos componentes antigos
            circuit.Dispose(true);
        }

        if (updatedCircuit.electrics.Count == 0) {
            // Circuito aberto, não continue
            circuit = null;
            return;
        }

        circuit = updatedCircuit;
        circuit.receivingCurrent = circuit.electromotiveForce / circuit.resistance;
        circuit.UpdateValues();
    }
    Circuit GetCircuitFromStartingConductors(Conductor[] conductors, Conductor endHere, bool force = false) {
        if (conductors.Length == 0) {
            return new Circuit(AssociationType.SERIES, new());
        }
        
        Circuit circuit;
        if (conductors.Length == 1) {
            circuit = new Circuit(AssociationType.SERIES, new());
        } else {
            circuit = new Circuit(AssociationType.PARALLEL, new());
        }

        HashSet<Conductor> usedPaths = new() { };

        foreach (Conductor conductor in conductors) {
            Conductor nextConductor = conductor;
            HashSet<Conductor> path = new();
            List<IElectric> electricsFound = new();

            while (
                nextConductor != null 
                && nextConductor != endHere 
                && (force || !usedPaths.Contains(nextConductor))
            ) {
                path.Add(nextConductor);
                Conductor[] bifurcations = nextConductor.GetConnectedConductors().Where(connected => !path.Contains(connected)).ToArray();
                if (bifurcations.Length == 0) {
                    // Beco sem saída, passe pro próximo caminho
                    nextConductor = null;
                    break;
                }

                if (bifurcations.Length == 1) {
                    nextConductor = bifurcations[0];
                    if (nextConductor is Endpoint endpoint && !electricsFound.Contains(endpoint.component)) {
                        electricsFound.Add(endpoint.component);
                    }
                    continue;
                }

                // Se tiver mais de uma bifurcação, é porque é um nó de junção
                Circuit bifurcationCircuit = GetCircuitFromStartingConductors(bifurcations, endHere);

                if (bifurcationCircuit.electrics.Count == 0 && bifurcationCircuit.associationType == AssociationType.PARALLEL) {
                    // Beco sem saída, passe pro próximo caminho
                    nextConductor = null;
                    break;
                }
                
                if (bifurcationCircuit.associationType == AssociationType.SERIES) {
                    electricsFound.AddRange(bifurcationCircuit.electrics);
                    continue;
                }

                electricsFound.Add(bifurcationCircuit);
            }

            if (nextConductor == null) {
                continue;
            }

            if (force && nextConductor != endHere) {
                continue;
            }

            usedPaths.AddRange(path);
            if (circuit.associationType == AssociationType.SERIES) {
                circuit.electrics.AddRange(electricsFound);
                return circuit;
            }

            if (circuit.electrics.Count == 1) {
                circuit.electrics.Add(circuit.electrics[0]);
                continue;
            }
            Circuit parallelLine = new(AssociationType.SERIES, circuit.electrics);
            circuit.electrics.Add(parallelLine);
        }

        return circuit;
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