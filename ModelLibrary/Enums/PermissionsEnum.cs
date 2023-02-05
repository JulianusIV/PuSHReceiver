namespace Models.Enums
{
    [Flags]
    public enum PermissionsEnum
    {
        root = 1,
        ManageForeignLeases = 2,
        ManageAssignedLeases = 4,
        ViewAllLeases = 8,
        ViewAssignedLeases = 16,
    }
}
