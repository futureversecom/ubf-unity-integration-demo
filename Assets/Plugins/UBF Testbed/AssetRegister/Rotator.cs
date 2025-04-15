using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float _dragMultiplier;
    
    private Vector3 _currentMousePosition;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _currentMousePosition = Input.mousePosition;
            return;
        }
        
        if (!Input.GetMouseButton(1))
        {
            return;
        }

        var delta = _currentMousePosition - Input.mousePosition;
        var newY = transform.eulerAngles.y + delta.x * _dragMultiplier;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, newY, transform.eulerAngles.z);
        _currentMousePosition = Input.mousePosition;
    }
}
