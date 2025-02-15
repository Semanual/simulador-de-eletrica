using UnityEngine;
public enum Polarity {
    NONE,
    POSITIVE,
    NEGATIVE,
};

public abstract class Conductor : MonoBehaviour {
    public abstract Conductor[] GetConnectedConductors(Conductor from);
    public Polarity polarity;
}