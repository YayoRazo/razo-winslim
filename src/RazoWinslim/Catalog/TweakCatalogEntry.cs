namespace RazoWinslim.Catalog;

public enum RiskTier { Safe, Advanced }

public enum TargetType { Service, ScheduledTask, RegistryValue, StartupEntry, AppxPackage, DefenderProtection }

public sealed record TweakCatalogEntry(
    string Id,
    string Category,
    string DisplayName,
    string Description,
    RiskTier RiskTier,
    TargetType TargetType,
    Dictionary<string, string> TargetIdentifier);
