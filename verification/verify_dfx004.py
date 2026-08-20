from pathlib import Path
import re

root = Path(__file__).resolve().parents[1]
fixtures = root / "verification" / "fixtures"

INSTANCE_KEYS = ["Instance ID", "Идентификатор экземпляра"]
ALIASES = {
    "instance": ["Instance ID", "Идентификатор экземпляра"],
    "description": ["Device Description", "Описание устройства"],
    "class": ["Class Name", "Имя класса"],
    "manufacturer": ["Manufacturer Name", "Имя изготовителя", "Производитель"],
    "status": ["Status", "Состояние"],
    "problem": ["Problem Code", "Код проблемы"],
    "hardware": ["Hardware IDs", "ИД оборудования"],
    "compatible": ["Compatible IDs", "Совместимые ИД"],
}

def split_blocks(text):
    blocks, current, seen = [], [], False
    for raw in text.replace("\r\n", "\n").split("\n"):
        trimmed = raw.strip()
        is_instance = any(
            trimmed.lower().startswith((key + ":").lower())
            for key in INSTANCE_KEYS
        )
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

def parse_problem(value):
    if not value:
        return None
    match = re.match(r"^\s*(\d+)", value)
    return int(match.group(1)) if match else None

def parse(text):
    devices = []
    for block in split_blocks(text):
        fields = parse_fields(block)
        instance = first(fields, ALIASES["instance"])
        if not instance:
            continue
        devices.append({
            "instance": instance,
            "description": first(fields, ALIASES["description"]),
            "class": first(fields, ALIASES["class"]),
            "manufacturer": first(fields, ALIASES["manufacturer"]),
            "status": first(fields, ALIASES["status"]),
            "problem": parse_problem(first(fields, ALIASES["problem"])),
            "hardware": vals(fields, ALIASES["hardware"]),
            "compatible": vals(fields, ALIASES["compatible"]),
        })
    return devices

def value(v):
    return "unknown" if v is None or not v.strip() else v.strip()

def format_devices(devices):
    lines = [f"Connected devices: {len(devices)}"]
    for index, device in enumerate(devices, 1):
        lines += [
            "",
            f"[{index}] {value(device['description'])}",
            f"Instance ID: {device['instance']}",
            f"Class: {value(device['class'])}",
            f"Manufacturer: {value(device['manufacturer'])}",
            f"Status: {value(device['status'])}",
            f"Problem Code: {device['problem'] if device['problem'] is not None else 'none'}",
            "Hardware IDs:",
        ]
        lines += [f"  - {x}" for x in device["hardware"]] or ["  - none"]
        lines += ["Compatible IDs:"]
        lines += [f"  - {x}" for x in device["compatible"]] or ["  - none"]
    return "\n".join(lines).rstrip()

parser_source = root / "DriverFix.Windows" / "PnpUtilInventoryParser.cs"
formatter_source = root / "DriverFix.Cli" / "DeviceInventoryTextFormatter.cs"

if parser_source.exists() and formatter_source.exists():
    parser_text = parser_source.read_text(encoding="utf-8")
    formatter_text = formatter_source.read_text(encoding="utf-8")
    assert 'Regex.Match(value, @"^\\s*(\\d+)")' in parser_text
    assert '"Идентификатор экземпляра"' in parser_text
    assert '"Compatible IDs"' in parser_text
    assert 'Connected devices: {devices.Count}' in formatter_text
    assert '"Hardware IDs"' in formatter_text
    assert '"Compatible IDs"' in formatter_text
    assert all(token not in parser_text + formatter_text for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install"
    ])
    print("PASS: frozen_production_contract_binding")

cases = [
    ("pnputil-integration-en.txt", "cli-integration-en.expected.txt"),
    ("pnputil-integration-ru.txt", "cli-integration-ru.expected.txt"),
]

for source_name, expected_name in cases:
    source = (fixtures / source_name).read_text(encoding="utf-8")
    expected = (fixtures / expected_name).read_text(encoding="utf-8").strip()
    actual = format_devices(parse(source))
    assert actual == expected, (
        f"{source_name} mismatch\n--- actual ---\n{actual}\n--- expected ---\n{expected}"
    )
    print(f"PASS: {source_name} -> exact expected output")

assert parse("") == []
assert format_devices([]) == "Connected devices: 0"
print("PASS: empty_inventory_output")
print("\nDFX-004 INTEGRATION REFERENCE PASS: 4/4" if parser_source.exists() else "\nDFX-004 INTEGRATION REFERENCE PASS: 3/3")
