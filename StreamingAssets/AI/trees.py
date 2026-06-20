"""Tree placement and irrigation coverage checks."""

from geometry import point_in_forbidden, point_in_well_safe_zone


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

            if point_in_forbidden(pos_x, pos_z, forbidden_zones) or point_in_well_safe_zone(
                pos_x, pos_z, well_x, well_z, well_safe_radius
            ):
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
        "spacing_z": spacing,
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

            if point_in_forbidden(pos_x, pos_z, forbidden_zones) or point_in_well_safe_zone(
                pos_x, pos_z, well_x, well_z, well_safe_radius
            ):
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
        "spacing_z": spacing_z,
    }


def generate_trees(
    width,
    height,
    border,
    spacing,
    well_x,
    well_z,
    well_safe_radius=0.0,
    forbidden_zones=None,
    layout_mode="centered",
    base_spacing=5.0,
):
    if layout_mode == "centered":
        return generate_trees_centered(width, height, border, spacing, well_x, well_z, well_safe_radius, forbidden_zones)
    if layout_mode == "adaptive_fill":
        return generate_trees_adaptive_fill(
            width, height, border, base_spacing, well_x, well_z, well_safe_radius, forbidden_zones
        )
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
