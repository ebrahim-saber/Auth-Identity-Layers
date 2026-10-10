namespace IdentityAuthLayer.Domain.Enums
{
    /// <summary>
    /// تصنيف اليوزر (Category) - بغض النظر عن التصنيف، الصلاحيات بتتحدد عن طريق Roles/Permissions.
    /// </summary>
    public enum UserType
    {
        Admin = 1,
        GroupAdmin = 2,
        User = 3
    }
}
