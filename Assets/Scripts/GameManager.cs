using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [Header("Base Config")]
    [SerializeField] private GameObject _levelGroundPrefab;
    [SerializeField] private Transform _playerTr;
    [SerializeField] private float _yDistanceToTravel;
    [SerializeField] private Vector3 _checkpoint = new(0.0f, 0.5f, 0.0f);  // read only
    [SerializeField] private float _currentDistanceTraveled = 0.0f;        // read only
    [SerializeField] private int _levelID = 1;                             // read only

    [Header("Vfx's")]
    [SerializeField] private Volume _volume;
    [SerializeField, ColorUsage(true, true)] private Color _postProcessBloomColor;
    [SerializeField] private Color[] _levelColors;
    [SerializeField] private UnityEngine.UI.Image _overlayImg;
    private ColorAdjustments _colorAdjustments;

    private void Start()
    {
        InitializeVFXs();
    }
    private void Update()
    {
        ApplyVFXs();

        _currentDistanceTraveled = _playerTr.position.y;
        if (_currentDistanceTraveled >= _yDistanceToTravel * _levelID)
        {
            _levelID++;
            _checkpoint = _playerTr.position;
            _checkpoint.x = 0.0f;
            _checkpoint.z = 0.0f;
            InitializeLevel(_checkpoint);
        }

        if (_playerTr.position.y < _checkpoint.y) _playerTr.position = _checkpoint;
    }

    private void InitializeLevel(Vector3 checkpoint)
    {
        Instantiate(_levelGroundPrefab, checkpoint, Quaternion.identity);
    }

    private Color ConvertColorToHDR(Color color)
    {
        float intensityMultiplier = 0.9f;
        return new Color(color.r * intensityMultiplier, color.g * intensityMultiplier, color.b * intensityMultiplier, color.a);
    }
    private void InitializeVFXs()
    {
        if (_volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            _colorAdjustments = colorAdjustments;

            Color targetOverlayColor = _levelColors[_levelID - 1];
            Color newColor = new(targetOverlayColor.r, targetOverlayColor.g, targetOverlayColor.b, 0.0f);
            _overlayImg.color = newColor;

            _postProcessBloomColor = ConvertColorToHDR(_overlayImg.color);
            _colorAdjustments.colorFilter.value = _postProcessBloomColor;
        }
    }
    private void ApplyVFXs()
    {
        float levelStart = (_levelID - 1) * _yDistanceToTravel;
        float levelEnd = _levelID * _yDistanceToTravel;
        float progress = Mathf.Clamp01((_playerTr.position.y - levelStart) / (levelEnd - levelStart));
        float alpha = progress * 0.25f;

        Color targetColor = _levelColors[_levelID - 1];
        _overlayImg.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
        _postProcessBloomColor = ConvertColorToHDR(_overlayImg.color);
        _colorAdjustments.colorFilter.value = _postProcessBloomColor;
    }
}
