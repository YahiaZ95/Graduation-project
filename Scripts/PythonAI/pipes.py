from .geometry import segment_intersects_forbidden


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
