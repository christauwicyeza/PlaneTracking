using System.Collections.Generic;
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

    [SerializeField]
    private Button redButton;
    [SerializeField]
    private Button greenButton;
    [SerializeField]
    private Button blueButton;

    private Vector3 initialScale;

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
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            if (spawnedObject == null && aRRaycastManager.Raycast(mousePosition, hits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = hits[0];
                Pose pose = hit.pose;

                Vector3 adjustedPosition = pose.position;
                adjustedPosition.y += 0.3f;

                spawnedObject = Instantiate(prefab, adjustedPosition, pose.rotation);
                initialScale = spawnedObject.transform.localScale;
            }
        }

        if (spawnedObject != null)
        {
            if (Input.GetMouseButton(0))
            {
                float rotationX = Input.GetAxis("Mouse X") * 200f * Time.deltaTime;
                float rotationY = Input.GetAxis("Mouse Y") * 200f * Time.deltaTime;

                spawnedObject.transform.Rotate(Vector3.up, -rotationX, Space.World);
                spawnedObject.transform.Rotate(Vector3.right, rotationY, Space.World);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                Vector3 newScale = spawnedObject.transform.localScale + Vector3.one * scroll;
                spawnedObject.transform.localScale = Vector3.Max(newScale, initialScale * 0.1f);
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
