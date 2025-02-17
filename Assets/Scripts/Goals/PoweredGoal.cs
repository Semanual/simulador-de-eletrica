using UnityEngine;
[RequireComponent(typeof(ElectricComponent))]
public class PoweredGoal : BaseGoal {
    [SerializeField] bool shouldBePowered = true;
    [SerializeField] bool shouldBeShortCircuited = false;
    public override bool CheckGoal() {
        if (!TryGetComponent(out ElectricComponent component)) {
            return false;
        }

        return component.isPowered == shouldBePowered && component.isShortCircuited == shouldBeShortCircuited;
    }
}