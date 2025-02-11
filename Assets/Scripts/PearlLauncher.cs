using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PearlLauncher : MonoBehaviour
{
    [SerializeField] private ShootProjection _projection;
    [SerializeField] private PearlProjectile _ghostProjectile;

    [SerializeField] private List<PearlProjectile> _availablePearls;
    [SerializeField] private Transform _currentPearlParent;
    [SerializeField] private Transform _nextPearlParent;

    [SerializeField] private PearlPlanet _planetParent;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _shootAudioClip;

    [SerializeField] private float mobileSensitivity = 1f;
    [SerializeField] private float pcSensitivity = 1f;

    [SerializeField] private float _force = 20;
    [SerializeField] private Transform _launchPivot;
    [SerializeField] private float _rotateSpeed = 30;

    private int _shootIndex = 0;

    private PearlProjectile _currentProjectile;
    private PearlProjectile _nextProjectile;

    private bool _canShoot = false;
    private bool _allowShoot => _shootIndex < _availablePearls.Count;

    private Vector2 lastTouchPosition;
    private Vector2 lastMousePosition;

    private void Start()
    {
        _shootIndex = 0;
        _canShoot = true;
        SetPearlProjectiles(firstTime: true);
    }

    private void Update()
    {
        HandleControls();
        if (_allowShoot && _currentProjectile != null)
        {
            _projection.SimulateTrajectory(_ghostProjectile, _currentPearlParent.position, _currentPearlParent.forward * _force);
        }
        else
        {
            _projection.CancelSimulateTrajectory();
        }
    }

    private void Shoot()
    {
        if (_allowShoot && _currentProjectile != null)
        {
            Debug.Log($"Shooting {_currentProjectile.name}, _shootIndex: {_shootIndex}");

            _currentProjectile.Fire(_currentPearlParent.forward * _force, false, _planetParent);
            _shootIndex++;

            TransitionToNextProjectile();

            _audioSource.clip = _shootAudioClip;
            _audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Can't shoot: No projectiles left!");
        }
    }

    private void SetPearlProjectiles(bool firstTime)
    {
        if (!_allowShoot) return;

        bool nextPearlAvailable = _shootIndex + 1 < _availablePearls.Count;
        Debug.Log($"Setting pearl projectiles. Next available: {nextPearlAvailable}");

        if (firstTime)
        {
            _currentProjectile = Instantiate(_availablePearls[_shootIndex], _currentPearlParent.position, _currentPearlParent.rotation);
            _currentProjectile.transform.localScale = Vector3.zero;
            _currentProjectile.SetAsKinematic(true);
            _currentProjectile.transform.DOScale(Vector3.one, 0.3f);
        }

        if (nextPearlAvailable)
        {
            _nextProjectile = Instantiate(_availablePearls[_shootIndex + 1], _nextPearlParent.position, _nextPearlParent.rotation);
            _nextProjectile.transform.localScale = Vector3.zero;
            _nextProjectile.SetAsKinematic(true);
            _nextProjectile.transform.DOScale(Vector3.one, 0.3f);
        }
        else
        {
            _nextProjectile = null;
        }
    }

    private void TransitionToNextProjectile()
    {
        if (_nextProjectile != null)
        {
            _canShoot = false;
            _projection.SetIsSimulating(false);
            _nextProjectile.transform.DOMove(_currentPearlParent.position, 0.3f).OnComplete(() =>
            {
                _currentProjectile = _nextProjectile;
                _nextProjectile = null;
                SetPearlProjectiles(firstTime: false);
                _canShoot = true;
                _projection.SetIsSimulating(true);
            });
        }
        else
        {
            _currentProjectile = null;
        }
    }
    private void HandleControls()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 touchDelta = touch.position - lastTouchPosition;

                _launchPivot.Rotate(Vector3.right * -touchDelta.y * mobileSensitivity * _rotateSpeed * Time.deltaTime);
                _launchPivot.Rotate(Vector3.up * touchDelta.x * mobileSensitivity * _rotateSpeed * Time.deltaTime);

                lastTouchPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                Debug.Log("Finger released!");
                if (_canShoot)
                    Shoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePosition;

                _launchPivot.Rotate(Vector3.right * -mouseDelta.y * pcSensitivity * _rotateSpeed * Time.deltaTime);
                _launchPivot.Rotate(Vector3.up * mouseDelta.x * pcSensitivity * _rotateSpeed * Time.deltaTime);

                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Mouse button released!");
                if (_canShoot)
                    Shoot();
            }
        }
    }


}
