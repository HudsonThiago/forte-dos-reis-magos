using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _towerIcon;

    // NOVO: Adicione uma Image para visualização do cooldown no Inspector
    [SerializeField] private Image _cooldownImage;

    // NOVO: Cooldown Properties
    [SerializeField] private float _cooldownDuration = 5f; // Duração do cooldown em segundos
    private float _remainingCooldown = 0f; // Tempo restante do cooldown

    // Camada que define os spots de colocação no mundo (Mantido, mas não usado diretamente para a colocação com OnTrigger)
    private LayerMask _placementLayer;

    private Tower _towerPrefab;
    private Tower _currentSpawnedTower;

    void Start()
    {
        // Inicialização mantida
        _placementLayer = LayerMask.GetMask("PlacementSlot");

        // Configura o estado inicial do cooldown (se a imagem existir)
        if (_cooldownImage != null)
        {
            _cooldownImage.fillAmount = 0f;
        }
    }

    // Update é onde o cooldown será processado e o visual atualizado
    void Update()
    {
        if (_remainingCooldown > 0f)
        {
            _remainingCooldown -= Time.deltaTime;

            if (_cooldownImage != null)
            {
                // Atualiza o preenchimento do ícone de cooldown (0 = pronto, 1 = total)
                _cooldownImage.fillAmount = _remainingCooldown / _cooldownDuration;
            }
        }
        else
        {
            // Garante que o ícone de cooldown esteja invisível quando pronto
            if (_cooldownImage != null && _cooldownImage.fillAmount > 0)
            {
                _cooldownImage.fillAmount = 0f;
            }
        }
    }

    public void SetTowerPrefab(Tower tower)
    {
        _towerPrefab = tower;
        _towerIcon.sprite = tower.GetTowerHeadIcon();
    }

    // Implementasi dari Interface IBeginDragHandler
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 🚨 VERIFICAÇÃO DO COOLDOWN: Impede o arraste se o cooldown não zerou
        if (_remainingCooldown > 0f)
        {
            return;
        }

        GameObject newTowerObj = Instantiate(_towerPrefab.gameObject);
        _currentSpawnedTower = newTowerObj.GetComponent<Tower>();
        _currentSpawnedTower.ToggleOrderInLayer(true);
    }

    // Implementasi dari Interface IDragHandler
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        Camera mainCamera = Camera.main;
        Vector3 screenPosition = eventData.position;

        screenPosition.z = -mainCamera.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        _currentSpawnedTower.transform.position = targetPosition;
    }

    // Implementasi dari Interface IEndDragHandler
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        // A lógica original depende que o OnTriggerEnter2D do TowerPlacement
        // tenha definido _currentSpawnedTower.PlacePosition.

        if (_currentSpawnedTower.PlacePosition == null)
        {
            // Colocação falhou (não foi solta sobre um TowerPlacement spot válido)
            Destroy(_currentSpawnedTower.gameObject);
        }
        else
        {
            // Colocação bem-sucedida (PlacePosition foi definido por TowerPlacement)

            // Trava a torre no lugar
            _currentSpawnedTower.LockPlacement();
            _currentSpawnedTower.ToggleOrderInLayer(false);

            // Registra e ATIVA O COOLDOWN
            LevelManager.Instance.RegisterSpawnedTower(_currentSpawnedTower);

            _remainingCooldown = _cooldownDuration; // 🚨 ATIVAÇÃO DO COOLDOWN
            if (_cooldownImage != null)
            {
                _cooldownImage.fillAmount = 1f;
            }

            _currentSpawnedTower = null;
        }
    }
}