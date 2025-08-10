# Real-Time Intelligence - Interface Blazor

## 🎯 Description
Application Blazor Server moderne pour le système de surveillance en temps réel avec capture d'écran, reconnaissance vocale, OCR et intelligence artificielle.

## ✨ Fonctionnalités

### 🖥️ Interface moderne
- Dashboard responsive avec Bootstrap 5
- Cartes de statut en temps réel
- Animations et transitions fluides
- Icônes Font Awesome

### 📊 Modules de surveillance
- **Capture d'écran** : Surveillance continue de l'écran
- **Microphone** : Reconnaissance vocale en temps réel
- **OCR** : Extraction de texte des images
- **Intelligence artificielle** : Analyse et traitement des données

### 🎛️ Contrôles
- Démarrage/arrêt du système complet
- Diagnostic automatique des modules
- Journal d'activité en temps réel
- Interface entièrement en français

## 🚀 Démarrage rapide

### Prérequis
- .NET 7.0 ou supérieur
- Navigateur web moderne

### Installation et lancement

1. **Lancement automatique** (recommandé)
   ```cmd
   launch-blazor-simple.bat
   ```

2. **Lancement manuel**
   ```cmd
   cd RealTimeIntelligence.Web
   dotnet restore
   dotnet run --urls "https://localhost:7001"
   ```

3. **Accès à l'application**
   - Ouvrir https://localhost:7001 dans votre navigateur
   - L'application se lance automatiquement

## 🧹 Maintenance

### Nettoyage du projet
```cmd
clean-project.bat
```
Supprime tous les fichiers temporaires, dossiers bin/obj et anciens projets.

## 📁 Structure du projet

```
RealTimeIntelligence.Web/           # Projet Blazor Server principal
├── Pages/                          # Pages Razor
│   ├── Index.razor                 # Page d'accueil avec dashboard
│   ├── _Host.cshtml               # Page hôte HTML
│   └── Error.cshtml               # Page d'erreur
├── Shared/                         # Composants partagés
│   ├── MainLayout.razor           # Layout principal
│   └── NavMenu.razor              # Menu de navigation
├── wwwroot/                        # Fichiers statiques
│   ├── css/                       # Styles CSS
│   └── favicon.png                # Icône de l'application
├── Program.cs                      # Point d'entrée de l'application
└── appsettings.json               # Configuration

Scripts utilitaires:
├── launch-blazor-simple.bat       # Script de lancement
├── clean-project.bat              # Script de nettoyage
└── README.md                      # Documentation (ce fichier)
```

## 🎨 Interface utilisateur

### Dashboard principal
- **Cartes de statut** : Affichage visuel de l'état de chaque module
- **Panneau de contrôle** : Boutons de démarrage/arrêt et diagnostic
- **Journal d'activité** : Historique des événements en temps réel

### Design
- Interface responsive (mobile, tablette, desktop)
- Thème moderne avec Bootstrap 5
- Animations CSS pour une expérience fluide
- Icônes Font Awesome pour une meilleure UX

## 🔧 Configuration

### Ports et URLs
- **Développement** : https://localhost:7001
- **Production** : Configurable dans `appsettings.json`

### Personnalisation
- Styles CSS : `wwwroot/css/site.css`
- Configuration : `appsettings.json`
- Layout : `Shared/MainLayout.razor`

## 🎯 Utilisation

1. **Démarrer l'application** avec `launch-blazor-simple.bat`
2. **Accéder au dashboard** via https://localhost:7001
3. **Cliquer sur "Démarrer le système"** pour activer tous les modules
4. **Surveiller les statuts** via les cartes colorées
5. **Consulter le journal** pour voir les activités en temps réel
6. **Utiliser le diagnostic** pour vérifier le bon fonctionnement

## 🔍 Diagnostic

Le système inclut un diagnostic automatique qui vérifie :
- ✅ Fonctionnement de la capture d'écran
- ✅ État du microphone
- ✅ Disponibilité du moteur OCR  
- ✅ Connexion aux services IA

## ⚡ Performance

- **Démarrage rapide** : ~2-3 secondes
- **Interface réactive** : Mises à jour en temps réel
- **Faible consommation** : Optimisé pour une utilisation continue

## 🎨 Thème et couleurs

- **Primaire** : Bleu (#0d6efd) - Système principal
- **Succès** : Vert (#198754) - Capture d'écran active
- **Info** : Cyan (#0dcaf0) - Microphone actif
- **Attention** : Jaune (#ffc107) - OCR actif
- **Danger** : Rouge (#dc3545) - Erreurs ou alertes

## 📝 Notes techniques

- **Framework** : ASP.NET Core Blazor Server 7.0
- **UI** : Bootstrap 5 + Font Awesome 6
- **Architecture** : Server-side rendering pour de meilleures performances
- **Compatibilité** : Windows, macOS, Linux

## 🔄 Mises à jour futures

Prochaines fonctionnalités prévues :
- Intégration SignalR pour les mises à jour temps réel
- Capture d'écran en direct dans l'interface
- Configuration des paramètres via l'interface
- Export des journaux d'activité
- Notification push des événements importants

---

*Développé avec ❤️ pour une surveillance intelligente et moderne*
