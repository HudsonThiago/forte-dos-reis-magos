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
    // O piecePrefab deve ser um prefab que voc� arrastou no Inspector
    [SerializeField] private Transform piecePrefab;

    // Campo do Novo Sistema de Input (Configure no Inspector)
    [SerializeField] private InputAction clickAction;

    // Campo para o LayerMask (Configure no Inspector)
    [SerializeField] private LayerMask pieceLayer;

    // Campo para definir dificuldade (Configure no Inspector)
    // O valor � definido pelo Painel de Sele��o antes do jogo come�ar
    [SerializeField] private int sizeGame = 3;

    // --- NOVOS CAMPOS PARA SELE��O ---
    [Header("UI de Sele��o de Jogo")]
    [Tooltip("Arraste o painel de escolha de imagem/dificuldade.")]
    [SerializeField] private GameObject panelEscolhaImagem;

    [Tooltip("Lista de materiais para as pe�as. O �ndice corresponde ao �ndice de conclus�o.")]
    [SerializeField] private List<Material> materiaisQuebraCabeca = new List<Material>();

    [Header("Toggles de Dificuldade")]
    // Estes campos s�o usados apenas para definir o estado inicial (como marcado)
    [SerializeField] private Toggle toggleFacil;
    [SerializeField] private Toggle toggleMedio;
    [SerializeField] private Toggle toggleDificil;
    // ---

    // NOVOS CAMPOS PARA CONCLUS�O DO QUEBRA-CABE�A
    // ---
    [Header("UI de Conclus�o")]
    [Tooltip("Arraste o painel completo (panelQuebraCabecaCompleto) que deve ter um Canvas Group.")]
    [SerializeField] private GameObject panelQuebraCabecaCompleto;

    [Tooltip("Arraste o componente Text (Legacy) para atribui��o do texto.")]
    [SerializeField] private Text textQCC; // Componente Text (Legacy)

    [Tooltip("Lista de textos. O �ndice do texto � definido por 'Imagem Atual Index'.")]
    [SerializeField] private List<string> textosDeConclusao = new List<string>();

    [Tooltip("Dura��o do fade-in do painel de conclus�o.")]
    [SerializeField] private float animationDuration = 1.0f;

    // �NDICE DA IMAGEM ATUAL (IMPORTANTE: Mapeia Material e Texto de Conclus�o)
    [Tooltip("�ndice da imagem/texto atual (0, 1, 2, etc.)")]
    [SerializeField] private int imagemAtualIndex = 0;
    // ---

    // --- NOVOS CAMPOS PARA O MENU DE PAUSA ---
    [Header("UI de Pausa")]
    [Tooltip("Arraste o painel de pausa (pausePanel) que cont�m os bot�es.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("Arraste o bot�o de pausa (ButtonPause) para que ele possa ser desativado/reativado.")]
    [SerializeField] private Button buttonPause; // Componente Button

    [Tooltip("Arraste o bot�o de embaralhar para que ele possa ser desativado/reativado.")]
    [SerializeField] private Button buttonEmbralhar; // Componente Button

    private bool isPaused = false;
    private bool gameStarted = false; // Flag para controlar se o jogo come�ou
    // ---

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = false;

    // M�todos para ativar e desativar a Action
    private void OnEnable()
    {
        clickAction.performed += HandleClick;
        clickAction.Enable();

        // Remove listeners din�micos, a conex�o agora � feita no Inspector com SetDifficulty(int)
        // Se a conex�o for feita via c�digo no Start(), mantenha os AddListener
    }

    private void OnDisable()
    {
        clickAction.performed -= HandleClick;
        clickAction.Disable();
    }

    // A nova fun��o que ser� chamada quando o clique for detectado
    private void HandleClick(InputAction.CallbackContext context)
    {
        // Impede o clique se o jogo estiver pausado, embaralhando ou se n�o tiver come�ado
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
        // Limpa pe�as anteriores se houver (para Novo Jogo/Reset)
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
            Debug.LogWarning($"Material n�o encontrado para o �ndice {imagemAtualIndex}. Verifique a lista 'Materiais Quebra Cabeca'.");
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

    // Start � chamado antes do primeiro frame update
    void Start()
    {
        pieces = new List<Transform>();

        // 1. Garante que os pain�is de jogo estejam desativados
        if (panelQuebraCabecaCompleto != null) panelQuebraCabecaCompleto.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Desativa os bot�es de jogo no in�cio
        SetGameButtonsInteractive(false);

        // 2. ATIVA O PAINEL DE ESCOLHA DE IMAGEM/DIFICULDADE
        if (panelEscolhaImagem != null)
        {
            panelEscolhaImagem.SetActive(true);
        }

        // 3. Define a dificuldade inicial (padr�o 3x3)
        // � crucial que sizeGame tenha um valor antes de StartGame()
        sizeGame = 3;
        if (toggleFacil != null) toggleFacil.isOn = true; // Marca o f�cil como padr�o

        // O jogo n�o come�a aqui. Ele s� come�ar� quando o usu�rio clicar em "Iniciar" no painel.
    }

    // --- M�TODOS DE CONTROLE DO JOGO ---

    // M�todo para controlar a intera��o dos bot�es de jogo (Pause e Embaralhar)
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
    /// Chamado para iniciar o jogo ap�s a sele��o de imagem e dificuldade.
    /// </summary>
    public void StartGame()
    {
        if (gameStarted) return; // Evita iniciar duas vezes

        if (panelEscolhaImagem != null)
        {
            panelEscolhaImagem.SetActive(false);
        }

        size = sizeGame; // Define o tamanho do jogo com base na sele��o

        // 1. Cria as pe�as com o material e tamanho escolhidos
        CreateGamePieces(0.01f);

        // 2. Ativa os bot�es de jogo
        SetGameButtonsInteractive(true);

        // 3. Inicia o embaralhamento
        shuffling = true;
        StartCoroutine(WaitShuffle(0.5f));
        gameStarted = true; // Marca que o jogo pode ser jogado
    }

    // Update � mantido para checagem de conclus�o
    void Update()
    {
        // S� checa a conclus�o se o jogo tiver come�ado, n�o estiver embaralhando e n�o estiver pausado.
        if (gameStarted && !shuffling && !isPaused && CheckCompletion())
        {
            Debug.Log("quebra cabe�a completo");
            HandleCompletion();
            enabled = false;
        }
    }

    // --- L�GICA DE SELE��O DE IMAGEM/DIFICULDADE ---

    /// <summary>
    /// Chamado pelos bot�es de escolha de imagem.
    /// </summary>
    /// <param name="index">O �ndice da imagem e do texto na lista (0, 1, 2...).</param>
    public void SelectImage(int index)
    {
        if (index >= 0 && index < materiaisQuebraCabeca.Count)
        {
            imagemAtualIndex = index;
            Debug.Log($"Imagem selecionada (Index: {index})");
        }
        else
        {
            Debug.LogError($"�ndice de imagem inv�lido: {index}. Verifique 'Materiais Quebra Cabeca' e 'Textos De Conclusao'.");
        }
    }

    /// <summary>
    /// Chamado pelos Toggles de dificuldade (On Value Changed -> Fun��o est�tica int).
    /// </summary>
    /// <param name="newSize">O novo tamanho (3, 4 ou 5).</param>
    public void SetDifficulty(int newSize)
    {
        // Esta fun��o ser� chamada duas vezes (true/false) se o evento OnValueChanged for usado.
        // J� que o Toggle Group garante que apenas um est� ativo, simplesmente definimos o tamanho.
        // O valor 'sizeGame' ser� usado apenas quando StartGame() for chamado.
        sizeGame = newSize;
        Debug.Log($"Dificuldade definida: {sizeGame}x{sizeGame}");
    }

    // --- L�GICA DE PAUSA ---

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

        // Desativa/Ativa os bot�es de jogo quando pausado
        if (gameStarted)
        {
            SetGameButtonsInteractive(!shouldPause);
        }
        else if (buttonPause != null)
        {
            // No caso do jogo ainda n�o ter come�ado (StartGame n�o chamado)
            buttonPause.interactable = false;
        }

        Debug.Log(shouldPause ? "Jogo Pausado" : "Jogo Despausado");
    }

    // --- FUN��ES DE NAVEGA��O DO MENU ---

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
        SceneManager.LoadScene("LobbyForte");
    }

    public void clickButtonVoltarAoMenu()
    {
        TogglePause(false);
        Debug.Log("Voltando ao Menu Principal (TODO: Carregar cena do Menu)");
        SceneManager.LoadScene("MainMenu");
    }

    public void clickButtonSair()
    {
        Debug.Log("Saindo do Jogo (Apenas funciona em Build)");
        Application.Quit();
    }


    // NOVO M�TODO: Lida com a conclus�o do quebra-cabe�a
    private void HandleCompletion()
    {
        // Reativa a pe�a vazia para o estado de conclus�o
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
            textQCC.text = "Quebra-cabe�a Completo! (Texto padr�o)";
        }

        // 2. Inicia a anima��o de Fade-In do painel
        if (panelQuebraCabecaCompleto != null)
        {
            panelQuebraCabecaCompleto.SetActive(true);
            StartCoroutine(AnimateCompletionPanel(panelQuebraCabecaCompleto));
        }

        // Desativa os bot�es de jogo
        SetGameButtonsInteractive(false);
    }

    // NOVA CORROTINA: Anima o painel de conclus�o (Fade-In)
    private IEnumerator AnimateCompletionPanel(GameObject panel)
    {
        // Tenta obter o CanvasGroup para a anima��o de opacidade
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogWarning("O Painel de Conclus�o precisa de um componente Canvas Group para o fade-in animado.");
            yield break; // Sai da corrotina se n�o houver CanvasGroup
        }

        // Configura��o inicial (invis�vel)
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Interpola a opacidade de 0 a 1 (Lerp)
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);

            elapsedTime += Time.deltaTime;

            yield return null; // Espera o pr�ximo frame
        }

        // Garante que o alpha final seja 1.0 (totalmente vis�vel)
        canvasGroup.alpha = 1f;
    }


    public void clickButtonEmbaralhar()
    {
        // Impede o embaralhamento se o jogo estiver pausado ou n�o tiver come�ado
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