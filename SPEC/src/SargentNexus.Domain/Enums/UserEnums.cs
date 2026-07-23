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