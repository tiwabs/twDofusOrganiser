# twDofusOrganiser

**twDofusOrganiser** est un outil Windows permettant de gérer et de naviguer facilement entre plusieurs fenêtres Dofus Unity.

## Fonctionnalités

### Navigation rapide
- **Raccourcis clavier globaux** : Naviguez entre vos fenêtres Dofus avec des raccourcis personnalisables (fenêtre suivante/précédente)
- **Activation automatique** : L'outil met au premier plan la fenêtre suivante ou précédante automatiquement

### Gestion des fenêtres
- **Détection automatique** : Scan automatique de toutes les fenêtres Dofus Unity ouvertes
- **Réorganisation** : Glissez-déposez les fenêtres dans l'interface pour définir votre ordre de navigation
- **Activation sélective** : Activez ou désactivez individuellement les fenêtres que vous souhaitez inclure dans la navigation
- **Identification visuelle** : Affichage du nom du personnage et de l'icône de classe

### Configuration persistante
- **Sauvegarde automatique** : L'ordre des fenêtres, les états actifs/inactifs et les raccourcis clavier sont sauvegardés automatiquement
- **Restauration au démarrage** : Votre configuration est rechargée à chaque lancement

## Prérequis

- **Windows** : L'application utilise l'API Win32 et n'est compatible qu'avec Windows
- **Dofus Unity** : Fonctionne uniquement avec la version Unity de Dofus (pas la version Retro pour le moment)
- **.NET 8.0 Runtime** : Requis pour exécuter l'application

## Installation

1. Téléchargez la dernière version depuis la page [Releases](https://github.com/tiwabs/twDofusOrganiser/releases/latest)
2. Extrayez l'archive dans un dossier de votre choix
3. Lancez `twDofusOrganiser.exe`

Aucune installation supplémentaire n'est nécessaire.

## Utilisation

### Premier lancement

1. **Lancez vos clients Dofus** avant d'ouvrir l'organisateur
2. **Ouvrez twDofusOrganiser**
3. Cliquez sur le bouton de rafraîchissement pour détecter de nouvelles fenêtres Dofus
4. Organisez vos fenêtres dans l'ordre souhaité par glisser-déposer

### Configuration des raccourcis clavier

1. Cliquez sur le bouton "Précédent" ou "Suivant" pour configurer un raccourci
2. Appuyez sur la combinaison de touches souhaitée (ex: `Ctrl + Shift + Left`)
3. Le raccourci est automatiquement enregistré
4. Pour supprimer un raccourci, faites un clic droit sur le bouton correspondant

### Activation du mode organisateur

1. Cliquez sur le bouton menu (☰) en haut à droite
2. Sélectionnez "Activer"
3. L'application se réduit dans la barre système et les raccourcis clavier deviennent actifs
4. Vous pouvez maintenant naviguer entre vos fenêtres Dofus avec vos raccourcis configurés

### Désactivation de fenêtres

Décochez la case à côté d'une fenêtre pour l'exclure de la navigation sans la supprimer de la liste.

## Sécurité et transparence

Le code source de ce projet est **entièrement open source** et disponible sur GitHub. Vous pouvez :

- Consulter le code source pour vérifier son fonctionnement
- Compiler vous-même l'application à partir des sources

### Autorisations utilisées

L'application utilise l'API Win32 pour :
- **Énumérer les fenêtres** : Détecter les fenêtres Dofus ouvertes
- **Activer les fenêtres** : Mettre au premier plan la fenêtre sélectionnée
- **Enregistrer des raccourcis clavier globaux** : Capturer vos raccourcis même quand l'application est en arrière-plan

**Aucune donnée n'est collectée ni envoyée sur Internet.** Toute la configuration est stockée localement dans un fichier `config.json`.

## Limitations connues

- Fonctionne uniquement avec la **version Unity de Dofus**
- **Windows uniquement** (utilise l'API Win32)
- Les fenêtres Dofus doivent être ouvertes **avant** le lancement de l'organisateur (ou cliquez sur rafraîchir après les avoir ouvertes)

## Développement

### Technologies utilisées

- **.NET 8.0** avec **WPF** pour l'interface graphique
- **Win32 API** pour la gestion des fenêtres
- **System.Text.Json** pour la sérialisation de la configuration

## Contribution

Les contributions sont les bienvenues ! N'hésitez pas à :

- Signaler des bugs via les [Issues](https://github.com/tiwabs/twDofusOrganiser/issues)
- Proposer des améliorations
- Soumettre des Pull Requests

## Licence

Ce projet est distribué sous licence **Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)**.

Cela signifie que vous pouvez :
- Utiliser, modifier et partager ce logiciel
- L'adapter à vos besoins

**Sous réserve que :**
- Vous ne l'utilisiez **pas à des fins commerciales**
- Vous créditiez l'auteur original
- Vous partagiez vos modifications sous la même licence

Consultez le fichier `LICENSE` pour plus de détails.

## Aperçu

Pour voir l'application en action, consultez la [vidéo de démonstration](https://youtu.be/Ub91uiVU-Lw).

## Support

Si vous rencontrez des problèmes ou avez des questions :

1. Vérifiez que vous utilisez bien la version Unity de Dofus
2. Assurez-vous d'avoir le .NET 8.0 Runtime installé
3. Consultez les [Issues](https://github.com/tiwabs/twDofusOrganiser/issues) existantes
4. Ouvrez une nouvelle Issue si votre problème n'a pas encore été signalé
