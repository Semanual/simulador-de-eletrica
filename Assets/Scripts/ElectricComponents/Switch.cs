using UnityEngine;
using UnityEngine.EventSystems;
public class Switch : ElectricComponent, IPointerDownHandler {
    [Endpoint(Polarity.NONE)]
    [SerializeField] Endpoint[] terminals;
    [SerializeField] Animator animator;
    [SerializeField] bool isOn;
    protected override void Awake() {
        base.Awake();
        animator.SetBool("isOn", isOn);
    }

    public override Endpoint[] GetPoweredOutputEndpoints() {
        if (!isOn) {
            return new Endpoint[0];
        }

        return terminals;
    }

    public void OnPointerDown(PointerEventData eventData) {
        isOn = !isOn;
        animator.SetBool("isOn", isOn);
    }
}