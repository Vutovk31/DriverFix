namespace DriverFix.Core.Matching;

public enum DriverIdentifierMatchKind
{
    None = 0,
    HardwareToHardware,
    HardwareToCompatible,
    CompatibleToHardware,
    CompatibleToCompatible
}
