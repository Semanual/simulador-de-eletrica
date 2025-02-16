using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeCameraOnClick : MonoBehaviour, IPointerDownHandler {
    [SerializeField] CinemachineCamera changeToThisCamera;
    [SerializeField] bool disableCurrentOnClick = false;
    CinemachineBrain brain;
    void Awake() {
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (disableCurrentOnClick) {
            CinemachineCamera currentCamera = brain.ActiveVirtualCamera as CinemachineCamera;
            currentCamera.gameObject.SetActive(false);
        }

        changeToThisCamera.gameObject.SetActive(true);
    }
}
