# 🎯 Système de Capture d'Écran Avancé

## 🚀 Fonctionnalités Implémentées

### 1. 📋 Modes de Capture Disponibles

| Mode | Description | Configuration |
|------|-------------|---------------|
| **FullScreen** | 🖥️ Capture l'écran principal complet | `"CaptureMode": "FullScreen"` |
| **AllScreens** | 🖥️ Capture tous les écrans connectés | `"CaptureMode": "AllScreens"` |
| **SpecificWindow** | 🪟 Capture une fenêtre spécifique | `"CaptureMode": "SpecificWindow"` |
| **CustomRegion** | 📐 Capture une région personnalisée | `"CaptureMode": "CustomRegion"` |

### 2. 🔧 Configuration appsettings.json

```json
{
  "Vision": {
    "CaptureMode": "FullScreen",
    "WindowTitle": "Visual Studio Code",
    "CustomRegion": {
      "X": 100,
      "Y": 100,
      "Width": 800,
      "Height": 600
    },
    "AllowRegionSelection": true,
    "ShowCapturePreview": false
  }
}
```

### 3. 🎮 Utilisation

#### Mode Ligne de Commande
```bash
# Lancer l'interface de sélection de source
.\Core.exe --select-capture

# Mode normal avec configuration JSON
.\Core.exe
```

#### Configuration par Code
```csharp
// Mode plein écran
screenAnalyzer.SetCaptureMode(CaptureMode.FullScreen);

// Capture d'une fenêtre spécifique
screenAnalyzer.SetTargetWindow(windowHandle, "Titre Fenêtre");

// Région personnalisée
var region = new CaptureRegion { X = 0, Y = 0, Width = 800, Height = 600 };
screenAnalyzer.SetCustomRegion(region);

// Multi-écrans
screenAnalyzer.SetTargetScreen(0); // Index de l'écran
```

### 4. 🖼️ Interface Graphique de Sélection

L'interface graphique permet :
- ✅ **Sélection du mode de capture** via boutons radio
- ✅ **Liste des fenêtres disponibles** avec détails (titre, processus, dimensions)
- ✅ **Sélecteur de région interactif** avec aperçu en temps réel
- ✅ **Aperçu des dimensions** pendant la sélection
- ✅ **Actualisation automatique** des sources disponibles

#### Fonctionnalités du Sélecteur de Région
- **Sélection par glisser-déposer** : Cliquez et faites glisser pour définir la zone
- **Affichage des dimensions** : Affichage en temps réel de la taille sélectionnée
- **Annulation** : Échap pour annuler la sélection
- **Validation** : Clic pour confirmer la région

### 5. 🏗️ Architecture Technique

#### Classes Principales

**`CaptureSourceSelector`** - Gestion des sources disponibles
- Énumération des fenêtres Windows
- Détection des écrans multiples  
- Interface de sélection de région

**`OpenCvScreenAnalyzer`** - Moteur de capture
- Support multi-modes
- Optimisations Windows API
- Gestion des erreurs robuste

**`CaptureSourceForm`** - Interface graphique
- WinForms pour sélection intuitive
- Prévisualisation en temps réel
- Validation des paramètres

#### API Windows Utilisées
- **EnumWindows** : Énumération des fenêtres
- **GetWindowRect** : Dimensions des fenêtres
- **PrintWindow** : Capture directe de fenêtre
- **CopyFromScreen** : Capture d'écran fallback

### 6. 🎯 Avantages du Système

#### ✅ **Flexibilité Maximale**
- Configuration via JSON ou interface
- Changement à chaud sans redémarrage
- Support multi-écrans natif

#### ✅ **Performance Optimisée**
- Capture directe des fenêtres (PrintWindow)
- Fallback intelligent sur échec
- Gestion mémoire optimisée

#### ✅ **Facilité d'Utilisation**
- Interface graphique intuitive
- Configuration par glisser-déposer
- Prévisualisation immédiate

#### ✅ **Robustesse**
- Gestion d'erreurs complète
- Validation des paramètres
- Mode de secours automatique

### 7. 🔧 Exemples de Configuration

#### Capture de Navigateur Web
```json
{
  "Vision": {
    "CaptureMode": "SpecificWindow",
    "WindowTitle": "Google Chrome"
  }
}
```

#### Région Personnalisée Fixe
```json
{
  "Vision": {
    "CaptureMode": "CustomRegion",
    "CustomRegion": {
      "X": 0,
      "Y": 0, 
      "Width": 1920,
      "Height": 540
    }
  }
}
```

#### Multi-Écrans
```json
{
  "Vision": {
    "CaptureMode": "AllScreens"
  }
}
```

### 8. 🚀 Utilisation Avancée

#### Sélection Interactive
1. Lancez `.\Core.exe --select-capture`
2. Choisissez votre mode de capture
3. Sélectionnez la source (fenêtre/région)
4. Visualisez le test de capture
5. La configuration est appliquée automatiquement

#### Configuration Programmatique
```csharp
// Récupérer les fenêtres disponibles
var selector = new CaptureSourceSelector();
var windows = selector.GetAvailableWindows();

// Trouver une fenêtre spécifique
var targetWindow = windows.FirstOrDefault(w => 
    w.Title.Contains("Visual Studio", StringComparison.OrdinalIgnoreCase));

// Configurer la capture
if (targetWindow != null)
{
    screenAnalyzer.SetTargetWindow(targetWindow.Handle, targetWindow.Title);
}
```

### 9. 🛠️ Troubleshooting

#### Problèmes Courants

**Fenêtre non détectée**
- Vérifiez que la fenêtre est visible
- Essayez le mode FullScreen en fallback
- Actualisez la liste des fenêtres

**Région invalide**
- Vérifiez les coordonnées dans appsettings.json
- Utilisez l'interface graphique pour sélectionner
- Assurez-vous que la région est dans les limites de l'écran

**Permissions d'accès**
- Exécutez en tant qu'Administrateur
- Vérifiez les paramètres de confidentialité Windows
- Autorisez l'accès à l'enregistrement d'écran

### 10. 🎉 Résultat Final

🎯 **Système de capture d'écran complet et flexible** permettant :
- ✅ Capture de toutes les sources possibles
- ✅ Configuration intuitive via interface graphique
- ✅ Intégration transparente avec le système d'IA
- ✅ Performance optimisée pour l'analyse en temps réel
- ✅ Robustesse et gestion d'erreurs avancée

**Le système est maintenant prêt pour une utilisation professionnelle avec une flexibilité maximale !** 🚀
