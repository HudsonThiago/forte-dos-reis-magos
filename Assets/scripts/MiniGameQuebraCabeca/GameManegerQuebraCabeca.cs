using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Adicionamos o namespace do Novo Sistema de Input
using UnityEngine.InputSystem;
// Importante para usar Text (Legacy) e CanvasGroup
using UnityEngine.UI;

public class GameManegerQuebraCabeca : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;

    // Campo do Novo Sistema de Input (Configure no Inspector)
    [SerializeField] private InputAction clickAction;

    // Campo para o LayerMask (Configure no Inspector)
    [SerializeField] private LayerMask pieceLayer;

    // Campo para definir dificuldade (Configure no Inspector)
    [SerializeField] private int sizeGame;

    // NOVOS CAMPOS PARA CONCLUSÃO DO QUEBRA-CABEÇA
    // ---
    [Header("UI de Conclusão")]
    [Tooltip("Arraste o painel completo (panelQuebraCabecaCompleto) que deve ter um Canvas Group.")]
    [SerializeField] private GameObject panelQuebraCabecaCompleto;

    [Tooltip("Arraste o componente Text (Legacy) para atribuição do texto.")]
    [SerializeField] private Text textQCC; // Componente Text (Legacy)

    [Tooltip("Lista de textos. O índice do texto é definido por 'Imagem Atual Index'.")]
    [SerializeField] private List<string> textosDeConclusao = new List<string>();

    [Tooltip("Duração do fade-in do painel de conclusão.")]
    [SerializeField] private float animationDuration = 1.0f;

    [Tooltip("Índice da imagem/texto atual (0, 1, 2, etc.)")]
    [SerializeField] private int imagemAtualIndex = 0;
    // ---

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = false;

    // Métodos para ativar e desativar a Action
    private void OnEnable()
    {
        clickAction.performed += HandleClick;
        clickAction.Enable();
    }

    private void OnDisable()
    {
        clickAction.performed -= HandleClick;
        clickAction.Disable();
    }

    // A nova função que será chamada quando o clique for detectado
    private void HandleClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // 1. Converte a posição da tela (mouse) para um ponto 3D no mundo.
        Vector3 worldPoint3D = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // CORREÇÃO Z: Garante que o ponto de clique esteja no plano das peças (Z=0).
        worldPoint3D.z = 0f;

        // 2. Cria um Vector2 (Point-Cast).
        Vector2 worldPoint2D = new Vector2(worldPoint3D.x, worldPoint3D.y);

        // 3. Executa o Raycast 2D, usando a Layer Mask para filtrar APENAS as peças.
        RaycastHit2D hit = Physics2D.Raycast(worldPoint2D, Vector2.zero, Mathf.Infinity, pieceLayer);

        if (hit)
        {
            // DEBUG: Isto deve aparecer no console ao clicar na peça!
            // Debug.Log($"Peça Clicada: {hit.transform.name}", hit.transform.gameObject);

            // Lógica de movimento
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == hit.transform)
                {
                    if (SwapIfValid(i, -size, size)) { break; }
                    if (SwapIfValid(i, +size, size)) { break; }
                    if (SwapIfValid(i, -1, 0)) { break; }
                    if (SwapIfValid(i, +1, size - 1)) { break; }
                }
            }
        }
    }


    // Create the game setup with size x size pieces.
    private void CreateGamePieces(float gapThickness)
    {
        // This is the width of each tile.
        float width = 1 / (float)size;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);

                // Pieces will be in a game board going from -1 to +1.
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                 +1 - (2 * width * row) - width,
                                                 0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";

                // We want an empty space in the bottom right.
                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    // We want to map the UV coordinates appropriately, they are 0->1.
                    float gap = gapThickness / 2;
                    // ATENÇÃO: Garanta que piecePrefab tem MeshFilter/MeshRenderer para isso!
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;

                    Vector2[] uv = new Vector2[4];
                    // UV coord order: (0, 1), (1, 1), (0, 0), (1, 0)
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                    // Assign our new UVs to the mesh.
                    mesh.uv = uv;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pieces = new List<Transform>();
        if (sizeGame > 0) size = sizeGame;
        CreateGamePieces(0.01f);

        // Garante que o painel de conclusão comece desativado
        if (panelQuebraCabecaCompleto != null)
        {
            panelQuebraCabecaCompleto.SetActive(false);
        }

        shuffling = true;
        StartCoroutine(WaitShuffle(0.5f));
    }

    // Update is kept for completion check and shuffling only.
    void Update()
    {
        // Check for completion.
        if (!shuffling && CheckCompletion())
        {
            Debug.Log("quebra cabeça completo");

            // Chama a rotina para finalizar o jogo
            HandleCompletion();

            // Desativa o script (Update) após a conclusão para economizar recursos
            enabled = false;
        }
    }

    // NOVO MÉTODO: Lida com a conclusão do quebra-cabeça
    private void HandleCompletion()
    {
        // 1. Atribui o texto correto
        if (textQCC != null && textosDeConclusao.Count > imagemAtualIndex && imagemAtualIndex >= 0)
        {
            textQCC.text = textosDeConclusao[imagemAtualIndex];
        }
        else if (textQCC != null)
        {
            // Fallback caso o índice esteja fora do limite
            textQCC.text = "Quebra-cabeça Completo! (Texto padrão)";
        }

        // 2. Inicia a animação de Fade-In do painel
        if (panelQuebraCabecaCompleto != null)
        {
            panelQuebraCabecaCompleto.SetActive(true);
            StartCoroutine(AnimateCompletionPanel(panelQuebraCabecaCompleto));
        }
    }

    // NOVA CORROTINA: Anima o painel de conclusão (Fade-In)
    private IEnumerator AnimateCompletionPanel(GameObject panel)
    {
        // Tenta obter o CanvasGroup para a animação de opacidade
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogWarning("O Painel de Conclusão precisa de um componente Canvas Group para o fade-in animado.");
            yield break; // Sai da corrotina se não houver CanvasGroup
        }

        // Configuração inicial (invisível)
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Interpola a opacidade de 0 a 1 (Lerp)
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);

            elapsedTime += Time.deltaTime;

            yield return null; // Espera o próximo frame
        }

        // Garante que o alpha final seja 1.0 (totalmente visível)
        canvasGroup.alpha = 1f;
    }


    public void clickButtonEmbaralhar()
    {
        if (!shuffling)
        {
            Debug.Log("Embaralhando");

            shuffling = true;
            StartCoroutine(WaitShuffle(0.5f));
        }
    }

    // colCheck is used to stop horizontal moves wrapping.
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            // Swap them in game state.
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            // Swap their transforms.
            (pieces[i].localPosition, pieces[i + offset].localPosition) = ((pieces[i + offset].localPosition, pieces[i].localPosition));
            // Update empty location.
            emptyLocation = i;
            return true;
        }
        return false;
    }

    // We name the pieces in order so we can use this to check completion.
    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
    }

    // Brute force shuffling.
    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        while (count < (size * size * size))
        {
            // Pick a random location.
            int rnd = Random.Range(0, size * size);
            // Only thing we forbid is undoing the last move.
            if (rnd == last) { continue; }
            last = emptyLocation;
            // Try surrounding spaces looking for valid move.
            if (SwapIfValid(rnd, -size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, -1, 0))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }
}