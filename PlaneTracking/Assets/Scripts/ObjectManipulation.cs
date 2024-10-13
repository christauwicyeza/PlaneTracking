using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulation : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    private GameObject spawnedObject;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnObjectAtMousePosition();
        }
    }

    private void SpawnObjectAtMousePosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        if (spawnedObject == null)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                spawnedObject = Instantiate(prefab, hit.point, Quaternion.identity);
            }
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
