#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// APIs modernes
using UnityEditor.Build;             // NamedBuildTarget
using UnityEditor.VersionControl;    // VersionControlSettings (mode est une string)

namespace PathOfFaith.Tools.Validation
{
    public enum ValidationSeverity { Info, Warning, Error }

    public class ValidationResult
    {
        public ValidationSeverity severity;
        public string message;
        public Action fixAction; // null si non corrigeable automatiquement
        public bool IsFixable => fixAction != null;
    }

    public static class ProjectValidator
    {
        const string CONFIG_TYPE_SEARCH = "t:PathOfFaith.Tools.Validation.ProjectValidationConfig";
        const string PREFS_BLOCK_PLAY   = "POF_ProjectValidator_BlockPlay";

        public static ProjectValidationConfig FindConfig()
        {
            var guids = AssetDatabase.FindAssets(CONFIG_TYPE_SEARCH);
            if (guids.Length == 0) return null;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ProjectValidationConfig>(path);
        }

        public static List<ValidationResult> Validate(ProjectValidationConfig cfg, bool withFixes = false)
        {
            var results = new List<ValidationResult>();
            if (cfg == null)
            {
                results.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Error,
                    message = "Aucune ProjectValidationConfig trouvée. Créez-en une via: Assets/Create/Path of Faith/Project Validation Config."
                });
                return results;
            }

            // Dossiers
            results.AddRange(CheckFolders(cfg, withFixes));

            // Layers & Tags
            results.AddRange(CheckLayers(cfg, withFixes));
            results.AddRange(CheckTags(cfg, withFixes));

            // Scenes in Build
            results.AddRange(CheckScenesInBuild(cfg, withFixes));

            // Define symbols (API modernisée)
            results.AddRange(CheckDefineSymbols(cfg, withFixes));

            // Project settings clés
            results.AddRange(CheckProjectSettings(cfg, withFixes));

            // Avertissements optionnels
            if (cfg.warnIfNoRenderPipelineAsset)
                results.Add(CheckRenderPipelineAssigned());

            if (cfg.warnIfNoInputSystem)
                results.Add(CheckInputSystemAvailable());

            // Présence du Bootstrapper (type)
            results.Add(CheckBootstrapperType());

            return results;
        }

        #region Folders
        static IEnumerable<ValidationResult> CheckFolders(ProjectValidationConfig cfg, bool withFixes)
        {
            foreach (var folder in cfg.requiredFolders.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        message = $"Dossier manquant: {folder}",
                        fixAction = withFixes ? (() => CreateFolderPath(folder)) : (Action)null
                    };
                }
                else
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Info,
                        message = $"Dossier OK: {folder}"
                    };
                }
            }
        }

        static void CreateFolderPath(string folderPath)
        {
            // Crée récursivement ex: "Assets/Scripts/Fondation/Core"
            var parts = folderPath.Split('/');
            if (parts.Length < 2 || parts[0] != "Assets") return;
            var current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
            AssetDatabase.Refresh();
        }
        #endregion

        #region Layers & Tags
        static IEnumerable<ValidationResult> CheckLayers(ProjectValidationConfig cfg, bool withFixes)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");

            foreach (var layer in cfg.requiredLayers)
            {
                if (string.IsNullOrWhiteSpace(layer)) continue;

                bool exists = false;
                for (int i = 0; i < layersProp.arraySize; i++)
                {
                    var sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp != null && sp.stringValue == layer) { exists = true; break; }
                }

                if (!exists)
                {
                    Action fixer = null;
                    if (withFixes)
                    {
                        fixer = () =>
                        {
                            // Cherche un slot libre (>=8)
                            for (int i = 8; i < layersProp.arraySize; i++)
                            {
                                var sp = layersProp.GetArrayElementAtIndex(i);
                                if (string.IsNullOrEmpty(sp.stringValue))
                                {
                                    sp.stringValue = layer;
                                    tagManager.ApplyModifiedProperties();
                                    AssetDatabase.SaveAssets();
                                    return;
                                }
                            }
                            Debug.LogError($"Aucun slot de Layer disponible pour '{layer}'. Libérez un slot dans le TagManager.");
                        };
                    }

                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        message = $"Layer manquant: {layer}",
                        fixAction = fixer
                    };
                }
                else
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Info,
                        message = $"Layer OK: {layer}"
                    };
                }
            }
        }

        static IEnumerable<ValidationResult> CheckTags(ProjectValidationConfig cfg, bool withFixes)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");

            foreach (var tag in cfg.requiredTags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;

                bool exists = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    var sp = tagsProp.GetArrayElementAtIndex(i);
                    if (sp != null && sp.stringValue == tag) { exists = true; break; }
                }

                if (!exists)
                {
                    Action fixer = null;
                    if (withFixes)
                    {
                        fixer = () =>
                        {
                            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                            tagManager.ApplyModifiedProperties();
                            AssetDatabase.SaveAssets();
                        };
                    }

                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        message = $"Tag manquant: {tag}",
                        fixAction = fixer
                    };
                }
                else
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Info,
                        message = $"Tag OK: {tag}"
                    };
                }
            }
        }
        #endregion

        #region Scenes in Build
        static IEnumerable<ValidationResult> CheckScenesInBuild(ProjectValidationConfig cfg, bool withFixes)
        {
            var current = EditorBuildSettings.scenes.Select(s => s.path).ToHashSet();
            foreach (var path in cfg.requiredScenePaths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                if (!File.Exists(path))
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Warning,
                        message = $"Scene référencée introuvable sur disque: {path} (crée-la ou corrige le chemin dans la config)."
                    };
                    continue;
                }

                if (!current.Contains(path))
                {
                    Action fixer = null;
                    if (withFixes)
                    {
                        fixer = () =>
                        {
                            var list = EditorBuildSettings.scenes.ToList();
                            list.Add(new EditorBuildSettingsScene(path, true));
                            EditorBuildSettings.scenes = list.ToArray();
                        };
                    }

                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        message = $"Scene non ajoutée au Build Settings: {path}",
                        fixAction = fixer
                    };
                }
                else
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Info,
                        message = $"Scene OK dans Build: {path}"
                    };
                }
            }
        }
        #endregion

        #region Define Symbols (API moderne)
        static IEnumerable<ValidationResult> CheckDefineSymbols(ProjectValidationConfig cfg, bool withFixes)
        {
            var group = UnityEditor.BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

#if UNITY_2021_2_OR_NEWER
            var nbt = NamedBuildTarget.FromBuildTargetGroup(group);
            PlayerSettings.GetScriptingDefineSymbols(nbt, out string[] definesArr);
#else
            PlayerSettings.GetScriptingDefineSymbolsForGroup(group, out string[] definesArr);
#endif

            var defines = new HashSet<string>(definesArr ?? Array.Empty<string>());

            foreach (var def in cfg.requiredDefineSymbols.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                if (!defines.Contains(def))
                {
                    Action fixer = null;
                    if (withFixes)
                    {
                        fixer = () =>
                        {
                            defines.Add(def);
#if UNITY_2021_2_OR_NEWER
                            PlayerSettings.SetScriptingDefineSymbols(nbt, defines.ToArray());
#else
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines.ToArray());
#endif
                        };
                    }

                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        message = $"Define symbol manquant: {def}",
                        fixAction = fixer
                    };
                }
                else
                {
                    yield return new ValidationResult
                    {
                        severity = ValidationSeverity.Info,
                        message = $"Define symbol OK: {def}"
                    };
                }
            }
        }
        #endregion

        #region Project Settings clés
        static IEnumerable<ValidationResult> CheckProjectSettings(ProjectValidationConfig cfg, bool withFixes)
        {
            // Serialization
            if (cfg.enforceForceTextSerialization && EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                yield return new ValidationResult
                {
                    severity = ValidationSeverity.Error,
                    message = "Serialization Mode n'est pas 'Force Text'.",
                    fixAction = withFixes ? (() =>
                    {
                        EditorSettings.serializationMode = SerializationMode.ForceText;
                    }) : null
                };
            }
            else
            {
                yield return new ValidationResult { severity = ValidationSeverity.Info, message = "Serialization Mode OK (Force Text)." };
            }

            // Visible Meta Files (API modernisée — string)
#if UNITY_2021_2_OR_NEWER
            if (cfg.enforceVisibleMetaFiles && VersionControlSettings.mode != "Visible Meta Files")
            {
                yield return new ValidationResult
                {
                    severity = ValidationSeverity.Error,
                    message = "Version Control n'est pas 'Visible Meta Files'.",
                    fixAction = withFixes ? (() =>
                    {
                        VersionControlSettings.mode = "Visible Meta Files";
                    }) : null
                };
            }
            else
            {
                yield return new ValidationResult { severity = ValidationSeverity.Info, message = "Version Control OK (Visible Meta Files)." };
            }
#else
            if (cfg.enforceVisibleMetaFiles && EditorSettings.externalVersionControl != "Visible Meta Files")
            {
                yield return new ValidationResult
                {
                    severity = ValidationSeverity.Error,
                    message = "Version Control n'est pas 'Visible Meta Files'.",
                    fixAction = withFixes ? (() =>
                    {
                        EditorSettings.externalVersionControl = "Visible Meta Files";
                    }) : null
                };
            }
            else
            {
                yield return new ValidationResult { severity = ValidationSeverity.Info, message = "Version Control OK (Visible Meta Files)." };
            }
#endif

            // Color Space
            if (cfg.enforceLinearColorSpace && PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                yield return new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = "Color Space n'est pas Linear.",
                    fixAction = withFixes ? (() =>
                    {
                        PlayerSettings.colorSpace = ColorSpace.Linear;
                    }) : null
                };
            }
            else
            {
                yield return new ValidationResult { severity = ValidationSeverity.Info, message = "Color Space OK (Linear)." };
            }
        }
        #endregion

        #region Avertissements optionnels
        static ValidationResult CheckRenderPipelineAssigned()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null)
            {
                return new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = "Aucun Render Pipeline Asset défini (URP/HDRP). Assigne-en un dans Project Settings/Graphics."
                };
            }
            return new ValidationResult { severity = ValidationSeverity.Info, message = "Render Pipeline Asset trouvé." };
        }

        static ValidationResult CheckInputSystemAvailable()
        {
            var hasInput = AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(a => a.GetType("UnityEngine.InputSystem.InputAction", false) != null);

            if (!hasInput)
            {
                return new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = "Package Input System non détecté. Installe et active l'Input System si le projet l'exige."
                };
            }
            return new ValidationResult { severity = ValidationSeverity.Info, message = "Input System détecté." };
        }

        static ValidationResult CheckBootstrapperType()
        {
            var found = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType("PathOfFaith.Fondation.Core.Bootstrapper", false))
                .Any(t => t != null);

            if (!found)
            {
                return new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = "Type 'PathOfFaith.Fondation.Core.Bootstrapper' introuvable. Vérifie la présence du script et son namespace."
                };
            }
            return new ValidationResult { severity = ValidationSeverity.Info, message = "Bootstrapper détecté (type trouvé)." };
        }
        #endregion

        #region Play Mode Guard
        [InitializeOnLoadMethod]
        static void SetupPlayModeGuard()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChange;
            EditorApplication.playModeStateChanged += OnPlayModeChange;
        }

        static void OnPlayModeChange(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;

            var cfg = FindConfig();
            if (cfg == null) return;

            // Préférence utilisateur (fenêtre) prime sur l'asset
            bool block = GetBlockPlayPref(cfg.blockEnterPlayModeOnErrors);
            if (!block) return;

            var results = Validate(cfg, withFixes: false);
            bool hasErrors = results.Any(r => r.severity == ValidationSeverity.Error);
            if (hasErrors)
            {
                if (EditorUtility.DisplayDialog(
                    "Project Validator",
                    "Des erreurs de configuration ont été détectées. Corrige-les (Auto-Fix possible) avant de lancer Play.",
                    "Ouvrir le Validator", "Ignorer et annuler Play"))
                {
                    ShowWindow();
                }
                // Empêche l’entrée en Play
                EditorApplication.isPlaying = false;
            }
        }
        #endregion

        #region UI Window
        [MenuItem("Tools/Path of Faith/Project Validator")]
        public static void ShowWindow()
        {
            ProjectValidatorWindow.ShowWindow();
        }

        public static void SetBlockPlayPref(bool value)
        {
            EditorPrefs.SetBool(PREFS_BLOCK_PLAY, value);
        }

        public static bool GetBlockPlayPref(bool defaultValue)
        {
            return EditorPrefs.GetBool(PREFS_BLOCK_PLAY, defaultValue);
        }
        #endregion
    }
}
#endif
