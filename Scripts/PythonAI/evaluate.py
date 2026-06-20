from .config import CROP_CONFIG
from .trees import generate_trees, is_tree_served
from .pipes import generate_pipes, calculate_total_pipe_length


def evaluate_layout_with_mode(width, height, border, base_spacing, well_x, well_z, forbidden_zones=None, well_safe_radius=0.0, strategy="main_vertical", layout_mode="centered"):
    tree_result = generate_trees(width, height, border, base_spacing, well_x, well_z, well_safe_radius=well_safe_radius, forbidden_zones=forbidden_zones, layout_mode=layout_mode)
    trees = tree_result["trees"]
    tree_count = tree_result["tree_count"]
    invalid_tree_count = tree_result["invalid_tree_count"]
    layout_mode_result = tree_result["layout_mode"]
    spacing_x = tree_result["spacing_x"]
    spacing_z = tree_result["spacing_z"]

    candidates = generate_pipes(width, height, border, base_spacing, well_x, well_z, trees, strategy=strategy, forbidden_zones=forbidden_zones)

    rows = {}
    for t in trees:
        rz = round(t["z"], 2)
        rows.setdefault(rz, []).append(t)
    cols = {}
    for t in trees:
        cx = round(t["x"], 2)
        cols.setdefault(cx, []).append(t)
    main_pipe_count = 1
    if strategy == "main_vertical":
        branch_pipe_count = max(0, len(rows) - 1)
        lateral_pipe_count = max(0, len(rows) - 1)
    else:
        branch_pipe_count = max(0, len(cols) - 1)
        lateral_pipe_count = max(0, len(cols) - 1)

    best = None
    for cand in candidates:
        pipes = cand["pipes"]
        invalid_pipe_count = cand["invalid_pipe_count"]
        forbidden_hits = cand["forbidden_hits"]
        hub_choice = cand["hub_choice"]
        reroute = cand["reroute"]

        total_pipe_length = calculate_total_pipe_length(pipes)

        unserved_count = 0
        for t in trees:
            if not is_tree_served(t, pipes, spacing_x=spacing_x, spacing_z=spacing_z, margin=1.0):
                unserved_count += 1

        served_count = tree_count - unserved_count

        score = tree_count - 0.2 * total_pipe_length
        score -= 5 * invalid_tree_count
        score -= 10 * invalid_pipe_count
        score -= 50 * unserved_count
        score -= 100 * forbidden_hits
        if tree_count < 10:
            score -= (10 - tree_count) * 2

        normalized_score = score + 200
        spacing_display = (spacing_x + spacing_z) / 2.0 if spacing_x > 0 and spacing_z > 0 else base_spacing

        candidate_result = {
            "score": score,
            "normalized_score": normalized_score,
            "trees": trees,
            "pipes": pipes,
            "tree_count": tree_count,
            "served_trees": served_count,
            "unserved_trees": unserved_count,
            "pipe_length": total_pipe_length,
            "pipe_count": len(pipes),
            "invalid_trees": invalid_tree_count,
            "invalid_pipes": invalid_pipe_count,
            "forbidden_penalty": forbidden_hits,
            "strategy": strategy,
            "selected_strategy": strategy,
            "hub_choice": f"{hub_choice[0]},{hub_choice[1]}" if hub_choice else None,
            "reroute_used": reroute,
            "layout_mode": layout_mode_result,
            "spacing": spacing_display,
            "spacing_x": spacing_x,
            "spacing_z": spacing_z,
            "main_pipe_count": main_pipe_count,
            "branch_pipe_count": branch_pipe_count,
            "lateral_pipe_count": lateral_pipe_count
        }

        if best is None:
            best = candidate_result
            continue

        if candidate_result["unserved_trees"] < best["unserved_trees"]:
            best = candidate_result
            continue
        if candidate_result["unserved_trees"] > best["unserved_trees"]:
            continue

        if candidate_result["forbidden_penalty"] < best["forbidden_penalty"]:
            best = candidate_result
            continue
        if candidate_result["forbidden_penalty"] > best["forbidden_penalty"]:
            continue

        if candidate_result["pipe_length"] < best["pipe_length"]:
            best = candidate_result
            continue
        if candidate_result["pipe_length"] > best["pipe_length"]:
            continue

        if candidate_result["score"] > best["score"]:
            best = candidate_result

    if best is None:
        return {
            "score": 0,
            "normalized_score": 200,
            "trees": [],
            "pipes": [],
            "tree_count": 0,
            "served_trees": 0,
            "unserved_trees": 0,
            "pipe_length": 0,
            "pipe_count": 0,
            "invalid_trees": 0,
            "invalid_pipes": 0,
            "forbidden_penalty": 0,
            "strategy": strategy,
            "selected_strategy": strategy,
            "hub_choice": "",
            "reroute_used": False,
            "layout_mode": layout_mode,
            "spacing": base_spacing,
            "spacing_x": base_spacing,
            "spacing_z": base_spacing,
            "main_pipe_count": 0,
            "branch_pipe_count": 0,
            "lateral_pipe_count": 0
        }

    return best


def find_best_layout(width, height, crop_type, border, well_x, well_z, forbidden_zones=None, well_safe_radius=0.0):
    base_spacing = CROP_CONFIG.get(crop_type, CROP_CONFIG[0])["spacing"]

    layout_modes = ["centered", "adaptive_fill"]
    strategy_options = ["main_vertical", "main_horizontal"]

    best = None
    for layout_mode in layout_modes:
        for st in strategy_options:
            candidate = evaluate_layout_with_mode(width, height, border, base_spacing, well_x, well_z, forbidden_zones=forbidden_zones, well_safe_radius=well_safe_radius, strategy=st, layout_mode=layout_mode)

            if best is None:
                best = candidate
                continue

            if candidate["unserved_trees"] < best["unserved_trees"]:
                best = candidate
                continue
            if candidate["unserved_trees"] > best["unserved_trees"]:
                continue

            if candidate["forbidden_penalty"] < best["forbidden_penalty"]:
                best = candidate
                continue
            if candidate["forbidden_penalty"] > best["forbidden_penalty"]:
                continue

            if candidate["tree_count"] > best["tree_count"]:
                best = candidate
                continue
            if candidate["tree_count"] < best["tree_count"]:
                continue

            if candidate["pipe_length"] < best["pipe_length"]:
                best = candidate
                continue
            if candidate["pipe_length"] > best["pipe_length"]:
                continue

            if candidate["score"] > best["score"]:
                best = candidate

    return best


def process_request(json_data):
    width = json_data["width"]
    height = json_data["height"]
    crop_type = json_data["crop_type"]
    well_x = json_data["well_position"]["x"]
    well_z = json_data["well_position"]["z"]
    well_safe_radius = json_data.get("well_safe_radius", 0.0)

    border = CROP_CONFIG.get(crop_type, CROP_CONFIG[0])["border"]
    forbidden_zones = json_data.get("forbidden_zones", [])

    best = find_best_layout(width, height, crop_type, border, well_x, well_z, forbidden_zones=forbidden_zones, well_safe_radius=well_safe_radius)

    return {
        "trees": best["trees"],
        "pipes": best["pipes"],
        "debug": {
            "crop_type": CROP_CONFIG.get(crop_type, CROP_CONFIG[0])["name"],
            "layout_mode": best["layout_mode"],
            "spacing": best["spacing"],
            "spacing_x": best["spacing_x"],
            "spacing_z": best["spacing_z"],
            "tree_count": len(best["trees"]),
            "unserved_trees": best["unserved_trees"],
            "total_pipe_length": best["pipe_length"],
            "score": best["score"],
            "normalized_score": best["normalized_score"],
            "unserved_penalty": best["unserved_trees"],
            "forbidden_penalty": best["forbidden_penalty"],
            "hub_choice": best["hub_choice"],
            "well_safe_radius": well_safe_radius,
            "well_position": {"x": well_x, "z": well_z},
            "served_trees": best["served_trees"],
            "pipe_count": best["pipe_count"],
            "main_pipe_count": best["main_pipe_count"],
            "branch_pipe_count": best["branch_pipe_count"],
            "lateral_pipe_count": best["lateral_pipe_count"],
            "strategy": best.get("strategy", "unknown"),
            "selected_strategy": best.get("selected_strategy", best.get("strategy", "unknown")),
            "reroute_used": best.get("reroute_used", False)
        }
    }
