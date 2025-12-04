using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Adicionamos o namespace do Novo Sistema de Input
using UnityEngine.InputSystem;
// Importante para usar Text (Legacy), CanvasGroup, Button e Toggle
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Adicionado para carregar cenas

public class GameManegerQuebraCabeca : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    // O piecePrefab deve ser um prefab que você arrastou no Inspector
    [SerializeField] private Transform piecePrefab;

    // Campo do Novo Sistema de Input (Configure no Inspector)
    [SerializeField] private InputAction clickAction;

    // Campo para o LayerMask (Configure no Inspector)
    [SerializeField] private LayerMask pieceLayer;

    // Campo para definir dificuldade (Configure no Inspector)
    // O valor é definido pelo Painel de Seleção antes do jogo começar
    [SerializeField] private int sizeGame = 3;

    // --- NOVOS CAMPOS PARA SELEÇÃO ---
    [Header("UI de Seleção de Jogo")]
    [Tooltip("Arraste o painel de escolha de imagem/dificuldade.")]
    [SerializeField] private GameObject panelEscolhaImagem;

    [Tooltip("Lista de materiais para as peças. O índice corresponde ao índice de conclusão.")]
    [SerializeField] private List<Material> materiaisQuebraCabeca = new List<Material>();

    [Header("Toggles de Dificuldade")]
    // Estes campos são usados apenas para definir o estado inicial (como marcado)
    [SerializeField] private Toggle toggleFacil;
    [SerializeField] private Toggle toggleMedio;
    [SerializeField] private Toggle toggleDificil;
    // ---

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

    // ÍNDICE DA IMAGEM ATUAL (IMPORTANTE: Mapeia Material e Texto de Conclusão)
    [Tooltip("Índice da imagem/texto atual (0, 1, 2, etc.)")]
    [SerializeField] private int imagemAtualIndex = 0;
    // ---

    // --- NOVOS CAMPOS PARA O MENU DE PAUSA ---
    [Header("UI de Pausa")]
    [Tooltip("Arraste o painel de pausa (pausePanel) que contém os botões.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("Arraste o botão de pausa (ButtonPause) para que ele possa ser desativado/reativado.")]
    [SerializeField] private Button buttonPause; // Componente Button

    [Tooltip("Arraste o botão de embaralhar para que ele possa ser desativado/reativado.")]
    [SerializeField] private Button buttonEmbralhar; // Componente Button

    private bool isPaused = false;
    private bool gameStarted = false; // Flag para controlar se o jogo começou
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

        // Remove listeners dinâmicos, a conexão agora é feita no Inspector com SetDifficulty(int)
        // Se a conexão for feita via código no Start(), mantenha os AddListener
    }

    private void OnDisable()
    {
        clickAction.performed -= HandleClick;
        clickAction.Disable();
    }

    // A nova função que será chamada quando o clique for detectado
    private void HandleClick(InputAction.CallbackContext context)
    {
        // Impede o clique se o jogo estiver pausado, embaralhando ou se não tiver começado
        if (!context.performed || isPaused || shuffling || !gameStarted) return;

        Vector3 worldPoint3D = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPoint3D.z = 0f;
        Vector2 worldPoint2D = new Vector2(worldPoint3D.x, worldPoint3D.y);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint2D, Vector2.zero, Mathf.Infinity, pieceLayer);

        if (hit)
        {
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


    private void CreateGamePieces(float gapThickness)
    {
        // Limpa peças anteriores se houver (para Novo Jogo/Reset)
        if (pieces != null && pieces.Count > 0)
        {
            foreach (Transform piece in pieces)
            {
                Destroy(piece.gameObject);
            }
            pieces.Clear();
        }

        // --- APLICA O MATERIAL ESCOLHIDO ---
        // Pega o Renderer no PiecePrefab (assume que tem um MeshRenderer ou similar)
        if (piecePrefab.TryGetComponent<MeshRenderer>(out MeshRenderer renderer) && materiaisQuebraCabeca.Count > imagemAtualIndex && imagemAtualIndex >= 0)
        {
            // Aplica o material no prefab ANTES de instanciar
            renderer.sharedMaterial = materiaisQuebraCabeca[imagemAtualIndex];
        }
        else
        {
            Debug.LogWarning($"Material não encontrado para o índice {imagemAtualIndex}. Verifique a lista 'Materiais Quebra Cabeca'.");
        }
        // ---

        float width = 1 / (float)size;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);

                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                 +1 - (2 * width * row) - width,
                                                 0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";

                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;

                    Vector2[] uv = new Vector2[4];
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                    mesh.uv = uv;
                }
            }
        }
    }

    // Start é chamado antes do primeiro frame update
    void Start()
    {
        pieces = new List<Transform>();

        // 1. Garante que os painéis de jogo estejam desativados
        if (panelQuebraCabecaCompleto != null) panelQuebraCabecaCompleto.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Desativa os botões de jogo no início
        SetGameButtonsInteractive(false);

        // 2. ATIVA O PAINEL DE ESCOLHA DE IMAGEM/DIFICULDADE
        if (panelEscolhaImagem != null)
        {
            panelEscolhaImagem.SetActive(true);
        }

        // 3. Define a dificuldade inicial (padrão 3x3)
        // É crucial que sizeGame tenha um valor antes de StartGame()
        sizeGame = 3;
        if (toggleFacil != null) toggleFacil.isOn = true; // Marca o fácil como padrão

        // O jogo não começa aqui. Ele só começará quando o usuário clicar em "Iniciar" no painel.
    }

    // --- MÉTODOS DE CONTROLE DO JOGO ---

    // Método para controlar a interação dos botões de jogo (Pause e Embaralhar)
    private void SetGameButtonsInteractive(bool interactive)
    {
        if (buttonPause != null)
        {
            buttonPause.interactable = interactive;
        }
        if (buttonEmbralhar != null)
        {
            buttonEmbralhar.interactable = interactive;
        }
    }

    /// <summary>
    /// Chamado para iniciar o jogo após a seleção de imagem e dificuldade.
    /// </summary>
    public void StartGame()
    {
        if (gameStarted) return; // Evita iniciar duas vezes

        if (panelEscolhaImagem != null)
        {
            panelEscolhaImagem.SetActive(false);
        }

        size = sizeGame; // Define o tamanho do jogo com base na seleção

        // 1. Cria as peças com o material e tamanho escolhidos
        CreateGamePieces(0.01f);

        // 2. Ativa os botões de jogo
        SetGameButtonsInteractive(true);

        // 3. Inicia o embaralhamento
        shuffling = true;
        StartCoroutine(WaitShuffle(0.5f));
        gameStarted = true; // Marca que o jogo pode ser jogado
    }

    // Update é mantido para checagem de conclusão
    void Update()
    {
        // Só checa a conclusão se o jogo tiver começado, não estiver embaralhando e não estiver pausado.
        if (gameStarted && !shuffling && !isPaused && CheckCompletion())
        {
            Debug.Log("quebra cabeça completo");
            HandleCompletion();
            enabled = false;
        }
    }

    // --- LÓGICA DE SELEÇÃO DE IMAGEM/DIFICULDADE ---

    /// <summary>
    /// Chamado pelos botões de escolha de imagem.
    /// </summary>
    /// <param name="index">O índice da imagem e do texto na lista (0, 1, 2...).</param>
    public void SelectImage(int index)
    {
        if (index >= 0 && index < materiaisQuebraCabeca.Count)
        {
            imagemAtualIndex = index;
            Debug.Log($"Imagem selecionada (Index: {index})");
        }
        else
        {
            Debug.LogError($"Índice de imagem inválido: {index}. Verifique 'Materiais Quebra Cabeca' e 'Textos De Conclusao'.");
        }
    }

    /// <summary>
    /// Chamado pelos Toggles de dificuldade (On Value Changed -> Função estática int).
    /// </summary>
    /// <param name="newSize">O novo tamanho (3, 4 ou 5).</param>
    public void SetDifficulty(int newSize)
    {
        // Esta função será chamada duas vezes (true/false) se o evento OnValueChanged for usado.
        // Já que o Toggle Group garante que apenas um está ativo, simplesmente definimos o tamanho.
        // O valor 'sizeGame' será usado apenas quando StartGame() for chamado.
        sizeGame = newSize;
        Debug.Log($"Dificuldade definida: {sizeGame}x{sizeGame}");
    }

    // --- LÓGICA DE PAUSA ---

    public void clickButtonPause()
    {
        // Pausa o jogo
        TogglePause(true);
    }

    public void clickButtonRetomar()
    {
        // Despausa o jogo
        TogglePause(false);
    }

    private void TogglePause(bool shouldPause)
    {
        isPaused = shouldPause;

        // Time.timeScale = 0f para pausar, 1f para despausar (velocidade normal)
        Time.timeScale = shouldPause ? 0f : 1f;

        // Ativa/Desativa o painel de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(shouldPause);
        }

        // Desativa/Ativa os botões de jogo quando pausado
        if (gameStarted)
        {
            SetGameButtonsInteractive(!shouldPause);
        }
        else if (buttonPause != null)
        {
            // No caso do jogo ainda não ter começado (StartGame não chamado)
            buttonPause.interactable = false;
        }

        Debug.Log(shouldPause ? "Jogo Pausado" : "Jogo Despausado");
    }

    // --- FUNÇÕES DE NAVEGAÇÃO DO MENU ---

    public void clickButtonNovoJogo()
    {
        // Despausa antes de carregar e recarrega a cena para resetar
        TogglePause(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void clickButtonVoltarAoLobby()
    {
        TogglePause(false);
        Debug.Log("Voltando ao Lobby (TODO: Carregar cena do Lobby)");
        // Exemplo: SceneManager.LoadScene("LobbySceneName");
    }

    public void clickButtonVoltarAoMenu()
    {
        TogglePause(false);
        Debug.Log("Voltando ao Menu Principal (TODO: Carregar cena do Menu)");
        // Exemplo: SceneManager.LoadScene("MenuPrincipalSceneName");
    }

    public void clickButtonSair()
    {
        Debug.Log("Saindo do Jogo (Apenas funciona em Build)");
        Application.Quit();
    }


    // NOVO MÉTODO: Lida com a conclusão do quebra-cabeça
    private void HandleCompletion()
    {
        // Reativa a peça vazia para o estado de conclusão
        if (pieces.Count > emptyLocation)
        {
            pieces[emptyLocation].gameObject.SetActive(true);
        }

        // 1. Atribui o texto correto com base no imagemAtualIndex
        if (textQCC != null && textosDeConclusao.Count > imagemAtualIndex && imagemAtualIndex >= 0)
        {
            textQCC.text = textosDeConclusao[imagemAtualIndex];
        }
        else if (textQCC != null)
        {
            textQCC.text = "Quebra-cabeça Completo! (Texto padrão)";
        }

        // 2. Inicia a animação de Fade-In do painel
        if (panelQuebraCabecaCompleto != null)
        {
            panelQuebraCabecaCompleto.SetActive(true);
            StartCoroutine(AnimateCompletionPanel(panelQuebraCabecaCompleto));
        }

        // Desativa os botões de jogo
        SetGameButtonsInteractive(false);
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
        // Impede o embaralhamento se o jogo estiver pausado ou não tiver começado
        if (!shuffling && !isPaused && gameStarted)
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