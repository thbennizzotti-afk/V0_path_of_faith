#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Tools.Validation
{
    [CreateAssetMenu(menuName = "Path of Faith/Project Validation Config", fileName = "ProjectValidationConfig")]
    public class ProjectValidationConfig : ScriptableObject
    {
        [Header("Répertoires requis (créés si manquants)")]
        public List<string> requiredFolders = new()
        {
            "Assets/Scenes",
            "Assets/Prefabs",
            "Assets/Art",
            "Assets/Resources",
            "Assets/Scripts",
            "Assets/Scripts/Fondation",
            "Assets/Scripts/Fondation/Core",
            "Assets/Scripts/Fondation/Exploration",
            "Assets/Scripts/Combats"
        };

        [Header("Layers requis (créés si manquants)")]
        public List<string> requiredLayers = new()
        {
            "Ground",
            "Clickable",
            "Enemy",
            "UI",
            "Obstacle"
        };

        [Header("Tags requis (créés si manquants)")]
        public List<string> requiredTags = new()
        {
            "Player",
            "Enemy",
            "Interactable",
            "NPC"
        };

        [Header("Scenes in Build (ajoutées si existantes)")]
        [Tooltip("Chemins d'assets *.unity (ex: Assets/Scenes/Boot.unity)")]
        public List<string> requiredScenePaths = new()
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Game.unity"
        };

        [Header("Scripting Define Symbols (ajoutés si manquants)")]
        public List<string> requiredDefineSymbols = new()
        {
            "POF_DEV",
            "USE_INPUT_SYSTEM"
        };

        [Header("Règles d'éditeur")]
        public bool enforceForceTextSerialization = true;
        public bool enforceVisibleMetaFiles = true;
        public bool enforceLinearColorSpace = true;

        [Header("Alerte (informative)")]
        public bool warnIfNoRenderPipelineAsset = true;
        public bool warnIfNoInputSystem = true;

        [Header("Comportement")]
        [Tooltip("Si vrai, on bloque l'entrée en Play s'il y a des erreurs non corrigées.")]
        public bool blockEnterPlayModeOnErrors = true;
    }
}
#endif
