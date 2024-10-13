using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnTouch = UnityEngine.InputSystem.EnhancedTouch;

public class ObjectSpawn : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    private ARRaycastManager aRRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject spawnedObject;
    private Vector3 initialScale;
    private float initialDistanceBetweenFingers;
    private Vector3 initialScaleAtPinch;
    private bool isRotating;

    [SerializeField]
    private Button redButton;
    [SerializeField]
    private Button greenButton;
    [SerializeField]
    private Button blueButton;

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
    }

    private void Start()
    {
        redButton.onClick.AddListener(() => ChangeObjectColor(Color.red));
        greenButton.onClick.AddListener(() => ChangeObjectColor(Color.green));
        blueButton.onClick.AddListener(() => ChangeObjectColor(Color.blue));
    }

    private void OnEnable()
    {
        EnTouch.EnhancedTouchSupport.Enable();
        EnTouch.TouchSimulation.Enable();
        EnTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnTouch.Touch.onFingerDown -= FingerDown;
        EnTouch.TouchSimulation.Disable();
        EnTouch.EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleMouseInput();
        HandleTouchInput();
    }

    private void HandleMouseInput()
    {
        if (spawnedObject == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            spawnedObject.transform.localScale += Vector3.one * scrollInput;
        }

        if (Input.GetMouseButton(0))
        {
            float rotationX = Input.GetAxis("Mouse X") * 200f * Time.deltaTime;
            float rotationY = Input.GetAxis("Mouse Y") * 200f * Time.deltaTime;

            spawnedObject.transform.Rotate(Vector3.up, -rotationX, Space.World);
            spawnedObject.transform.Rotate(Vector3.right, rotationY, Space.World);
        }
    }

    private void HandleTouchInput()
    {
        if (spawnedObject == null) return;

        if (EnTouch.Touch.activeTouches.Count == 2)
        {
            EnTouch.Touch finger1 = EnTouch.Touch.activeTouches[0];
            EnTouch.Touch finger2 = EnTouch.Touch.activeTouches[1];

            if (finger1.phase == UnityEngine.InputSystem.TouchPhase.Began || finger2.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                initialDistanceBetweenFingers = Vector2.Distance(finger1.screenPosition, finger2.screenPosition);
                initialScaleAtPinch = spawnedObject.transform.localScale;
            }
            else if (finger1.phase == UnityEngine.InputSystem.TouchPhase.Moved || finger2.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                float currentDistanceBetweenFingers = Vector2.Distance(finger1.screenPosition, finger2.screenPosition);
                float scaleFactor = currentDistanceBetweenFingers / initialDistanceBetweenFingers;
                spawnedObject.transform.localScale = initialScaleAtPinch * scaleFactor;
            }
        }

        if (EnTouch.Touch.activeTouches.Count == 1)
        {
            EnTouch.Touch finger = EnTouch.Touch.activeTouches[0];

            if (finger.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                isRotating = true;
            }
            else if (finger.phase == UnityEngine.InputSystem.TouchPhase.Moved && isRotating)
            {
                float rotationX = finger.delta.x * 0.5f;
                float rotationY = finger.delta.y * 0.5f;

                spawnedObject.transform.Rotate(Vector3.up, -rotationX, Space.World);
                spawnedObject.transform.Rotate(Vector3.right, rotationY, Space.World);
            }
            else if (finger.phase == UnityEngine.InputSystem.TouchPhase.Ended || finger.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                isRotating = false;
            }
        }
    }

    private void FingerDown(EnTouch.Finger finger)
    {
        if (finger.index != 0) return;

        if (spawnedObject != null) return;

        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            Pose pose = hit.pose;

            Vector3 adjustedPosition = pose.position;
            adjustedPosition.y += 0.3f;

            spawnedObject = Instantiate(prefab, adjustedPosition, pose.rotation);
            initialScale = spawnedObject.transform.localScale;
        }
    }

    public void ChangeObjectColor(Color newColor)
    {
        if (spawnedObject != null)
        {
            var renderer = spawnedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = newColor;
            }
        }
    }
}
