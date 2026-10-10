namespace IdentityAuthLayer.API.Routes
{
    public sealed class AuthRoutes
    {
        private const string Base = "api/v1/Auth";
        public const string Register = Base + "/register";
        public const string VerifyAccount = Base + "/verify-account";
        public const string ResendVerificationCode = Base + "/resend-verification-code";
        public const string CreatePassword = Base + "/create-password";
        public const string Login = Base + "/login";
        public const string RefreshToken = Base + "/refresh-token";
        public const string ForgotPassword = Base + "/forgot-password";
        public const string VerifyResetCode = Base + "/verify-reset-code";
        public const string ResetPassword = Base + "/reset-password";
        public const string Logout = Base + "/logout";
        public const string CheckEmail = Base + "/check-email";
    }

    public sealed class ClientsRoutes
    {
        private const string Base = "api/v1/Clients";
        public const string Create = Base;
        public const string GetById = Base + "/{clientId}";
        public const string Update = Base + "/{clientId}";
        public const string ToggleActive = Base + "/{clientId}/toggle-active";
        public const string LinkSystem = Base + "/{clientId}/systems";
        public const string UnlinkSystem = Base + "/{clientId}/systems/{systemId}";
        public const string ToggleSystemActive = Base + "/{clientId}/systems/{systemId}/toggle-active";
    }

    public sealed class SystemsRoutes
    {
        private const string Base = "api/v1/Systems";
        public const string Create = Base;
        public const string GetById = Base + "/{systemId}";
        public const string Update = Base + "/{systemId}";
        public const string ToggleActive = Base + "/{systemId}/toggle-active";
    }

    public sealed class UsersRoutes
    {
        private const string Base = "api/v1/Users";
        public const string Create = Base;
        public const string Me = Base + "/me";
        public const string ChangePassword = Base + "/change-password";
        public const string GetById = Base + "/{userId}";
        public const string Update = Base + "/{userId}";
        public const string ToggleActive = Base + "/{userId}/toggle-active";
        public const string AssignRole = Base + "/{userId}/roles";
        public const string RemoveRole = Base + "/{userId}/roles/{roleId}";
        public const string GetPermissions = Base + "/{userId}/permissions";
        public const string CheckPermission = Base + "/{userId}/permissions/check";
    }

    public sealed class RolesRoutes
    {
        private const string Base = "api/v1/Roles";
        public const string Create = Base;
        public const string Update = Base + "/{roleId}";
        public const string Delete = Base + "/{roleId}";
        public const string AssignPermission = Base + "/{roleId}/permissions";
        public const string RemovePermission = Base + "/{roleId}/permissions/{permissionId}";
    }

    public sealed class PermissionsRoutes
    {
        private const string Base = "api/v1/Permissions";
        public const string Create = Base;
        public const string Update = Base + "/{permissionId}";
        public const string Delete = Base + "/{permissionId}";
    }
}
