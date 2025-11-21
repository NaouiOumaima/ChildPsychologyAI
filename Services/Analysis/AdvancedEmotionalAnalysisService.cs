using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;

namespace ChildPsychologyAI.Services.Analysis;

public class AdvancedEmotionalAnalysisService : IAdvancedEmotionalAnalysisService
{
    private readonly ILogger<AdvancedEmotionalAnalysisService> _logger;

    public AdvancedEmotionalAnalysisService(ILogger<AdvancedEmotionalAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<AdvancedEmotionalAnalysis> AnalyzeEmotionsAsync(ColorAnalysis colors, AdvancedShapeAnalysis shapes)
    {
        return await Task.Run(() =>
        {
            var analysis = new AdvancedEmotionalAnalysis();

            try
            {
                analysis.EmotionScores = CalculateEmotionScores(colors, shapes);
                analysis.DominantEmotion = analysis.EmotionScores.OrderByDescending(e => e.Value).First().Key;
                analysis.EmotionalState = AssessEmotionalState(analysis.EmotionScores);
                analysis.Indicators = GenerateEmotionalIndicators(analysis, colors, shapes);
                analysis.RiskAssessment = AssessRisksAsync(analysis).Result;

                _logger.LogInformation($"Analyse émotionnelle avancée: {analysis.DominantEmotion}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'analyse émotionnelle avancée");
            }

            return analysis;
        });
    }

    private Dictionary<string, double> CalculateEmotionScores(ColorAnalysis colors, AdvancedShapeAnalysis shapes)
    {
        var scores = new Dictionary<string, double>
        {
            ["joy"] = 0.0,
            ["sadness"] = 0.0,
            ["anger"] = 0.0,
            ["fear"] = 0.0,
            ["calm"] = 0.0,
            ["energy"] = 0.0,
            ["anxiety"] = 0.0,
            ["love"] = 0.0
        };

        try
        {
            // 1. Scores des couleurs
            foreach (var (color, percentage) in colors.ColorDistribution)
            {
                var colorScores = GetColorEmotionScores(color, percentage);
                foreach (var (emotion, score) in colorScores)
                {
                    scores[emotion] += score;
                }
            }

            // 2. Scores des formes et symboles
            AdjustScoresWithAdvancedShapes(scores, shapes);

            // 3. Scores de la composition
            AdjustScoresWithComposition(scores, shapes.Composition);

            // Normalisation
            var maxScore = scores.Values.Max();
            if (maxScore > 0)
            {
                foreach (var emotion in scores.Keys.ToList())
                {
                    scores[emotion] = Math.Min(scores[emotion] / maxScore, 1.0);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des scores émotionnels");
        }

        return scores;
    }

    private Dictionary<string, double> GetColorEmotionScores(string color, double percentage)
    {
        var weight = percentage / 100.0;

        return color.ToLower() switch
        {
            "red" => new Dictionary<string, double>
            {
                ["anger"] = 0.8 * weight,
                ["energy"] = 0.7 * weight,
                ["joy"] = 0.2 * weight
            },
            "blue" => new Dictionary<string, double>
            {
                ["calm"] = 0.9 * weight,
                ["sadness"] = 0.6 * weight,
                ["fear"] = 0.3 * weight
            },
            "yellow" => new Dictionary<string, double>
            {
                ["joy"] = 0.9 * weight,
                ["energy"] = 0.7 * weight
            },
            "green" => new Dictionary<string, double>
            {
                ["calm"] = 0.8 * weight,
                ["joy"] = 0.4 * weight
            },
            "black" => new Dictionary<string, double>
            {
                ["sadness"] = 0.8 * weight,
                ["anxiety"] = 0.9 * weight,
                ["fear"] = 0.5 * weight
            },
            "white" => new Dictionary<string, double>
            {
                ["calm"] = 0.6 * weight,
                ["fear"] = 0.3 * weight
            },
            "orange" => new Dictionary<string, double>
            {
                ["joy"] = 0.7 * weight,
                ["energy"] = 0.8 * weight
            },
            "purple" => new Dictionary<string, double>
            {
                ["calm"] = 0.5 * weight,
                ["anxiety"] = 0.4 * weight
            },
            "brown" => new Dictionary<string, double>
            {
                ["calm"] = 0.6 * weight,
                ["sadness"] = 0.2 * weight
            },
            "pink" => new Dictionary<string, double>
            {
                ["love"] = 0.8 * weight,
                ["joy"] = 0.5 * weight,
                ["calm"] = 0.4 * weight
            },
            "gray" => new Dictionary<string, double>
            {
                ["sadness"] = 0.5 * weight,
                ["calm"] = 0.3 * weight,
                ["anxiety"] = 0.4 * weight
            },
            _ => new Dictionary<string, double>()
        };
    }

    private void AdjustScoresWithAdvancedShapes(Dictionary<string, double> scores, AdvancedShapeAnalysis shapes)
    {
        try
        {
            // Analyse basée sur les symboles détectés
            foreach (var symbol in shapes.Symbols)
            {
                switch (symbol.Type)
                {
                    case "human":
                    case "face":
                        scores["love"] += 0.3;
                        scores["calm"] += 0.2;
                        break;

                    case "isolation":
                        scores["sadness"] += 0.4;
                        scores["anxiety"] += 0.3;
                        scores["fear"] += 0.2;
                        break;

                    case "emotional_expression":
                        scores["energy"] += 0.3;
                        // Renforce l'émotion dominante
                        var dominant = scores.OrderByDescending(e => e.Value).First().Key;
                        scores[dominant] += 0.2;
                        break;

                    case "past_focus":
                        scores["sadness"] += 0.3;
                        scores["calm"] += 0.2;
                        break;

                    case "house":
                        scores["calm"] += 0.3;
                        scores["love"] += 0.2;
                        break;

                    case "tree":
                        scores["calm"] += 0.2;
                        scores["joy"] += 0.1;
                        break;
                }
            }

            // Analyse des formes organiques vs géométriques
            var organic = shapes.Shapes.Sum(s => s.Type == "organic" || s.Type == "blob" ? s.Count : 0);
            var geometric = shapes.Shapes.Sum(s => s.Type == "square" || s.Type == "rectangle" || s.Type == "triangle" ? s.Count : 0);

            if (organic > geometric)
            {
                scores["energy"] += 0.2;
                scores["love"] += 0.1;
            }
            else
            {
                scores["calm"] += 0.2;
            }

            // Analyse des formes spécifiques
            var circles = shapes.Shapes.Sum(s => s.Type == "circle" ? s.Count : 0);
            var triangles = shapes.Shapes.Sum(s => s.Type == "triangle" ? s.Count : 0);

            if (circles > 0) scores["calm"] += 0.1 * circles;
            if (triangles > 0) scores["energy"] += 0.1 * triangles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajustement des scores avec les formes");
        }
    }

    private void AdjustScoresWithComposition(Dictionary<string, double> scores, CompositionAnalysis composition)
    {
        try
        {
            // Analyse de la composition spatiale
            switch (composition.SpaceUsage)
            {
                case "constricted":
                    scores["anxiety"] += 0.4;
                    scores["fear"] += 0.2;
                    scores["sadness"] += 0.1;
                    break;
                case "expansive":
                    scores["joy"] += 0.3;
                    scores["energy"] += 0.2;
                    scores["calm"] += 0.1;
                    break;
                case "crowded":
                    scores["anxiety"] += 0.3;
                    scores["energy"] += 0.2;
                    break;
            }

            // Analyse de la pression
            switch (composition.Pressure)
            {
                case "heavy":
                    scores["anger"] += 0.3;
                    scores["energy"] += 0.2;
                    scores["anxiety"] += 0.1;
                    break;
                case "light":
                    scores["calm"] += 0.3;
                    scores["sadness"] += 0.1;
                    break;
            }

            // Analyse de l'équilibre
            switch (composition.Balance)
            {
                case "left-heavy":
                    scores["sadness"] += 0.2;
                    scores["calm"] += 0.1;
                    break;
                case "right-heavy":
                    scores["energy"] += 0.2;
                    scores["joy"] += 0.1;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajustement des scores avec la composition");
        }
    }

    private string AssessEmotionalState(Dictionary<string, double> emotionScores)
    {
        try
        {
            var highScores = emotionScores.Count(e => e.Value > 0.6);
            var conflictingPairs = new[]
            {
                ("joy", "sadness"),
                ("anger", "calm"),
                ("energy", "fear")
            };

            // Vérifier les émotions conflictuelles
            foreach (var (emotion1, emotion2) in conflictingPairs)
            {
                if (emotionScores.GetValueOrDefault(emotion1) > 0.4 &&
                    emotionScores.GetValueOrDefault(emotion2) > 0.4)
                {
                    return "conflicted";
                }
            }

            if (highScores >= 3) return "volatile";
            if (highScores == 0) return "neutral";
            if (highScores == 1 && emotionScores.Values.Max() > 0.8) return "intense";

            return "stable";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'évaluation de l'état émotionnel");
            return "unknown";
        }
    }

    private List<EmotionalIndicator> GenerateEmotionalIndicators(AdvancedEmotionalAnalysis analysis, ColorAnalysis colors, AdvancedShapeAnalysis shapes)
    {
        var indicators = new List<EmotionalIndicator>();

        try
        {
            // Indicateur de couleur dominante
            if (!string.IsNullOrEmpty(colors.DominantColor))
            {
                indicators.Add(new EmotionalIndicator
                {
                    Type = "color",
                    Description = $"Couleur dominante: {colors.DominantColor}",
                    Confidence = 0.8,
                    EmotionalCorrelation = GetColorEmotionalCorrelation(colors.DominantColor)
                });
            }

            // Indicateur de forme
            if (shapes.Shapes.Any(s => s.Type == "circle"))
            {
                indicators.Add(new EmotionalIndicator
                {
                    Type = "shape",
                    Description = "Présence de formes rondes",
                    Confidence = 0.7,
                    EmotionalCorrelation = "Harmonie et douceur émotionnelle"
                });
            }

            // Indicateur de symboles émotionnels
            if (shapes.Symbols.Any(s => s.Type == "isolation"))
            {
                indicators.Add(new EmotionalIndicator
                {
                    Type = "symbol",
                    Description = "Éléments de repli détectés",
                    Confidence = 0.6,
                    EmotionalCorrelation = "Tendance à l'introspection ou besoin de protection"
                });
            }

            // Indicateur d'émotion dominante
            indicators.Add(new EmotionalIndicator
            {
                Type = "emotional",
                Description = $"Émotion dominante: {analysis.DominantEmotion}",
                Confidence = analysis.EmotionScores[analysis.DominantEmotion],
                EmotionalCorrelation = "État émotionnel principal détecté"
            });

            // Indicateur d'état émotionnel global
            indicators.Add(new EmotionalIndicator
            {
                Type = "state",
                Description = $"État global: {analysis.EmotionalState}",
                Confidence = 0.7,
                EmotionalCorrelation = GetStateCorrelation(analysis.EmotionalState)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération des indicateurs émotionnels");
        }

        return indicators;
    }

    private string GetColorEmotionalCorrelation(string color)
    {
        return color.ToLower() switch
        {
            "red" => "Énergie intense, passion ou colère",
            "blue" => "Calme, sérénité ou tristesse",
            "yellow" => "Joie, optimisme et énergie positive",
            "black" => "Anxiété, tristesse ou besoin de structure",
            "green" => "Équilibre, croissance et harmonie",
            "white" => "Pureté, simplicité ou vide émotionnel",
            "orange" => "Créativité, enthousiasme et sociabilité",
            "purple" => "Imagination, spiritualité et sensibilité",
            "pink" => "Tendresse, affection et douceur",
            "brown" => "Stabilité, sécurité et pragmatisme",
            "gray" => "Neutralité, indécision ou maturité",
            _ => "Signification émotionnelle contextuelle"
        };
    }

    private string GetStateCorrelation(string state)
    {
        return state.ToLower() switch
        {
            "conflicted" => "Émotions contradictoires ou tension interne",
            "volatile" => "Émotions changeantes ou instables",
            "intense" => "Émotion forte et concentrée",
            "stable" => "Équilibre émotionnel",
            "neutral" => "Émotions modérées ou contenues",
            _ => "État émotionnel particulier"
        };
    }

    public async Task<RiskAssessment> AssessRisksAsync(AdvancedEmotionalAnalysis emotions)
    {
        return await Task.Run(() =>
        {
            var assessment = new RiskAssessment();

            try
            {
                var riskFactors = new List<string>();
                var recommendations = new List<string>();

                // Facteurs de risque basés sur les scores émotionnels
                if (emotions.EmotionScores.GetValueOrDefault("sadness", 0) > 0.7)
                {
                    riskFactors.Add("Tristesse élevée détectée");
                    recommendations.Add("Observer les comportements récents et l'humeur");
                }

                if (emotions.EmotionScores.GetValueOrDefault("anger", 0) > 0.7)
                {
                    riskFactors.Add("Colère intense détectée");
                    recommendations.Add("Encourager l'expression verbale des émotions");
                }

                if (emotions.EmotionScores.GetValueOrDefault("anxiety", 0) > 0.6)
                {
                    riskFactors.Add("Anxiété notable");
                    recommendations.Add("Activités relaxantes et sécurisantes recommandées");
                }

                if (emotions.EmotionScores.GetValueOrDefault("fear", 0) > 0.5)
                {
                    riskFactors.Add("Peur détectée");
                    recommendations.Add("Rassurer et créer un environnement sécurisant");
                }

                // Facteurs basés sur l'état émotionnel
                if (emotions.EmotionalState == "conflicted")
                {
                    riskFactors.Add("Conflit émotionnel interne");
                    recommendations.Add("Aider à verbaliser les émotions contradictoires");
                }

                if (emotions.EmotionalState == "volatile")
                {
                    riskFactors.Add("Instabilité émotionnelle");
                    recommendations.Add("Établir des routines sécurisantes");
                }

                assessment.RiskFactors = riskFactors;
                assessment.Recommendations = recommendations;

                // Déterminer le niveau de risque
                if (riskFactors.Count >= 3)
                {
                    assessment.Level = "high";
                    assessment.RequiresAttention = true;
                    recommendations.Add("Consultation professionnelle recommandée");
                }
                else if (riskFactors.Count >= 1)
                {
                    assessment.Level = "medium";
                    assessment.RequiresAttention = true;
                    recommendations.Add("Surveillance attentive recommandée");
                }
                else
                {
                    assessment.Level = "low";
                    assessment.RequiresAttention = false;
                    recommendations.Add("Continuer à observer le développement normal");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'évaluation des risques");
                assessment.Level = "unknown";
                assessment.RequiresAttention = false;
                assessment.Recommendations = new List<string> { "Évaluation à reprendre" };
            }

            return assessment;
        });
    }
}