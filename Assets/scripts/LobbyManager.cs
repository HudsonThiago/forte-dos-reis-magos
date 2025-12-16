using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    // 1. Variável para referenciar o destino do teletransporte (o "transformer")
    public Transform pontoDeTeletransporte;

    // 2. Variável para referenciar o BoxCollider do trigger
    // É uma boa prática, mas não estritamente necessário se o script estiver no mesmo GameObject
    // public BoxCollider triggerCollider; 

    // 3. Método chamado quando outro Collider entra no Trigger deste GameObject
    private void OnTriggerEnter(Collider other)
    {
        // 4. Verifique se a variável de destino está definida
        if (pontoDeTeletransporte == null)
        {
            Debug.LogError("O ponto de teletransporte (Transformer) não está definido no Inspector!");
            return;
        }

        // 5. Verifique se o objeto que entrou no trigger é o personagem
        // Você pode usar Tags, Layers ou componentes específicos.
        // Neste exemplo, vamos usar a Tag "Player". Certifique-se de que seu personagem tem a tag "Player".
        if (other.CompareTag("Player"))
        {
            // O componente 'other' é o Collider do personagem. O GameObject do personagem
            // é 'other.gameObject'. Para teletransportar, movemos a posição.

            // 6. Teletransporta o personagem para a posição e rotação do pontoDeTeletransporte
            other.transform.position = pontoDeTeletransporte.position;
            other.transform.rotation = pontoDeTeletransporte.rotation;

            Debug.Log("Personagem teletransportado!");
        }
    }

    // Os métodos Start e Update não são necessários para esta funcionalidade.
    void Start()
    {
        // Se você precisar fazer verificações iniciais, faça aqui.
    }

    void Update()
    {
        // Não é necessário para o teletransporte baseado em trigger.
    }
}