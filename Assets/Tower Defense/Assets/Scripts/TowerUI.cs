using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _towerIcon;

    private Tower _towerPrefab;
    private Tower _currentSpawnedTower;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetTowerPrefab(Tower tower)
    {
        _towerPrefab = tower;
        _towerIcon.sprite = tower.GetTowerHeadIcon();
    }

    // Implementasi dari Interface IBeginDragHandler
    public void OnBeginDrag(PointerEventData eventData)
    {
        GameObject newTowerObj = Instantiate(_towerPrefab.gameObject);
        _currentSpawnedTower = newTowerObj.GetComponent<Tower>();
        _currentSpawnedTower.ToggleOrderInLayer(true);
    }

    // Implementasi dari Interface IDragHandler
    // AJUSTADO: Usa eventData.position em vez de Input.mousePosition
    public void OnDrag(PointerEventData eventData)
    {
        Camera mainCamera = Camera.main;

        // CORRIGIDO: Use eventData.position, que já é a posição de tela
        Vector3 screenPosition = eventData.position;

        screenPosition.z = -mainCamera.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        _currentSpawnedTower.transform.position = targetPosition;
    }

    // Implementasi dari Interface IEndDragHandler
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower.PlacePosition == null)
        {
            Destroy(_currentSpawnedTower.gameObject);
        }
        else
        {
            _currentSpawnedTower.LockPlacement();
            _currentSpawnedTower.ToggleOrderInLayer(false);
            LevelManager.Instance.RegisterSpawnedTower(_currentSpawnedTower);
            _currentSpawnedTower = null;
        }
    }
}