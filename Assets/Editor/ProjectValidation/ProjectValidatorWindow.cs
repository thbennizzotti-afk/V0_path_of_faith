#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PathOfFaith.Tools.Validation
{
    public class ProjectValidatorWindow : EditorWindow
    {
        Vector2 _scroll;
        List<ValidationResult> _results = new();
        GUIStyle _msgStyle;

        // Filtres
        bool _showInfo = true;
        bool _showWarning = true;
        bool _showError = true;
        string _textFilter = string.Empty;

        // Cache des compteurs
        int _countInfo, _countWarning, _countError;

        public static void ShowWindow()
        {
            var wnd = GetWindow<ProjectValidatorWindow>("Project Validator");
            wnd.minSize = new Vector2(720, 480);
            wnd.Refresh();
            wnd.Show();
        }

        void OnEnable()
        {
            _msgStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
        }

        void OnGUI()
        {
            var cfg = ProjectValidator.FindConfig();

            DrawHeader(cfg);
            EditorGUILayout.Space(6);

            DrawToolbar(cfg);
            EditorGUILayout.Space(4);

            DrawSummaryAndFilters(cfg);
            EditorGUILayout.Space(6);

            DrawResultsList();
        }

        void DrawHeader(ProjectValidationConfig cfg)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (cfg == null)
                {
                    EditorGUILayout.HelpBox(
                        "Aucun ProjectValidationConfig trouvé. Créez-le via Assets/Create/Path of Faith/Project Validation Config.",
                        MessageType.Error);

                    if (GUILayout.Button("Créer Config par défaut", GUILayout.Width(220)))
                        CreateDefaultConfig();
                }
                else
                {
                    EditorGUILayout.ObjectField("Config", cfg, typeof(ProjectValidationConfig), false);
                }
            }
        }

        void DrawToolbar(ProjectValidationConfig cfg)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = cfg != null;

                if (GUILayout.Button("Validate (sans correction)", GUILayout.Height(26)))
                    _results = ProjectValidator.Validate(cfg, withFixes: false);

                if (GUILayout.Button("Auto-Fix (corriger quand possible)", GUILayout.Height(26)))
                {
                    _results = ProjectValidator.Validate(cfg, withFixes: true);

                    // Exécute toutes les actions fixables retournées par la passe.
                    foreach (var r in _results.Where(r => r.IsFixable))
                        SafeInvokeFix(r);

                    // Re-valider pour afficher l’état final.
                    _results = ProjectValidator.Validate(cfg, withFixes: false);
                }

                if (GUILayout.Button("Corriger tout (fixables)", GUILayout.Height(26)))
                {
                    foreach (var r in _results.Where(r => r.IsFixable))
                        SafeInvokeFix(r);

                    var refreshed = cfg != null ? ProjectValidator.Validate(cfg, withFixes: false) : null;
                    if (refreshed != null) _results = refreshed;
                }

                if (GUILayout.Button("Copier le rapport (Markdown)", GUILayout.Height(26)))
                {
                    var md = BuildMarkdownReport(_results);
                    EditorGUIUtility.systemCopyBuffer = md;
                    ShowNotification(new GUIContent("Rapport copié dans le presse-papiers"));
                }

                GUI.enabled = true;
            }

            if (cfg != null)
            {
                var block = ProjectValidator.GetBlockPlayPref(cfg.blockEnterPlayModeOnErrors);
                var newBlock = EditorGUILayout.ToggleLeft("Bloquer l'entrée en Play s'il y a des erreurs", block);
                if (newBlock != block) ProjectValidator.SetBlockPlayPref(newBlock);
            }
        }

        void DrawSummaryAndFilters(ProjectValidationConfig cfg)
        {
            // Recalcule les compteurs
            (_countInfo, _countWarning, _countError) = CountBySeverity(_results);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Résumé", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    var labelInfo = $"Info: {_countInfo}";
                    var labelWarn = $"Warnings: {_countWarning}";
                    var labelErr  = $"Erreurs: {_countError}";

                    // Filtres de sévérité
                    _showInfo = GUILayout.Toggle(_showInfo, labelInfo, "Button");
                    _showWarning = GUILayout.Toggle(_showWarning, labelWarn, "Button");
                    _showError = GUILayout.Toggle(_showError, labelErr, "Button");

                    GUILayout.FlexibleSpace();

                    // Filtre texte
                    EditorGUILayout.LabelField("Filtre texte:", GUILayout.Width(80));
                    _textFilter = EditorGUILayout.TextField(_textFilter ?? string.Empty, GUILayout.MinWidth(160));
                }

                // Aide
                EditorGUILayout.HelpBox(
                    "Astuce : utilise les boutons Info/Warnings/Erreurs pour masquer/afficher. Le filtre texte recherche dans le message.",
                    MessageType.Info);
            }
        }

        void DrawResultsList()
        {
            EditorGUILayout.LabelField("Résultats", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var display = FilteredResults(_results, _showInfo, _showWarning, _showError, _textFilter);

            if (display == null || display.Count == 0)
            {
                EditorGUILayout.HelpBox("Aucun résultat à afficher. Clique sur 'Validate' pour lancer une vérification.", MessageType.Info);
            }
            else
            {
                foreach (var r in display)
                    DrawResult(r);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawResult(ValidationResult r)
        {
            MessageType type = MessageType.None;
            switch (r.severity)
            {
                case ValidationSeverity.Info:    type = MessageType.Info; break;
                case ValidationSeverity.Warning: type = MessageType.Warning; break;
                case ValidationSeverity.Error:   type = MessageType.Error; break;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox(r.message, type);

            if (r.IsFixable)
            {
                if (GUILayout.Button("Corriger"))
                {
                    SafeInvokeFix(r);
                    var cfg = ProjectValidator.FindConfig();
                    _results = ProjectValidator.Validate(cfg, withFixes: false);
                }
            }

            EditorGUILayout.EndVertical();
        }

        void SafeInvokeFix(ValidationResult r)
        {
            try
            {
                r.fixAction?.Invoke();
            }
            catch (System.SystemException ex)
            {
                Debug.LogError($"[ProjectValidator] Échec de correction pour: {r.message}\n{ex}");
            }
        }

        void Refresh()
        {
            var cfg = ProjectValidator.FindConfig();
            if (cfg != null)
                _results = ProjectValidator.Validate(cfg, withFixes: false);
        }

        static void CreateDefaultConfig()
        {
            var asset = ScriptableObject.CreateInstance<ProjectValidationConfig>();
            var path = "Assets/ProjectValidationConfig.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        static (int info, int warn, int err) CountBySeverity(List<ValidationResult> list)
        {
            if (list == null || list.Count == 0) return (0, 0, 0);
            int i = 0, w = 0, e = 0;
            foreach (var r in list)
            {
                switch (r.severity)
                {
                    case ValidationSeverity.Info: i++; break;
                    case ValidationSeverity.Warning: w++; break;
                    case ValidationSeverity.Error: e++; break;
                }
            }
            return (i, w, e);
        }

        static List<ValidationResult> FilteredResults(List<ValidationResult> src, bool showInfo, bool showWarning, bool showError, string text)
        {
            if (src == null) return new List<ValidationResult>(0);

            var q = src.AsEnumerable();

            // Filtre sévérité
            q = q.Where(r =>
                (showInfo && r.severity == ValidationSeverity.Info) ||
                (showWarning && r.severity == ValidationSeverity.Warning) ||
                (showError && r.severity == ValidationSeverity.Error));

            // Filtre texte
            if (!string.IsNullOrEmpty(text))
            {
                var t = text.ToLowerInvariant();
                q = q.Where(r => (r.message ?? string.Empty).ToLowerInvariant().Contains(t));
            }

            return q.ToList();
        }

        static string BuildMarkdownReport(List<ValidationResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Project Validator - Rapport");
            sb.AppendLine();
            if (results == null || results.Count == 0)
            {
                sb.AppendLine("_Aucun résultat. Lancez une validation._");
                return sb.ToString();
            }

            var (i, w, e) = (results.Count(r => r.severity == ValidationSeverity.Info),
                             results.Count(r => r.severity == ValidationSeverity.Warning),
                             results.Count(r => r.severity == ValidationSeverity.Error));

            sb.AppendLine($"- **Infos**: {i}");
            sb.AppendLine($"- **Warnings**: {w}");
            sb.AppendLine($"- **Erreurs**: {e}");
            sb.AppendLine();

            foreach (var r in results)
            {
                var tag = r.severity switch
                {
                    ValidationSeverity.Info => "INFO",
                    ValidationSeverity.Warning => "WARN",
                    ValidationSeverity.Error => "ERROR",
                    _ => "UNK"
                };
                var fix = r.IsFixable ? " _(fixable)_" : string.Empty;
                sb.AppendLine($"- **[{tag}]** {EscapeMd(r.message)}{fix}");
            }

            return sb.ToString();
        }

        static string EscapeMd(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            // Échappement basique pour *, _, `, [
            return s.Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace("[", "\\[");
        }
    }
}
#endif
