using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Endpoint : Conductor, IPointerDownHandler, IPointerUpHandler {
    [HideInInspector] public ElectricComponent component;
    [SerializeField] bool canPullWire = true;
    [SerializeField] Vector3 wirePositionLocalOffset;
    public Wire wire = null;
    Wire wirePrefab;
    Plane wirePlane;
    bool isMovingWire = false;
    void Awake() {
        wirePrefab = Resources.Load<Wire>(Wire.PREFAB_PATH);
        wirePlane = new(transform.up, transform.position.y);
    }

    void Update() {
        if (isMovingWire && wire != null) {
            Vector3 worldMousePosition = GetWorldMousePosition();
            wire.EndPosition = worldMousePosition;
        }
    }

    public override Conductor[] GetConnectedConductors(Conductor from) {
        if (component == null) {
            Debug.LogError("Endpoint não registrado por um componente elétrico: " + name);
            return null;
        }

        Conductor[] positive = component.GetPoweredOutputEndpoints().Where(endpoint => endpoint != this).ToArray();
        Conductor[] negative = wire != null && wire != from ? new Conductor[] { wire } : new Conductor[0];

        return polarity switch {
            Polarity.POSITIVE => positive,
            Polarity.NEGATIVE => negative,
            _ => positive.Concat(negative).ToArray(),
        };
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!canPullWire) {
            return;
        }

        Vector3 worldMousePosition = GetWorldMousePosition();
        
        if (wire != null) {
            Destroy(wire);
        }

        wire = Instantiate(wirePrefab, transform.position, Quaternion.identity);
        wire.StartPosition = transform.TransformPoint(wirePositionLocalOffset);
        wire.EndPosition = worldMousePosition;
        isMovingWire = true;
        wire.StartConductor = this;
    }

    Vector3 GetWorldMousePosition() {
        return GetWorldMousePosition(out _);
    }

    Vector3 GetWorldMousePosition(out Ray ray) {
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!wirePlane.Raycast(ray, out float distance)) {
            Debug.LogError("Plano é paralelo ao raio, ou está atrás da câmera" + name);
            return Vector3.zero;
        }

        return ray.GetPoint(distance);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!isMovingWire) {
            return;
        }

        GetWorldMousePosition(out Ray ray);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
            return;
        }

        if (hit.collider == null) {
            return;
        }

        if (!hit.collider.gameObject.TryGetComponent(out Endpoint endPoint)) {
            return;
        }

        endPoint.wire = wire;
        wire.EndConductor = endPoint;
        isMovingWire = false;
    }
}