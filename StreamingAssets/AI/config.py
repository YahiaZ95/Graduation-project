"""Crop types and spacing settings used by the layout planner."""

CROP_CONFIG = {
    0: {
        "name": "Olive",
        "spacing": 6,
        "border": 2.8,
    },
    1: {
        "name": "Mango",
        "spacing": 10,
        "border": 5,
    },
    2: {
        "name": "Orange",
        "spacing": 5,
        "border": 2.5,
    },
    3: {
        "name": "Lemon",
        "spacing": 5,
        "border": 2.5,
    },
    4: {
        "name": "Guava",
        "spacing": 5,
        "border": 2.5,
    },
    5: {
        "name": "Pomegranate",
        "spacing": 4,
        "border": 2,
    },
    6: {
        "name": "Date Palm",
        "spacing": 9,
        "border": 4.5,
    },
    7: {
        "name": "Apple",
        "spacing": 4,
        "border": 2,
    },
    8: {
        "name": "Peach",
        "spacing": 5,
        "border": 2.5,
    },
    9: {
        "name": "Apricot",
        "spacing": 5,
        "border": 2.5,
    },
    10: {
        "name": "Pear",
        "spacing": 5,
        "border": 2.5,
    },
    11: {
        "name": "Fig",
        "spacing": 6,
        "border": 3,
    },
    12: {
        "name": "Mulberry",
        "spacing": 8,
        "border": 4,
    },
    13: {
        "name": "Avocado",
        "spacing": 8,
        "border": 4,
    },
    14: {
        "name": "Cherry",
        "spacing": 5,
        "border": 2.5,
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
