# Sistema de Cómic Interactivo - Documento Técnico Completo

## 1. VISIÓN GENERAL DEL SISTEMA

Este sistema permite mostrar cómics interactivos estilo "Gravity Rush" donde:
- Las **Páginas** contienen múltiples **Viñetas**
- Las viñetas aparecen una por una con **Fade In**
- Las viñetas anteriores **permanecen visibles**
- El jugador controla el ritmo con botones **Continuar** y **Saltar**

---

## 2. JERARQUÍA DE ESCENA - GRÁFICO VISUAL

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CANVAS (Canvas)                               │
│  Render Mode: Screen - Overlay                                      │
│  Sort Order: 10                                                     │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   COMIC MANAGER (GameObject)                         │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │ Script: ComicManager.cs                                      │   │
│  │ - pages: List<ComicPage>                                     │   │
│  │ - continueButton: Button                                     │   │
│  │ - skipButton: Button                                         │   │
│  │ - fadeDuration: 0.3                                          │   │
│  │ - pageFadeDuration: 0.4                                      │   │
│  └───────────────────────────────────────────────────────────────┘   │
│                                  │                                   │
│                                  ▼                                   │
│  ┌──────────────────┐  ┌──────────────────┐                        │
│  │ ContinueButton   │  │ SkipButton      │  (UI Buttons)          │
│  │ (Button)         │  │ (Button)        │                        │
│  └──────────────────┘  └──────────────────┘                        │
│                                  │                                   │
│                                  ▼                                   │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ PAGE_1 (GameObject) - ComicPage.cs                         │    │
│  │ ┌─────────────────────────────────────────────────────────┐  │    │
│  │ │ Script: ComicPage.cs                                    │  │    │
│  │ │ - pageTitle: "Acto 1 - El Comienzo"                    │  │    │
│  │ │ - vignettes: List<ComicVignette>                       │  │    │
│  │ └─────────────────────────────────────────────────────────┘  │    │
│  │                          │                                    │    │
│  │                          ▼                                    │    │
│  │  ┌─────────────────────────────────────────────────────┐    │    │
│  │  │ TitleText (TextMeshPro - TMP_Text)                 │    │    │
│  │  └─────────────────────────────────────────────────────┘    │    │
│  │                          │                                    │    │
│  │                          ▼                                    │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐       │    │
│  │  │ VIGNETTE_1  │  │ VIGNETTE_2  │  │ VIGNETTE_3  │       │    │
│  │  │ (GameObject)│  │ (GameObject)│  │ (GameObject)│       │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘       │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                  │                                   │
│                                  ▼                                   │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ PAGE_2 (GameObject) - ComicPage.cs                         │    │
│  │ (Misma estructura que PAGE_1)                               │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                  │                                   │
│                                  ▼                                   │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ PAGE_N (GameObject) - ComicPage.cs                         │    │
│  │ (Misma estructura que PAGE_1)                               │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 3. ESTRUCTURA DE CADA SCRIPT Y SUS COMPONENTES

### 3.1 ComicManager (GameObject Raíz)

**Nombre del GameObject:** `ComicManager`

**Componentes requeridos:**

| Componente | Tipo | Configuración |
|------------|------|---------------|
| ComicManager | Script | **ASIGNAR** |

**Campos del Script (Inspector):**

```
ComicManager (Script)
├── Pages (List<ComicPage>)
│   ├── Element 0: [arrastrar PAGE_1]
│   ├── Element 1: [arrastrar PAGE_2]
│   └── Element N: [arrastrar PAGE_N]
│
├── Continue Button: [arrastrar ContinueButton]
├── Skip Button: [arrastrar SkipButton]
│
├── Fade Duration: 0.3
└── Page Fade Duration: 0.4
```

---

### 3.2 ComicPage (GameObject Hijo)

**Nombre sugerido:** `Page_1`, `Page_2`, etc.

**Jerarquía correcta:**
```
ComicManager
├── Page_1 (GameObject)
│   ├── TitleText (GameObject)
│   │   └── TMP_Text
│   └── Vignette_1 (GameObject)
│       └── CanvasGroup
│       └── Image
```

**Componentes requeridos:**

| Componente | Tipo | Configuración |
|------------|------|---------------|
| ComicPage | Script | **ASIGNAR** |
| RectTransform | Component | Full Screen (o según diseño) |

**Campos del Script (Inspector):**

```
ComicPage (Script)
├── Page Title: "Acto 1 - El Comienzo"
├── Title Text: [arrastrar GameObject con TMP_Text]
│
└── Vignettes (List<ComicVignette>)
    ├── Element 0: [arrastrar VIGNETTE_1]
    ├── Element 1: [arrastrar VIGNETTE_2]
    └── Element N: [arrastrar VIGNETTE_N]
```

---

### 3.3 ComicVignette (GameObject Hijo de Page)

**Nombre sugerido:** `Vignette_1`, `Vignette_2`, etc.

**Jerarquía correcta:**
```
Page_1 (ComicPage)
├── TitleText
│   └── TMP_Text
├── Vignette_1 (GameObject)
│   ├── CanvasGroup
│   └── Image
└── Vignette_2 (GameObject)
    ├── CanvasGroup
    └── Image
```

**Componentes requeridos:**

| Componente | Tipo | Configuración |
|------------|------|---------------|
| ComicVignette | Script | **ASIGNAR** |
| CanvasGroup | Component | **ASIGNAR** - Alpha inicial: 0 |
| Image | Component | **ASIGNAR** - Sprite de la viñeta |
| RectTransform | Component | Posicionar en-editor según diseño |

**Campos del Script (Inspector):**

```
ComicVignette (Script)
├── Canvas Group: [arrastrar CanvasGroup del mismo GameObject]
├── Image: [arrastrar Image del mismo GameObject]
│
├── Has Dialogue: [toggle - opcional]
└── Dialogue Text: [arrastrar TMP_Text si tiene diálogo]
```

---

### 3.4 Buttons (UI)

**Continuar Button:**
```
ContinueButton (GameObject)
├── RectTransform
├── CanvasGroup (opcional para fades)
├── Image (Source Image: None o default)
└── Button
    └── On Click: [NO asignar - el script lo hace automáticamente]
```

**Skip Button:**
```
SkipButton (GameObject)
├── RectTransform
├── Image
└── Button
```

---

## 4. GUÍA DE CONFIGURACIÓN EN UNITY - PASOS EXACTOS

### Paso 1: Crear el Canvas

1. En Hierarchy: Click derecho → UI → Canvas
2. Configurar:
   - **Render Mode**: Screen - Overlay
   - **Sort Order**: 10 (o mayor si hay otros canvas)

### Paso 2: Crear ComicManager

1. En Canvas: Click derecho → Create Empty
2. Nombre: `ComicManager`
3. Añadir componente: **ComicManager** (script)
4. **NO activar** (dejar desactivado initially)

### Paso 3: Crear Botones

1. En Canvas: Click derecho → UI → Button
2. Nombre: `ContinueButton`
3. Posicionar donde quieras (esquina inferior derecha)
4. En Inspector: TextMeshPro - UGUI Child si pide crear
5. Repetir para `SkipButton`

### Paso 4: Asignar Botones en ComicManager

1. Seleccionar `ComicManager`
2. Arrastrar `ContinueButton` al campo **Continue Button**
3. Arrastrar `SkipButton` al campo **Skip Button**

### Paso 5: Crear Primera Página

1. En ComicManager: Click derecho → Create Empty
2. Nombre: `Page_1`
3. Añadir componente: **ComicPage** (script)
4. En Inspector de ComicPage:
   - **Page Title**: "Acto 1 - El Comienzo"

### Paso 6: Crear Título de Página

1. En Page_1: Click derecho → UI → Text - TextMeshPro
2. Nombre: `TitleText`
3. Configurar texto, fuente, tamaño
4. Posicionar arriba de la página

### Paso 7: Asignar Título

1. Seleccionar `Page_1`
2. Arrastrar `TitleText` al campo **Title Text** de ComicPage

### Paso 8: Crear Primera Viñeta

1. En Page_1: Click derecho → Create Empty
2. Nombre: `Vignette_1`
3. Añadir componente: **CanvasGroup**
   - **Alpha**: 0 (IMPORTANTE)
4. Añadir componente: **Image**
   - **Source Image**: Arrastrar sprite de viñeta
   - **Color**: White
5. Añadir componente: **ComicVignette** (script)
6. Configurar script:
   - **Canvas Group**: Arrastrar CanvasGroup del mismo objeto
   - **Image**: Arrastrar Image del mismo objeto
7. Posicionar RectTransform según diseño de página

### Paso 9: Repetir para Más Viñetas

1. Duplicar `Vignette_1` (Ctrl+D)
2. Renombrar: `Vignette_2`, `Vignette_3`, etc.
3. Cambiar Image Source en cada una
4. Posicionar según layout del cómic

### Paso 10: Asignar Viñetas en Page

1. Seleccionar `Page_1`
2. En Inspector de ComicPage:
   - Expandir lista **Vignettes**
   - Arrastrar cada viñeta a la lista:
     - Element 0: `Vignette_1`
     - Element 1: `Vignette_2`
     - etc.

### Paso 11: Asignar Página en Manager

1. Seleccionar `ComicManager`
2. En Inspector de ComicManager:
   - Expandir lista **Pages**
   - Element 0: Arrastrar `Page_1`

### Paso 12: Crear Más Páginas (Opcional)

1. Duplicar `Page_1`
2. Renombrar: `Page_2`, `Page_3`, etc.
3. Modificar título y viñetas según historia
4. Añadir a lista Pages en ComicManager

---

## 5. INTEGRACIÓN CON GAME MANAGER

### 5.1 Configuración en GameManager

1. Seleccionar objeto con **GameManager** (en escena persistente o prefab)
2. En Inspector:
   - **Comic Manager**: Arrastrar objeto `ComicManager` de la escena Game

### 5.2 Configuración en WinLoseHandler

1. Seleccionar objeto con **WinLoseHandler** (en escena Game)
2. En Inspector:
   - **Comic Manager**: Arrastrar objeto `ComicManager`

---

## 6. FLUJO COMPLETO DE EJECUCIÓN

### Cuando inicia el juego (Modo Historia):

```
1. Menú → New Game (Historia)
           │
           ▼
2. GameManager.StartNewGame(Story)
           │
           ▼
3. SceneLoader.LoadScene("Game")
           │
           ▼
4. Callback: comicManager.Play()
           │
           ▼
5. ComicManager activa su gameObject
           │
           ▼
6. Para cada Page (excepto primera): SetActive(false)
           │
           ▼
7. Page_1.Initialize() → Todas las viñetas con Alpha = 0
           │
           ▼
8. Page_1.FadeIn(0, 0.3) → Primera viñeta aparece
           │
           ▼
9. [ESPERAR INPUT DEL JUGADOR]
```

### Jugador presiona Continuar (mientras viñeta visible):

```
10. Si NO hay animación en curso:
    │
    ├── Si hay más viñetas en página:
    │   └── Page_1.FadeIn(1, 0.3) → Viñeta 2 aparece (1 sigue visible)
    │
    ├── Si es última viñeta de página Y hay más páginas:
    │   └── Transición: Page_1 oculta → Page_2 muestra primera viñeta
    │
    └── Si es última viñeta de última página:
        └── Finish() → OnFinished callback
```

### Jugador presiona Saltar:

```
10. Cancelled = true
    │
    └── Para cada página: Page.ShowAll() (todas visibles instantáneo)
    │
    └── Finish() → OnFinished callback
```

### Callback hacia GameManager:

```
OnFinished →
    WaveManager.StartRound(1)
        │
        ▼
    [JUGAR - MATAR ENEMIGOS]
        │
        ▼
    [GANAR RONDA]
        │
        ▼
    RoundScorePanel.Show()
        │
        ▼
    Continuar → RoundTransitionManager
        │
        ▼
    [Si modo historia Y hay más rondas]
        │
        ▼
    SceneLoader.LoadScene("Game")
        │
        ▼
    ComicManager.PlayRound(rondaActual + 1)
        │
        ▼
    [REPETIR DESDE PASO 5]
```

---

## 7. RESUMEN DE SCRIPTS Y SUS RESPONSABILIDADES

| Script | Dónde asignarlo | Qué hace |
|--------|----------------|----------|
| **ComicManager** | GameObject raíz | Controla flujo, input, transiciones |
| **ComicPage** | GameObject de página | Gestiona viñetas de esa página |
| **ComicVignette** | GameObject de viñeta | Animaciones fade de cada viñeta |

---

## 8. CHECKLIST DE VERIFICACIÓN

Antes de ejecutar, verificar:

- [ ] Canvas existe en escena
- [ ] ComicManager tiene script asignado
- [ ] ContinueButton asignado en ComicManager
- [ ] SkipButton asignado en ComicManager
- [ ] Page_1 tiene script ComicPage asignado
- [ ] Page_1.TitleText tiene TMP_Text
- [ ] TitleText asignado en ComicPage de Page_1
- [ ] Vignette_1 tiene CanvasGroup (Alpha = 0)
- [ ] Vignette_1 tiene Image con sprite
- [ ] Vignette_1 tiene script ComicVignette
- [ ] CanvasGroup e Image asignados en ComicVignette
- [ ] Vignette_1 asignado en lista Vignettes de Page_1
- [ ] Page_1 asignado en lista Pages de ComicManager
- [ ] GameManager tiene referencia a ComicManager
- [ ] WinLoseHandler tiene referencia a ComicManager

---

## 9. NOTAS IMPORTANTES

1. **Alpha Inicial**: Toda viñeta debe tener CanvasGroup con Alpha = 0 al inicio
2. **Posicionamiento**: Las viñetas se posicionan manualmente en el Editor
3. **Orden de Viñetas**: El orden en la lista determina el orden de aparición
4. **Imágenes Default**: Si no hay sprite, usar "Knob" o cualquier default de Unity
5. **Time.timeScale**: El sistema funciona con juego en pausa (DOTween usa unscaled time)
