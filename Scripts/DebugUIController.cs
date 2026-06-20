using UnityEngine;
using UnityEngine.UI;

public class DebugUIController : MonoBehaviour
{
    public Text cropTypeText;
    public Text layoutModeText;
    public Text spacingText;
    public Text spacingXText;
    public Text spacingZText;
    public Text strategyText;
    public Text selectedStrategyText;
    public Text hubChoiceText;
    public Text treeCountText;
    public Text servedTreesText;
    public Text unservedTreesText;
    public Text pipeLengthText;
    public Text pipeCountText;
    public Text mainPipeCountText;
    public Text branchPipeCountText;
    public Text lateralPipeCountText;
    public Text forbiddenPenaltyText;
    public Text scoreText;
    public Text normalizedScoreText;
    public Text rerouteUsedText;

    void Awake()
    {
        // Auto-assign UI elements if not set
        if (cropTypeText == null) cropTypeText = GameObject.Find("CropTypeText")?.GetComponent<Text>();
        if (layoutModeText == null) layoutModeText = GameObject.Find("LayoutModeText")?.GetComponent<Text>();
        if (spacingText == null) spacingText = GameObject.Find("SpacingText")?.GetComponent<Text>();
        if (spacingXText == null) spacingXText = GameObject.Find("SpacingXText")?.GetComponent<Text>();
        if (spacingZText == null) spacingZText = GameObject.Find("SpacingZText")?.GetComponent<Text>();
        if (strategyText == null) strategyText = GameObject.Find("StrategyText")?.GetComponent<Text>();
        if (selectedStrategyText == null) selectedStrategyText = GameObject.Find("SelectedStrategyText")?.GetComponent<Text>();
        if (hubChoiceText == null) hubChoiceText = GameObject.Find("HubChoiceText")?.GetComponent<Text>();
        if (treeCountText == null) treeCountText = GameObject.Find("TreeCountText")?.GetComponent<Text>();
        if (servedTreesText == null) servedTreesText = GameObject.Find("ServedTreesText")?.GetComponent<Text>();
        if (unservedTreesText == null) unservedTreesText = GameObject.Find("UnservedTreesText")?.GetComponent<Text>();
        if (pipeLengthText == null) pipeLengthText = GameObject.Find("PipeLengthText")?.GetComponent<Text>();
        if (pipeCountText == null) pipeCountText = GameObject.Find("PipeCountText")?.GetComponent<Text>();
        if (mainPipeCountText == null) mainPipeCountText = GameObject.Find("MainPipeCountText")?.GetComponent<Text>();
        if (branchPipeCountText == null) branchPipeCountText = GameObject.Find("BranchPipeCountText")?.GetComponent<Text>();
        if (lateralPipeCountText == null) lateralPipeCountText = GameObject.Find("LateralPipeCountText")?.GetComponent<Text>();
        if (forbiddenPenaltyText == null) forbiddenPenaltyText = GameObject.Find("ForbiddenPenaltyText")?.GetComponent<Text>();
        if (scoreText == null) scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
        if (normalizedScoreText == null) normalizedScoreText = GameObject.Find("NormalizedScoreText")?.GetComponent<Text>();
        if (rerouteUsedText == null) rerouteUsedText = GameObject.Find("RerouteUsedText")?.GetComponent<Text>();
    }

    public void SetDebug(AIFarmDebug debug)
    {
        if (debug == null) return;

        if (cropTypeText != null) cropTypeText.text = $"Crop Type: {debug.crop_type}";
        if (layoutModeText != null) layoutModeText.text = $"Layout Mode: {debug.layout_mode}";
        if (spacingText != null) spacingText.text = $"Spacing: {debug.spacing:F2}";
        if (spacingXText != null) spacingXText.text = $"Spacing X: {debug.spacing_x:F2}";
        if (spacingZText != null) spacingZText.text = $"Spacing Z: {debug.spacing_z:F2}";
        if (strategyText != null) strategyText.text = $"Strategy: {debug.strategy}";
        if (selectedStrategyText != null) selectedStrategyText.text = $"Selected Strategy: {debug.selected_strategy}";
        if (hubChoiceText != null) hubChoiceText.text = $"Hub: {debug.hub_choice}";
        if (treeCountText != null) treeCountText.text = $"Trees: {debug.tree_count}";
        if (servedTreesText != null) servedTreesText.text = $"Served Trees: {debug.served_trees}";
        if (unservedTreesText != null) unservedTreesText.text = $"Unserved Trees: {debug.unserved_trees}";
        if (pipeLengthText != null) pipeLengthText.text = $"Pipe Length: {debug.total_pipe_length:F2}";
        if (pipeCountText != null) pipeCountText.text = $"Pipes: {debug.pipe_count}";
        if (mainPipeCountText != null) mainPipeCountText.text = $"Main Pipes: {debug.main_pipe_count}";
        if (branchPipeCountText != null) branchPipeCountText.text = $"Branch Pipes: {debug.branch_pipe_count}";
        if (lateralPipeCountText != null) lateralPipeCountText.text = $"Lateral Pipes: {debug.lateral_pipe_count}";
        if (forbiddenPenaltyText != null) forbiddenPenaltyText.text = $"Forbidden Penalty: {debug.forbidden_penalty}";
        if (scoreText != null) scoreText.text = $"Score: {debug.score:F2}";
        if (normalizedScoreText != null) normalizedScoreText.text = $"Normalized Score: {debug.normalized_score:F2}";
        if (rerouteUsedText != null) rerouteUsedText.text = $"Reroute Used: {debug.reroute_used}";
    }
}
