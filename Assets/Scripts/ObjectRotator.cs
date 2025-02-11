using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _direction;
    

    private void Update()
    {
        transform.Rotate(_direction * Time.deltaTime * _speed);    
    }
}
