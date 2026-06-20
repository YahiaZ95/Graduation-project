import socket
import json
import sys

CROP_CONFIG = {
    0: {
        "name": "Trees",
        "spacing": 5,
        "border": 2.5
    },
    1: {
        "name": "Palm",
        "spacing": 8,
        "border": 3.0
    },
    2: {
        "name": "Olive",
        "spacing": 6,
        "border": 2.8
    },
}


def get_crop_config(crop_type):
    if isinstance(crop_type, str):
        candidate = crop_type.strip().lower()
        for key, cfg in CROP_CONFIG.items():
            if str(key).lower() == candidate or cfg["name"].lower() == candidate:
                return cfg

    try:
        return CROP_CONFIG[int(crop_type)]
    except (TypeError, ValueError, KeyError):
        return CROP_CONFIG.get(crop_type, CROP_CONFIG[0])


def point_in_forbidden(x, z, forbidden_zones):
    for zone in forbidden_zones:
        dx = x - zone["x"]
        dz = z - zone["z"]
        if (dx * dx + dz * dz) <= (zone.get("radius", 0) ** 2):
            return True
    return False


def point_in_well_safe_zone(x, z, well_x, well_z, well_safe_radius):
    if well_safe_radius <= 0:
        return False
    dx = x - well_x
    dz = z - well_z
    return (dx * dx + dz * dz) <= (well_safe_radius ** 2)


def segment_intersects_forbidden(p, forbidden_zones):
    x1, z1 = p["start"]["x"], p["start"]["z"]
    x2, z2 = p["end"]["x"], p["end"]["z"]

    for zone in forbidden_zones:
        cx, cz = zone["x"], zone["z"]
        r = zone.get("radius", 0)

        dx = x2 - x1
        dz = z2 - z1

        if dx == 0 and dz == 0:
            dist = ((x1 - cx) ** 2 + (z1 - cz) ** 2) ** 0.5
            if dist <= r:
                return True
            continue

        t = ((cx - x1) * dx + (cz - z1) * dz) / (dx * dx + dz * dz)
        t = max(0.0, min(1.0, t))
        px = x1 + t * dx
        pz = z1 + t * dz

        dist = ((px - cx) ** 2 + (pz - cz) ** 2) ** 0.5
        if dist <= r:
            return True

    return False


def generate_trees_centered(width, height, border, spacing, well_x, well_z, well_safe_radius=0.0, forbidden_zones=None):
    trees = []
    invalid_tree_count = 0

    forbidden_zones = forbidden_zones or []

    usable_width = width - border * 2
    usable_height = height - border * 2

    count_x = max(1, int(usable_width // spacing) + 1)
    count_z = max(1, int(usable_height // spacing) + 1)

    actual_layout_width = (count_x - 1) * spacing
    actual_layout_height = (count_z - 1) * spacing

    start_x = -actual_layout_width / 2
    start_z = -actual_layout_height / 2

    for z in range(count_z):
        for x in range(count_x):
            pos_x = start_x + x * spacing
            pos_z = start_z + z * spacing

            if point_in_forbidden(pos_x, pos_z, forbidden_zones) or point_in_well_safe_zone(pos_x, pos_z, well_x, well_z, well_safe_radius):
                invalid_tree_count += 1
                continue

            if abs(pos_x) > width / 2 or abs(pos_z) > height / 2:
                invalid_tree_count += 1
                continue

            trees.append({"x": pos_x, "z": pos_z})

    return {
        "trees": trees,
        "tree_count": len(trees),
        "invalid_tree_count": invalid_tree_count,
        "layout_mode": "centered",
        "spacing_x": spacing,
        "spacing_z": spacing
    }


def generate_trees_adaptive_fill(width, height, border, base_spacing, well_x, well_z, well_safe_radius=0.0, forbidden_zones=None):
    trees = []
    invalid_tree_count = 0

    forbidden_zones = forbidden_zones or []

    usable_width = width - 2 * border
    usable_height = height - 2 * border
    start_x = -width / 2 + border
    start_z = -height / 2 + border

    count_x = max(2, int(usable_width // base_spacing) + 1)
    count_z = max(2, int(usable_height // base_spacing) + 1)

    spacing_x = usable_width / (count_x - 1) if count_x > 1 else 0
    spacing_z = usable_height / (count_z - 1) if count_z > 1 else 0

    for z in range(count_z):
        for x in range(count_x):
            pos_x = start_x + x * spacing_x
            pos_z = start_z + z * spacing_z

            if point_in_forbidden(pos_x, pos_z, forbidden_zones) or point_in_well_safe_zone(pos_x, pos_z, well_x, well_z, well_safe_radius):
                invalid_tree_count += 1
                continue

            if abs(pos_x) > width / 2 or abs(pos_z) > height / 2:
                invalid_tree_count += 1
                continue

            trees.append({"x": pos_x, "z": pos_z})

    return {
        "trees": trees,
        "tree_count": len(trees),
        "invalid_tree_count": invalid_tree_count,
        "layout_mode": "adaptive_fill",
        "spacing_x": spacing_x,
        "spacing_z": spacing_z
    }


def generate_trees(width, height, border, spacing, well_x, well_z, well_safe_radius=0.0, forbidden_zones=None, layout_mode="centered", base_spacing=5.0):
    if layout_mode == "centered":
        return generate_trees_centered(width, height, border, spacing, well_x, well_z, well_safe_radius, forbidden_zones)
    if layout_mode == "adaptive_fill":
        return generate_trees_adaptive_fill(width, height, border, base_spacing, well_x, well_z, well_safe_radius, forbidden_zones)
    return generate_trees_centered(width, height, border, spacing, well_x, well_z, well_safe_radius, forbidden_zones)


def is_tree_served(tree, pipes, spacing_x=5.0, spacing_z=5.0, margin=1.0):
    max_dist = min(spacing_x, spacing_z) / 2.0 + margin

    tx, tz = tree["x"], tree["z"]
    for p in pipes:
        x1, z1 = p["start"]["x"], p["start"]["z"]
        x2, z2 = p["end"]["x"], p["end"]["z"]

        dx = x2 - x1
        dz = z2 - z1
        if dx == 0 and dz == 0:
            dist = ((tx - x1) ** 2 + (tz - z1) ** 2) ** 0.5
            if dist <= max_dist:
                return True
            continue

        t = ((tx - x1) * dx + (tz - z1) * dz) / (dx * dx + dz * dz)
        t = max(0.0, min(1.0, t))
        px = x1 + t * dx
        pz = z1 + t * dz
        dist = ((tx - px) ** 2 + (tz - pz) ** 2) ** 0.5
        if dist <= max_dist:
            return True
    return False


def _build_pipe_segments_for_hub(width, height, border, spacing, well_x, well_z, trees, strategy, hub_x, hub_z, forbidden_zones):
    pipes = []
    invalid_pipe_count = 0
    forbidden_hits = 0

    def add_segment(seg):
        nonlocal forbidden_hits, invalid_pipe_count
        if seg["start"] == seg["end"]:
            invalid_pipe_count += 1
            return

        if segment_intersects_forbidden(seg, forbidden_zones):
            forbidden_hits += 1
        pipes.append(seg)

    l_options = []
    if well_x != hub_x:
        l_options.append([
            {"start": {"x": well_x, "z": well_z}, "end": {"x": hub_x, "z": well_z}},
            {"start": {"x": hub_x, "z": well_z}, "end": {"x": hub_x, "z": hub_z}}
        ])
    if well_z != hub_z:
        l_options.append([
            {"start": {"x": well_x, "z": well_z}, "end": {"x": well_x, "z": hub_z}},
            {"start": {"x": well_x, "z": hub_z}, "end": {"x": hub_x, "z": hub_z}}
        ])

    best_l = None
    best_l_hits = None
    for option in l_options:
        hits = sum(1 for seg in option if segment_intersects_forbidden(seg, forbidden_zones))
        if best_l is None or hits < best_l_hits:
            best_l = option
            best_l_hits = hits

    if best_l:
        for seg in best_l:
            add_segment(seg)

    min_x = min(t["x"] for t in trees)
    max_x = max(t["x"] for t in trees)
    min_z = min(t["z"] for t in trees)
    max_z = max(t["z"] for t in trees)

    if strategy == "main_vertical":
        trunk = {"start": {"x": hub_x, "z": min_z}, "end": {"x": hub_x, "z": max_z}}
        add_segment(trunk)

        rows = {}
        for t in trees:
            rz = round(t["z"], 2)
            rows.setdefault(rz, []).append(t)

        sorted_row_zs = sorted(rows.keys())
        for i in range(len(sorted_row_zs) - 1):
            z1 = sorted_row_zs[i]
            z2 = sorted_row_zs[i + 1]
            midpoint_z = (z1 + z2) / 2.0

            combined_trees = rows[z1] + rows[z2]
            row_min_x = min(t["x"] for t in combined_trees)
            row_max_x = max(t["x"] for t in combined_trees)

            add_segment({"start": {"x": hub_x, "z": midpoint_z}, "end": {"x": row_min_x, "z": midpoint_z}})
            add_segment({"start": {"x": row_min_x, "z": midpoint_z}, "end": {"x": row_max_x, "z": midpoint_z}})

        if len(sorted_row_zs) == 1:
            row_z = sorted_row_zs[0]
            row_trees = rows[row_z]
            row_min_x = min(t["x"] for t in row_trees)
            row_max_x = max(t["x"] for t in row_trees)
            add_segment({"start": {"x": hub_x, "z": row_z}, "end": {"x": row_min_x, "z": row_z}})
            add_segment({"start": {"x": row_min_x, "z": row_z}, "end": {"x": row_max_x, "z": row_z}})

    elif strategy == "main_horizontal":
        trunk = {"start": {"x": min_x, "z": hub_z}, "end": {"x": max_x, "z": hub_z}}
        add_segment(trunk)

        cols = {}
        for t in trees:
            cx = round(t["x"], 2)
            cols.setdefault(cx, []).append(t)

        sorted_col_xs = sorted(cols.keys())
        for i in range(len(sorted_col_xs) - 1):
            x1 = sorted_col_xs[i]
            x2 = sorted_col_xs[i + 1]
            midpoint_x = (x1 + x2) / 2.0

            combined_trees = cols[x1] + cols[x2]
            col_min_z = min(t["z"] for t in combined_trees)
            col_max_z = max(t["z"] for t in combined_trees)

            add_segment({"start": {"x": midpoint_x, "z": hub_z}, "end": {"x": midpoint_x, "z": col_min_z}})
            add_segment({"start": {"x": midpoint_x, "z": col_min_z}, "end": {"x": midpoint_x, "z": col_max_z}})

        if len(sorted_col_xs) == 1:
            col_x = sorted_col_xs[0]
            col_trees = cols[col_x]
            col_min_z = min(t["z"] for t in col_trees)
            col_max_z = max(t["z"] for t in col_trees)
            add_segment({"start": {"x": col_x, "z": hub_z}, "end": {"x": col_x, "z": col_min_z}})
            add_segment({"start": {"x": col_x, "z": col_min_z}, "end": {"x": col_x, "z": col_max_z}})

    return {
        "pipes": pipes,
        "invalid_pipe_count": invalid_pipe_count,
        "forbidden_hits": forbidden_hits,
        "hub_choice": (hub_x, hub_z),
        "reroute": best_l is not None and len(best_l) > 0
    }


def generate_pipes(width, height, border, spacing, well_x, well_z, trees, strategy="main_vertical", forbidden_zones=None):
    forbidden_zones = forbidden_zones or []

    if not trees:
        return []

    min_x = min(t["x"] for t in trees)
    max_x = max(t["x"] for t in trees)
    min_z = min(t["z"] for t in trees)
    max_z = max(t["z"] for t in trees)

    hub_candidates = [
        (min_x - 1, (min_z + max_z) / 2),
        (max_x + 1, (min_z + max_z) / 2),
        ((min_x + max_x) / 2, min_z - 1),
        ((min_x + max_x) / 2, max_z + 1)
    ]

    layouts = []
    for hub_x, hub_z in hub_candidates:
        layout = _build_pipe_segments_for_hub(width, height, border, spacing, well_x, well_z, trees, strategy, hub_x, hub_z, forbidden_zones)
        layouts.append(layout)

    return layouts


def calculate_total_pipe_length(pipes):
    total = 0.0
    for p in pipes:
        dx = p["end"]["x"] - p["start"]["x"]
        dz = p["end"]["z"] - p["start"]["z"]
        total += (dx * dx + dz * dz) ** 0.5
    return total


def evaluate_layout_with_mode(width, height, border, base_spacing, well_x, well_z, forbidden_zones=None, well_safe_radius=0.0, strategy="main_vertical", layout_mode="centered"):
    tree_result = generate_trees(
        width,
        height,
        border,
        base_spacing,
        well_x,
        well_z,
        well_safe_radius=well_safe_radius,
        forbidden_zones=forbidden_zones,
        layout_mode=layout_mode,
        base_spacing=base_spacing,
    )
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
    crop_cfg = get_crop_config(crop_type)
    base_spacing = crop_cfg["spacing"]

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

    crop_cfg = get_crop_config(crop_type)
    border = crop_cfg["border"]
    forbidden_zones = json_data.get("forbidden_zones", [])

    best = find_best_layout(width, height, crop_type, border, well_x, well_z, forbidden_zones=forbidden_zones, well_safe_radius=well_safe_radius)

    return {
        "trees": best["trees"],
        "pipes": best["pipes"],
        "debug": {
            "crop_type": crop_cfg["name"],
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


def start_server(host="127.0.0.1", port=5005):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind((host, port))
    server_socket.listen(1)
    print(f"AI server listening at {host}:{port}")

    while True:
        conn, addr = server_socket.accept()
        print(f"Connection received from {addr}")
        buffer = ""

        while True:
            data = conn.recv(4096)
            if not data:
                break

            buffer += data.decode("utf-8")
            while "\n" in buffer:
                line, buffer = buffer.split("\n", 1)
                if not line.strip():
                    continue

                try:
                    json_data = json.loads(line)
                except json.JSONDecodeError as e:
                    print("JSON decode error:", e)
                    continue

                print("Received:", json_data)
                response = process_request(json_data)
                conn.send((json.dumps(response) + "\n").encode())
                print("Sent AI layout.")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--server":
        host = "127.0.0.1"
        port = 5005
        if "--host" in sys.argv:
            host_index = sys.argv.index("--host")
            if host_index + 1 < len(sys.argv):
                host = sys.argv[host_index + 1]
        if "--port" in sys.argv:
            port_index = sys.argv.index("--port")
            if port_index + 1 < len(sys.argv):
                port = int(sys.argv[port_index + 1])
        start_server(host=host, port=port)
    elif len(sys.argv) > 1:
        json_str = sys.argv[1]
        json_data = json.loads(json_str)
        response = process_request(json_data)
        print(json.dumps(response))
    else:
        try:
            json_str = sys.stdin.read().strip()
            if json_str:
                json_data = json.loads(json_str)
                response = process_request(json_data)
                print(json.dumps(response))
            else:
                start_server()
        except:
            start_server()



