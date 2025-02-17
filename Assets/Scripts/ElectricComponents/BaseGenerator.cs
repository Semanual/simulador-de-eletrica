using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public abstract class BaseGenerator : ElectricComponent {
    readonly protected HashSet<ElectricComponent> powering = new();
    [SerializeField] bool isMainGenerator = true;
    public float electromotiveForce = 10;
    public abstract void Refresh();
    protected virtual void Start() {
        if (isMainGenerator) {
            GeneratorManager.Singleton.Register(this);
        }
    }

    protected Circuit GetCircuitFromStartingConductors(Conductor[] conductors, Conductor origin, Conductor endHere, out Conductor commonConductor, bool force = false, HashSet<Conductor> pastConductors = null) {        
        commonConductor = endHere;

        if (pastConductors == null) {
            pastConductors = new();
        } else {
            pastConductors = Enumerable.ToHashSet(pastConductors);
        }

        pastConductors.AddRange(conductors);
        

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

                Conductor[] bifurcationsWithUsedPaths = nextConductor.GetConnectedConductors().Where(
                                                connected => 
                                                    !path.Contains(connected)
                                                    && connected != origin
                                                    && !pastConductors.Contains(connected)
                                            ).ToArray();
                if (bifurcationsWithUsedPaths.Length == 0) {
                    // Beco sem saída, passe pro próximo caminho
                    nextConductor = null;
                    break;
                }

                Conductor[] bifurcations = bifurcationsWithUsedPaths.Where(bifurcation => !usedPaths.Contains(bifurcation)).ToArray();

                if (bifurcationsWithUsedPaths.Length != bifurcations.Length) {
                    // A saída é um caminho já usado, então esse é o final da bifurcação+
                    IElectric lastElectricFromBifurcation = electricsFound.Last();
                    //nextConductor = bifurcations[0];
                    foreach (IElectric electric in circuit.electrics) {
                        if (electric is not Circuit subcircuit) {
                            continue;
                        }

                        int index = subcircuit.electrics.FindIndex(x => x == lastElectricFromBifurcation);
                        if (index == -1) {
                            continue;
                        }
                        subcircuit.electrics.RemoveRange(index, subcircuit.electrics.Count - index);
                    }
                    electricsFound.Remove(lastElectricFromBifurcation);
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
                Circuit bifurcationCircuit = GetCircuitFromStartingConductors(bifurcations, nextConductor, endHere, out nextConductor, pastConductors: pastConductors);

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

            commonConductor = nextConductor;

            if (force && nextConductor != endHere) {
                continue;
            }

            usedPaths.AddRange(path);
            if (circuit.associationType == AssociationType.SERIES) {
                circuit.electrics.AddRange(electricsFound);
                return circuit;
            }

            if (electricsFound.Count == 1) {
                circuit.electrics.Add(electricsFound[0]);
                continue;
            }
            Circuit parallelLine = new(AssociationType.SERIES, electricsFound);
            circuit.electrics.Add(parallelLine);
        }

        return Cleanup(circuit);
    }

    Circuit Cleanup(Circuit circuit) {
        if (circuit.associationType == AssociationType.SERIES) {
            return circuit;
        }

        if (circuit.electrics.Count == 1) {
            if (circuit.electrics[0] is Circuit) {
                return Cleanup(circuit.electrics[0] as Circuit);
            } else {
                return new Circuit(AssociationType.SERIES, new List<IElectric> {circuit.electrics[0]});
            }
        }

        return circuit;
    }
}