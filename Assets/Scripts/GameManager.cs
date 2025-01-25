using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _levelGroundPrefab;
    [SerializeField] private Transform _playerTr;
    [SerializeField] private Volume _volume;
    [SerializeField] private float _yDistanceToTravel;
    [SerializeField] private Color[] _levelColors;
    private ColorAdjustments _colorAdjustments;
    private Vector3 _checkpoint = new(0.0f, 0.5f, 0.0f);
    private float _currentDistanceTraveled = 0.0f;
    private int _levelID = 1;

    private void Start()
    {
        _volume.profile.TryGet(out ColorAdjustments colorAdjustments);
        colorAdjustments.colorFilter.value = _levelColors[_levelID - 1];
    }
    private void Update()
    {
        _currentDistanceTraveled = _playerTr.position.y;
        if (_currentDistanceTraveled >= _yDistanceToTravel * _levelID)
        {
            _levelID++;
            _checkpoint = _playerTr.position;
            _checkpoint.x = 0.0f;
            _checkpoint.z = 0.0f;
            InitializeLevel(_checkpoint);
            _volume.profile.TryGet(out ColorAdjustments colorAdjustments);
            colorAdjustments.colorFilter.value = _levelColors[_levelID - 1];
        }

        if (_playerTr.position.y < _checkpoint.y) _playerTr.position = _checkpoint;
    }

    private void InitializeLevel(Vector3 checkpoint)
    {
        Instantiate(_levelGroundPrefab, checkpoint, Quaternion.identity);
    }
}
