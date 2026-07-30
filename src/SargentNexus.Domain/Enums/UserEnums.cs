namespace SargentNexus.Domain;

public enum UserRole
{
    SiteAdmin = 1,
    OrgAdmin = 2,
    User = 3,
    ReadOnly = 4
}

public enum UserLifecycleStatus
{
    Active = 1,
    Inactive = 2
}

public enum IdeaPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum IdeaApprovalState
{
    None = 0,
    PendingApproval = 1
}