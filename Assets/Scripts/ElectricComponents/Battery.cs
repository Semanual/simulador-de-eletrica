using System.Collections.Generic;
using System.Linq;
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

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { positiveTerminal };
    }

    public override void Refresh() {
        bool isPowered = false;
        HashSet<Conductor> usedConductors = new();
        Queue<Conductor> conductorsToVisit = new();

        if (positiveTerminal != null) {
            conductorsToVisit.Enqueue(positiveTerminal);
            usedConductors.Add(positiveTerminal);
        }

        while (conductorsToVisit.Count > 0) {
            Conductor conductor = conductorsToVisit.Dequeue();
            Debug.Log(conductor);
            Debug.Log($"{conductor.transform.parent?.name}: {conductor.name}");
            Conductor[] connectedConductors = conductor.GetConnectedConductors();
            foreach (Conductor connected in connectedConductors) {
                if (usedConductors.Contains(connected)) {
                    continue;
                }

                Debug.Log("We should visit " + connected?.name);
                conductorsToVisit.Enqueue(connected);
                usedConductors.Add(connected);
            }

            if (conductor == negativeTerminal) {
                Debug.Log("ESTOU DE VOLTA!");
                isPowered = true;
                break;
            }
        }

        bool hasResistance = false;
        Debug.Log("Ligado? " + isPowered);
        HashSet<ElectricComponent> componentsInsideCircuit = new();
        foreach (Conductor conductor in usedConductors) {
            if (conductor is not Endpoint endpoint) {
                continue;
            }

            componentsInsideCircuit.Add(endpoint.component);

            if (isPowered && !powering.Contains(endpoint.component)) {
                powering.Add(endpoint.component);
                endpoint.component.SetPowered(isPowered, this);
            } else if (!isPowered && powering.Contains(endpoint.component)) {
                powering.Remove(endpoint.component);
                endpoint.component.SetPowered(isPowered, this);
            }

            if (endpoint.component.HasResistance) {
                hasResistance = true;
            }
            Debug.Log("Definindo isPowered de " + endpoint.component.name + " como " + isPowered);
        }

        foreach (ElectricComponent componentOutsideCircuit in powering.Where(x => !componentsInsideCircuit.Contains(x))) {
            componentOutsideCircuit.SetPowered(false, this);
            powering.Remove(componentOutsideCircuit);
        }

        ShortCircuit(!hasResistance && isPowered);
    }

    public override void ShortCircuit(bool isShortCircuited) {
        if (isShortCircuited) {
            shortCircuitParticles.Play();
        } else {
            shortCircuitParticles.Stop();
        }
    }
}