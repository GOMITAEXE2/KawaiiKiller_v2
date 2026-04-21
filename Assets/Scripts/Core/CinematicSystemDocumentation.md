# Sistema de Menú y Cinemáticas - Documento Técnico

## Índice

1. [Sistema de Menú Principal](#sistema-de-menú-principal)
   - [Arquitectura General](#arquitectura-general)
   - [Flujo de Pantallas](#flujo-de-pantallas)
   - [Configuración en Unity](#configuración-en-unity)
   - [Referencia de Scripts](#referencia-de-scripts-mainmenumanager)

2. [Sistema de Cinemáticas](#sistema-de-cinemáticas)
   - [Arquitectura General](#arquitectura-general-1)
   - [Flujo de Viñetas](#flujo-de-viñetas)
   - [Configuración en Unity](#configuración-en-unity-1)
   - [Referencia de Scripts](#referencia-de-scripts-cinematicmanager)

---

## Sistema de Menú Principal

### Arquitectura General

El sistema de menú principal está gestionado por **MainMenuManager** y consta de:

- **Panel Principal (MainPanel)**: Botones del menú principal
- **Panel de Opciones (OptionsPanel)**: Configuración del juego
- **Panel de Créditos (CreditsPanel)**: screen de créditos
- **Panel de Selección de Modo (ModeSelectPanelUI)**: elección Story/Endless
- **Camera Rig**: Cámara que se mueve entre secciones

### Flujo de Pantallas

```
MainPanel (índice 0)
    ├── Options → OptionsPanel (índice 2) → Volver a MainPanel
    ├── Credits → CreditsPanel (índice 3) → Volver a MainPanel
    └── New Game → ModeSelectPanelUI (índice 1) → SelectionCallback → StartNewGame
```

**Section Indices:**
- `mainSectionIndex = 0` (Main Panel)
- `modeSelectSectionIndex = 1` (Mode Select)
- `optionsSectionIndex = 2` (Options)
- `creditsSectionIndex = 3` (Credits)

### Configuración en Unity

#### 1. Escena del Menú Principal

Crear o verificar que existe `MenuScene.unity` con:

```
Hierarchy:
├── EventSystem
├── MainCamera
├── Canvas
│   ├── MainPanel (CanvasGroup)
│   │   ├── TitleText
│   │   ├── ContinueButton
│   │   ├── NewGameButton
│   │   ├── OptionsButton
│   │   ├── CreditsButton
│   │   └── QuitButton
│   ├── OptionsPanel (CanvasGroup)
│   │   └── [ contenido de opciones ]
│   ├── CreditsPanel (CanvasGroup)
│   │   └── [ contenido de créditos ]
│   ├── ModeSelectPanel (ModeSelectPanelUI)
│   │   ├── CanvasGroup
│   │   ├── PanelRect
│   │   ├── StoryButton
│   │   ├── EndlessButton
│   │   ├── EndlessLockedOverlay
│   │   └── BackButton
│   └── ConfirmPopup (ConfirmPopupUI)
├── MenuCameraRig
└── GameManager (prefab en escena)
```

#### 2. Configurar MainMenuManager

En el objeto que tenga **MainMenuManager**, asignar en Inspector:

```
MainMenuManager:
├── Camera: [arrerastar MenuCameraRig]
├── Main Panel: [arrerastar MainPanel]
├── Options Panel: [arrerastar OptionsPanel]
├── Credits Panel: [arrerastar CreditsPanel]
├── Mode Select Panel: [arrerastar ModeSelectPanel]
├── Confirm Popup: [arrerastar ConfirmPopup]
├── Continue Button: [arrerastar ContinueButton]
├── New Game Button: [arrerastar NewGameButton]
├── Options Button: [arrerastar OptionsButton]
├── Credits Button: [arrerastar CreditsButton]
├── Quit Button: [arrerastar QuitButton]
└── Section Indices:
    ├── Main Section Index: 0
    ├── Mode Select Section Index: 1
    ├── Options Section Index: 2
    └── Credits Section Index: 3
```

#### 3. Configurar MenuCameraRig

El MenuCameraRig debe tener tantos puntos de anclaje (anchors) como secciones existan:

- **Anchor 0**: Posición para Main Panel
- **Anchor 1**: Posición para Mode Select
- **Anchor 2**: Posición para Options
- **Anchor 3**: Posición para Credits

Cada anchor es un Transform posicionado en la escena quedefine dónde se mueve la cámara.

```
MenuCameraRig:
├── Camera (Camera component)
└── Anchors (array de transforms):
    ├── Anchor[0]: Position for Main
    ├── Anchor[1]: Position for Mode Select
    ├── Anchor[2]: Position for Options
    └── Anchor[3]: Position for Credits
```

#### 4. Scene Loader y GameManager en Build

**Importante**:确保todas las escenas están en Build Settings:

```
File → Build Settings → Add Open Scenes
```

Añadir:
- MenuScene.unity
- Shop.unity (o nombre de escena de tienda)
- Game.unity (o nombre de escena de juego)

### Referencia de Scripts MainMenuManager

#### MainMenuManager.cs

```csharp
public class MainMenuManager : MonoBehaviour
{
    // Referencias a paneles
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup optionsPanel;
    [SerializeField] private CanvasGroup creditsPanel;
    [SerializeField] private ModeSelectPanelUI modeSelectPanel;
    [SerializeField] private ConfirmPopupUI confirmPopup;

    // Índices de cámara
    [SerializeField] private int mainSectionIndex = 0;
    [SerializeField] private int modeSelectSectionIndex = 1;
    [SerializeField] private int optionsSectionIndex = 2;
    [SerializeField] private int creditsSectionIndex = 3;

    // Animación
    [SerializeField] private float panelFadeDuration = 0.25f;

    // Métodos públicos
    public void GoBackToMain()  // Vuelve al panel principal
}
```

**Flujo cuando se presiona New Game:**

1. Si existe partida guardada → Mostrar ConfirmPopup
2. Al confirmar → Llama a ShowModeSelect()
3. ShowModeSelect() → Mueve cámara a modeSelectSectionIndex + oculta mainPanel + muestra modeSelectPanel
4. Usuario selecciona modo (Story/Endless)
5. OnModeSelected() → Llama a GameManager.StartNewGame(mode)

---

## Sistema de Cinemáticas

### Arquitectura General

El sistema de cinemáticas permite mostrar viñetas (paneles gráficos) antes/después de las rondas en modo Historia. Hay 3 componentes principales:

1. **ComicPanelSystem**: Maneja la UI de las viñetas y el efecto de difuminado
2. **CinematicManager**:Orquestra qué viñetas mostrar y cuándo
3. **RoundTransitionManager**: Coordina la transición entre ronda y tienda

### Flujo de Viñetas

#### Modo Historia:

```
Nueva Partida → Viñetas Intro (opcionales)
    ↓
Game Scene + Ronda 1
    ↓
[GANAR RONDA]
    ↓
RoundScorePanel
    ↓
[CONTINUAR]
    ↓
Verificar si hay viñetas para ronda + 1
    ↓
SÍ → Game Scene + Viñetas → Ronda X + 1
NO → Shop Scene
    ↓
[...]
    ↓
[Ganar última ronda]
    ↓
Preguntar: ¿Pasar a Endless?
    ↓
SÍ → ConvertToEndless() → Continuar sin viñetas
NO → Fin del juego
```

#### Modo Endless:

```
Nueva Partida → Game Scene + Ronda 1
    (SIN viñetas en ningún momento)
```

### Configuración en Unity

#### 1. ComicPanelSystem - Estructura Requerida

En la **escena de Game**, crear:

```
Game Scene Hierarchy:
└── Canvas
    └── ComicPanelSystem (GameObject)
        ├── CanvasGroup (para fades)
        ├── PanelBackground (Image - sprite de fondo/default)
        ├── PanelImage (Image - sprite de viñeta actual)
        ├── CaptionText (TMP_Text - texto de viñeta)
        ├── ContinueButton (Button - avanzar a siguiente viñeta)
        └── SkipButton (Button - saltarse todas)
```

**Configuración de ComicPanelSystem en Inspector:**

```
ComicPanelSystem:
├── Canvas Group: [arrerastar CanvasGroup]
├── Panel Image: [arrerastar Image de viñeta]
├── Panel Background: [arrerastar Image de fondo]
├── Caption Text: [arrerastar TMP_Text]
├── Continue Button: [arrerastar Button]
├── Skip Button: [arrerastar Button]
├── Fade Duration: 0.4
└── Paneles (Lista de ComicPanel):
    └── [ver sección 2]
```

#### 2. Configurar Viñetas de Intro

En **CinematicManager** (puede estar en el mismo objeto ComicPanelSystem o separado):

```
CinematicManager:
├── Intro Panels (List<ComicPanel>):
│   ├── Element 0:
│   │   ├── Image: [arrastrar sprite]
│   │   ├── Caption: "Texto de viñeta 1"
│   │   └── Display Duration: 3
│   ├── Element 1:
│   │   ├── Image: [arrastrar sprite]
│   │   ├── Caption: "Texto de viñeta 2"
│   │   └── Display Duration: 3
│   └── ...
```

**Importante**: Si no hay viñetas de intro configuradas o la lista está vacía, el juego saltará directamente a la ronda 1.

#### 3. Configurar Viñetas por Ronda

Para mostrar viñetas DESPUÉS de una ronda específica:

```
CinematicManager:
├── Comic System: [arrerastar ComicPanelSystem]
└── Round Cinematics (List<RoundCinematic>):
    └── Element 0 (RoundCinematic):
        ├── Round After: 1  (mostrar después de ronda 1)
        └── Panels (List<ComicPanel>):
            ├── Element 0:
            │   ├── Image: [arrastrar sprite]
            │   ├── Caption: "Viñeta post-ronda 1"
            │   └── Display Duration: 3
            └── ...
```

**Regla**: `roundAfter = X` significa "mostrar estas viñetas después de completar la ronda X".

Ejemplos:
- `roundAfter = 1`: Viñetas entre ronda 1 y 2
- `roundAfter = 5`: Viñetas entre ronda 5 y 6
- `roundAfter = 10`: Viñetas después de la última ronda (fin del modo Historia)

#### 4. Configurar WinLoseHandler

En la escena de Game, asignar:

```
WinLoseHandler:
├── Game Over Screen: [arrerastar GameOverScreen]
├── Score Panel: [arrerastar RoundScorePanel]
└── Cinematic Manager: [arrerastar CinematicManager/ComicPanelSystem]
```

#### 5. Configurar GameManager

En la escena persistente (o en el GameManager prefab):

```
GameManager:
├── Main Menu Scene: "MenuScene"
├── Shop Scene: "Shop"
├── Game Scene: "Game"
└── Cinematic Manager: [arrerastar CinematicManager de Game Scene]
```

### Referencia de Scripts CinematicManager

#### ComicPanelSystem.cs

```csharp
public class ComicPanelSystem : MonoBehaviour
{
    [System.Serializable]
    public class ComicPanel
    {
        public Sprite image;        // Sprite de la viñeta
        public string caption;       // Texto a mostrar
        public float displayDuration = 3f;  // Tiempo máximo
    }

    // Referencias UI
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image panelImage;
    [SerializeField] private Image panelBackground;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button skipButton;

    // Duración del efecto de difuminado
    [SerializeField] private float fadeDuration = 0.4f;

    // Lista de viñetas (para debug o uso directo)
    [SerializeField] private List<ComicPanel> panels;

    // MÉTODO PRINCIPAL - Reproducir viñetas
    public IEnumerator Play(List<ComicPanel> overridePanels = null, Action onComplete = null)
}
```

**Uso típico:**

```csharp
// Reproducir viñetas específicas con callback
StartCoroutine(comicSystem.Play(miListaDePaneles, () => {
    Debug.Log("¡Todas las viñetas completadas!");
}));
```

#### CinematicManager.cs

```csharp
public class CinematicManager : MonoBehaviour
{
    [System.Serializable]
    private class RoundCinematic
    {
        public int roundAfter;  // Número de ronda
        public List<ComicPanelSystem.ComicPanel> panels;
    }

    // Viñetas de intro (antes de ronda 1)
    [SerializeField] private List<ComicPanelSystem.ComicPanel> introPanels;

    // Sistema de UI
    [SerializeField] private ComicPanelSystem comicSystem;

    // Viñetas por ronda
    [SerializeField] private List<RoundCinematic> cinematics;

    // Reproducir intro antes del juego
    public void PlayIntroCinematic(Action onComplete)

    // Reproducir viñetas después de una ronda
    public void PlayRoundCinematic(int completedRound, Action onComplete)

    // Verificar si hay viñetas para una ronda
    public bool HasCinematicForRound(int roundNumber)
}
```

**Uso típico:**

```csharp
// Intro antes de ronda 1
cinematicManager.PlayIntroCinematic(() => {
    WaveManager.Instance.StartRound(1);
});

// Después de ganar una ronda
cinematicManager.PlayRoundCinematic(rondaActual, () => {
    // Ir a tienda o siguiente ronda
});
```

#### RoundTransitionManager.cs

```csharp
public class RoundTransitionManager : MonoBehaviour
{
    // Referencia al manager de cinemáticas
    public void SetCinematicManager(CinematicManager manager)

    // Iniciar transición (llamado por WinLoseHandler)
    public void BeginTransition(int completedRound)
}
```

### Casos de Uso y Comportamiento

| Escenario | Comportamiento |
|-----------|-------------|
| Modo Historia, sin viñetas configuradas | Salta directo a ronda 1 |
| Modo Historia, con introPanels | Muestra viñetas intro, luego Ronda 1 |
| Modo Endless | Salta directo a ronda 1 (sin viñetas) |
| Ganar ronda sin viñetas para siguiente | Va directo a Shop |
| Ganar ronda CON viñetas para siguiente | Carga Game + muestra viñetas + siguiente ronda |
| Fin del modo Historia | Muestra panel de conversión a Endless |

### Personalización de Sprites (Imágenes de Viñetas)

Si no tienes sprites creados aún, el sistema usará:

- **panelImage.sprite**: La imagen asignada en cada ComicPanel
- **fallback**: Imagen por defecto de Unity (rosa/cube) si no hay sprite

Para crear viñetas:

1. Crear sprites en cualquier software de edición de imágenes
2. Importar a Unity (drag & drop o File → Import)
3. Asignar en Inspector de CinematicManager

---

## Resumen de Configuración Mínima

Para que funcione el sistema completo:

### Menú Principal:

1. ✅ MenuScene en Build Settings
2. ✅ Game en Build Settings
3. ✅ Shop en Build Settings (opcional)
4. ✅ MainMenuManager configurado
5. ✅ MenuCameraRig con 4 anchors

### Cinemáticas (Modo Historia):

1. ✅ ComicPanelSystem en Game Scene
2. ✅ CinematicManager configurado
3. ✅ WinLoseHandler con referencia a CinematicManager
4. ✅ GameManager con referencia a CinematicManager
5. ✅ Sprites en introPanels (opcional)
6. ✅ RoundCinematics para cada ronda (opcional)

---

## Notas Adicionales

- ** Tiempo.timeScale = 0**: El sistema de viñetas utiliza `Time.unscaledDeltaTime` para funcionar incluso cuando el juego está en pausa
- **Callbacks**: El sistema usa lambdas para encadenar acciones (mostrar → esperar input → siguiente acción)
- **Flexibilidad**: Se puede tener 0, 1 o X viñetas por sección sin romper el sistema
- **Fallback**: Si no hay viñetas, el flujo continúa normalmente