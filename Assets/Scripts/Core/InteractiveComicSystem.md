# Sistema de Cómic Interactivo - Documento Técnico

## Índice

1. [Arquitectura General](#arquitectura-general)
2. [Jerarquía de Escena](#jerarquía-de-escena)
3. [Configuración en Unity](#configuración-en-unity)
4. [Flujo de Páginas por Ronda](#flujo-de-páginas-por-ronda)
5. [Referencia de APIs](#referencia-de-apis)

---

## Arquitectura General

El sistema de cómics maneja **viñetas por ronda**. Cada ronda del modo Historia puede tener su propio conjunto de páginas.

### Conceptos Clave:

| Concepto | Descripción |
|---------|-------------|
| **Intro Pages** | Páginas mostradas al inicio (Ronda 1) |
| **Round Comics** | Conjunto de páginas por cada ronda |
| **ComicPage** | Una página individual con título y viñetas |
| **ComicVignette** | Una viñeta individual con CanvasGroup para fade |

---

## Jerarquía de Escena

### GameScene:

```
GameScene
│
├── Canvas
│   └── ComicManager (GameObject)
│       ├── ComicManager.cs
│       │
│       ├── ContinueButton (Button)
│       ├── SkipButton (Button)
│       │
│       ├── IntroPages (Lista - para Ronda 1)
│       │   ├── Page_Intro_1
│       │   │   └── ComicPage.cs
│       │   │       ├── TitleText
│       │   │       └── Vignette_1, 2...
│       │   │           └── ComicVignette.cs + CanvasGroup + Image
│       │   └── Page_Intro_2...
│       │
│       ├── RoundComics (Lista)
│       │   └── RoundComic_1 (para Ronda 1)
│       │       ├── Round Number: 1
│       │       └── Pages: [Page_R1_1, Page_R1_2...]
│       │   └── RoundComic_2 (para Ronda 2)
│       │       ├── Round Number: 2
│       │       └── Pages: [Page_R2_1...]
│       │
│       └── Page_RN_X (cada página individual)
│           ├── ComicPage.cs
│           ├── TitleText
│           └── Vignette_1, 2...
│
├── GameLoopOrchestrator
├── WaveManager
├── EnemySpawner
└── Player
```

---

## Configuración en Unity

### 1. ComicManager - Inspector:

```
ComicManager (Script)
│
├── Continue Button: [arrastrar Button]
├── Skip Button: [arrastrar Button]
│
├── Fade Duration: 0.3
├── Page Fade Duration: 0.4
│
├── Use Intro Only Mode: [toggle]
│   └── Si TRUE: usa IntroPages para TODAS las rondas
│   └── Si FALSE: usa RoundComics para cada ronda
│
├── Intro Pages (List<ComicPage>)
│   ├── Element 0: [arrastrar Page_Intro_1]
│   ├── Element 1: [arrastrar Page_Intro_2]
│   └── ...
│
└── Round Comics (List<RoundComic>)
    ├── Element 0:
    │   ├── Round Number: 1
    │   └── Pages: [Page_R1_1, Page_R1_2]
    ├── Element 1:
    │   ├── Round Number: 2
    │   └── Pages: [Page_R2_1, Page_R2_2]
    └── ...
```

### 2. ComicPage - Inspector:

```
Page_R1_1 (GameObject)
├── ComicPage.cs
│
├── Title: "Acto 1 - El Comienzo"
├── Title Text: [arrastrar TMP_Text]
│
└── Vignettes (List<ComicVignette>)
    ├── Element 0: [arrastrar Vignette_1]
    ├── Element 1: [arrastrar Vignette_2]
    └── ...
```

### 3. ComicVignette - Inspector:

```
Vignette_1 (GameObject)
├── ComicVignette.cs
├── Canvas Group: [arrastrar CanvasGroup]
│   └── Alpha: 0 (inicial)
├── Image: [arrastrar Image]
│   └── Source Image: [sprite de viñeta]
└── RectTransform: [posicionar en editor]
```

---

## Flujo de Páginas por Ronda

### Modo Historia Completo:

```
Menu → New Game (Historia)
    ↓
GameLoopOrchestrator.Start()
    ↓
GameManager.IsStoryMode = TRUE
    ↓
[Si Use Intro Only Mode = TRUE]
    ↓
ComicManager.Play(introPages)
    ↓
[Si Use Intro Only Mode = FALSE]
    ↓
ComicManager.PlayRound(1, roundComics[1].pages)
    ↓
[Mostrar Page_Intro_1, Page_Intro_2...]
    ↓
OnFinished → WaveManager.StartRound(1)
    ↓
[JUGAR - Matar enemigos]
    ↓
[Ganar Ronda 1]
    ↓
RoundScorePanel → Continue
    ↓
GameLoopOrchestrator.NextRound()
    ↓
[Si Use Intro Only Mode = TRUE]
    ↓
ComicManager.Play(introPages)
    ↓
[Si Use Intro Only Mode = FALSE]
    ↓
ComicManager.PlayRound(2, roundComics[2].pages)
    ↓
[Mostrar Page_R2_1, Page_R2_2...]
    ↓
OnFinished → WaveManager.StartRound(2)
    ↓
...repetir...
```

### Modo Infinito:

```
Menu → New Game (Infinito)
    ↓
ComicManager.Play(introPages) → OnFinished
    ↓
WaveManager.StartRound(1)
    ↓
[JUGAR]
    ↓
[Ganar] → WaveManager.StartRound(2)
    ↓
...SIN cómics intermedios...
```

---

## Referencia de APIs

### ComicManager

```csharp
public class ComicManager : MonoBehaviour
{
    public static ComicManager Instance;
    public static bool IsPlaying;

    public event Action OnFinished;

    // Configuración
    [SerializeField] private List<RoundComic> roundComics;
    [SerializeField] private List<ComicPage> introPages;
    [SerializeField] private bool useIntroOnly = false;

    // Métodos públicos
    public void Play() { }                    // Usa introPages
    public void Play(Action onComplete) { }   // Usa introPages con callback
    public void PlayRound(int round, Action onComplete) { }  // Usa páginas de esa ronda
    public bool HasComicContent() { }        // Verifica si hay contenido
    public bool HasPagesForRound(int round) { }  // Verifica si hay páginas para esa ronda
}
```

### RoundComic

```csharp
[Serializable]
public class RoundComic
{
    public int roundNumber;           // Número de ronda (1, 2, 3...)
    public List<ComicPage> pages;   // Páginas para esta ronda
}
```

### Uso en GameLoopOrchestrator:

```csharp
private void StartCinematicSequence()
{
    int round = _waveManager != null ? _waveManager.CurrentRound : 1;
    _comicManager.OnFinished += HandleCinematicFinished;
    _comicManager.PlayRound(round, null);
}
```

---

## Checklist de Configuración

- [ ] ComicManager en GameScene
- [ ] ContinueButton y SkipButton asignados
- [ ] IntroPages con al menos una página
- [ ] Round Comics configurados para cada ronda
- [ ] Cada ComicPage tiene título y viñetas
- [ ] Cada ComicVignette tiene CanvasGroup (alpha = 0)
- [ ] Use Intro Only Mode según preferencia
- [ ] GameLoopOrchestrator en escena
- [ ] Player referenciado para bloqueo de input
- [ ] EnemySpawner referenciado para bloqueo de spawning