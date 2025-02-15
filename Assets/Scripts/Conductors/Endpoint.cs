using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Endpoint : Conductor, IPointerDownHandler, IPointerUpHandler {
    [HideInInspector] public ElectricComponent component;
    [SerializeField] bool canPullWire = true;
    [SerializeField] Vector3 wirePositionLocalOffset;
    [SerializeField] InputActionReference pointInput;
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

    public override Conductor[] GetConnectedConductors(Conductor from = null) {
        if (component == null) {
            Debug.LogError("Endpoint não registrado por um componente elétrico: " + name);
            return null;
        }

        Conductor[] positive = component.GetPoweredOutputEndpoints().Where(endpoint => endpoint != this).ToArray();
        Conductor[] negative = wire != null && wire != from ? new Conductor[] { wire } : new Conductor[0];
        Debug.Log(polarity);
        Debug.Log(positive.ToDebugString());
        Debug.Log(negative.ToDebugString());

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
            Destroy(wire.gameObject);
        }

        wire = Instantiate(wirePrefab);
        wire.StartPosition = GetConnectionPoint();
        wire.EndPosition = worldMousePosition;
        isMovingWire = true;
        wire.StartConductor = this;
    }

    Vector3 GetWorldMousePosition() {
        return GetWorldMousePosition(out _);
    }

    Vector3 GetWorldMousePosition(out Ray ray) {
        ray = Camera.main.ScreenPointToRay(pointInput.action.ReadValue<Vector2>());

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
            Destroy(wire.gameObject);
            wire = null;
            return;
        }

        if (hit.collider == null) {
            Destroy(wire.gameObject);
            wire = null;
            return;
        }

        if (!hit.collider.gameObject.TryGetComponent(out Endpoint endpoint)) {
            Destroy(wire.gameObject);
            wire = null;
            return;
        }

        if (endpoint == this) {
            Destroy(wire.gameObject);
            wire = null;
            return;
        }

        endpoint.wire = wire;
        wire.EndConductor = endpoint;
        wire.EndPosition = endpoint.GetConnectionPoint();
        isMovingWire = false;
    }
    
    public Vector3 GetConnectionPoint() {
        return transform.TransformPoint(wirePositionLocalOffset);
    }
}