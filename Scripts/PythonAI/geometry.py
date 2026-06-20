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
