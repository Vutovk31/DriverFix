from pathlib import Path
import re

root = Path(__file__).resolve().parents[1]
parser_path = root / "DriverFix.Windows/PnpUtilInventoryParser.cs"
parser = parser_path.read_text(encoding="utf-8")
fixtures = root / "verification/fixtures"

checks = {
    "problem_code_prefix_parser": 'Regex.Match(value, @"^\\s*(\\d+)")' in parser,
    "english_aliases": all(x in parser for x in ["Instance ID", "Hardware IDs", "Compatible IDs", "Problem Code"]),
    "russian_aliases": all(x in parser for x in ["Идентификатор экземпляра", "ИД оборудования", "Совместимые ИД", "Код проблемы"]),
    "known_locale_split_preserved": "SplitByKnownInstanceKeys" in parser,
    "unknown_locale_paragraph_fallback": "SplitByParagraphs" in parser,
    "unknown_locale_instance_inference": "InferInstanceId" in parser and "LooksLikeDeviceInstanceId" in parser,
    "continuation_lines_preserved": "char.IsWhiteSpace(raw[0])" in parser,
    "empty_output_safe": "Array.Empty<DeviceInventoryItem>()" in parser,
    "no_driver_mutation": all(x not in parser for x in ["/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/scan-devices"]),
}

INSTANCE_KEYS = ["Instance ID", "Идентификатор экземпляра"]
ALIASES = {
    "instance": ["Instance ID", "Идентификатор экземпляра"],
    "description": ["Device Description", "Описание устройства"],
    "manufacturer": ["Manufacturer Name", "Имя изготовителя", "Производитель"],
    "problem": ["Problem Code", "Код проблемы"],
    "hardware": ["Hardware IDs", "ИД оборудования"],
    "compatible": ["Compatible IDs", "Совместимые ИД"],
}

def split_known(text):
    blocks, current, seen = [], [], False
    for raw in text.replace("\r\n", "\n").split("\n"):
        trimmed = raw.strip()
        is_instance = any(trimmed.lower().startswith((key + ":").lower()) for key in INSTANCE_KEYS)
        if is_instance and seen and current:
            blocks.append(current)
            current = []
        if is_instance:
            seen = True
        if seen:
            current.append(raw)
    if current:
        blocks.append(current)
    return blocks

def split_paragraphs(text):
    blocks, current = [], []
    for raw in text.replace("\r\n", "\n").split("\n"):
        if not raw.strip():
            if current:
                blocks.append(current)
                current = []
            continue
        current.append(raw)
    if current:
        blocks.append(current)
    return blocks

def split_blocks(text):
    known = split_known(text)
    return known if known else split_paragraphs(text)

def parse_fields(lines):
    result, current = {}, None
    for raw in lines:
        if not raw.strip():
            continue
        colon = raw.find(":")
        if colon > 0:
            current = raw[:colon].strip()
            value = raw[colon + 1:].strip()
            result.setdefault(current.lower(), [])
            if value:
                result[current.lower()].append(value)
            continue
        if current is not None and raw[:1].isspace():
            result[current.lower()].append(raw.strip())
    return result

def vals(fields, names):
    for name in names:
        if name.lower() in fields:
            return fields[name.lower()]
    return []

def first(fields, names):
    values = vals(fields, names)
    return values[0] if values else None

def infer_instance(lines):
    for raw in lines:
        colon = raw.find(":")
        if colon <= 0:
            continue
        value = raw[colon + 1:].strip()
        if " " in value or "\\" not in value:
            continue
        prefix, rest = value.split("\\", 1)
        if prefix and rest and all(ch.isalnum() or ch in "_-" for ch in prefix):
            return value
    return None

def parse_problem(value):
    if not value:
        return None
    match = re.match(r"^\s*(\d+)", value)
    return int(match.group(1)) if match else None

def parse(text):
    devices = []
    for block in split_blocks(text):
        fields = parse_fields(block)
        instance = first(fields, ALIASES["instance"]) or infer_instance(block)
        if not instance:
            continue
        devices.append({
            "instance": instance,
            "description": first(fields, ALIASES["description"]),
            "manufacturer": first(fields, ALIASES["manufacturer"]),
            "problem": parse_problem(first(fields, ALIASES["problem"])),
            "hardware": vals(fields, ALIASES["hardware"]),
            "compatible": vals(fields, ALIASES["compatible"]),
        })
    return devices

en = parse((fixtures / "pnputil-en-problem.txt").read_text(encoding="utf-8"))
checks["fixture_en_single_device"] = len(en) == 1
checks["fixture_en_decorated_problem_52"] = en[0]["problem"] == 52
checks["fixture_en_id_continuations"] = len(en[0]["hardware"]) == 2 and len(en[0]["compatible"]) == 1

ru = parse((fixtures / "pnputil-ru-synthetic.txt").read_text(encoding="utf-8"))
checks["fixture_ru_aliases"] = len(ru) == 1 and ru[0]["manufacturer"] == "Intel" and ru[0]["problem"] == 0
checks["fixture_ru_id_continuations"] = len(ru[0]["hardware"]) == 2 and len(ru[0]["compatible"]) == 1

two = parse((fixtures / "pnputil-two-devices.txt").read_text(encoding="utf-8"))
checks["fixture_two_devices_without_blank_separator"] = len(two) == 2
checks["fixture_second_problem_28"] = two[1]["problem"] == 28

unknown = parse((fixtures / "pnputil-localized-unknown.txt").read_text(encoding="utf-8"))
checks["fixture_unknown_locale_devices_recovered"] = len(unknown) == 2
checks["fixture_unknown_locale_instance_ids"] = (
    unknown[0]["instance"].startswith("PCI\\") and unknown[1]["instance"].startswith("USB\\")
)
checks["fixture_unknown_locale_does_not_invent_fields"] = unknown[0]["problem"] is None and unknown[0]["hardware"] == []

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)
if failed:
    raise SystemExit("DFX-002 FAIL: " + ", ".join(failed))
print(f"\nDFX-002 CONTRACT/FIXTURE PASS: {len(checks)}/{len(checks)}")
