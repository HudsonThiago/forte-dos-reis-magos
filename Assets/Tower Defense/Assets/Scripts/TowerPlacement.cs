using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower; // Variável para armazenar a torre colocada neste spot

    // ... Start() e Update()

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tower incomingTower = collision.GetComponent<Tower>();

        if (incomingTower != null)
        {
            // Se houver uma torre anterior (já colocada) neste spot
            if (_placedTower != null)
            {
                // **AÇÃO CHAVE:** Destrói a torre antiga para dar lugar à nova.
                // Isso garante que a torre antiga desapareça imediatamente.
                Destroy(_placedTower.gameObject);

                // Nota: Não é necessário definir _placedTower = null aqui, pois
                // ela será definida para a nova torre logo abaixo.
            }

            // 1. Define a posição de colocação para a nova torre.
            incomingTower.SetPlacePosition(transform.position);

            // 2. Marca a nova torre como a torre colocada.
            _placedTower = incomingTower;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_placedTower == null)
        {
            return;
        }

        // Se a torre que sair for a que estava marcada como colocada, limpe a posição.
        if (collision.GetComponent<Tower>() == _placedTower)
        {
            _placedTower.SetPlacePosition(null);
            _placedTower = null;
        }
    }
}