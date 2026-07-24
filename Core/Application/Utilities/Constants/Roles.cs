namespace Application.Utilities.Constants
{
    /// <summary>
    /// Role names as they appear in the <c>Claims</c> array on each actor entity and in the
    /// <c>ClaimTypes.Role</c> claims of an issued token.
    /// </summary>
    /// <remarks>
    /// Values are lowercase to match what AuthManager and EmployerAuthManager hard-code at
    /// registration. Note that no code path currently assigns <see cref="Admin"/> — Phase 4
    /// introduces role seeding so an administrator can exist at all.
    /// </remarks>
    public static class Roles
    {
        public const string JobSeeker = "jobseeker";
        public const string Employer = "employer";
        public const string Admin = "admin";
    }
}
