"""Crop types and spacing settings used by the layout planner."""

CROP_CONFIG = {
    0: {
        "name": "Trees",
        "spacing": 5,
        "border": 2.5,
    },
    1: {
        "name": "Palm",
        "spacing": 8,
        "border": 3.0,
    },
    2: {
        "name": "Olive",
        "spacing": 6,
        "border": 2.8,
    },
}


def get_crop_config(crop_type):
    """Resolve crop settings from an int id or string name."""
    if isinstance(crop_type, str):
        candidate = crop_type.strip().lower()
        for key, cfg in CROP_CONFIG.items():
            if str(key).lower() == candidate or cfg["name"].lower() == candidate:
                return cfg

    try:
        return CROP_CONFIG[int(crop_type)]
    except (TypeError, ValueError, KeyError):
        return CROP_CONFIG.get(crop_type, CROP_CONFIG[0])
