namespace Application.Utilities.Constants
{
    public static class Roles
    {
        public const string JobSeeker = "jobseeker";
        public const string Employer = "employer";
        public const string Admin = "admin";

        // Moderation surfaces. An admin reaches the same endpoint as the owning role and skips the
        // ownership check inside the manager. Kept here so "who may moderate" is one edit, not six.
        public const string EmployerOrAdmin = $"{Employer},{Admin}";
        public const string JobSeekerOrAdmin = $"{JobSeeker},{Admin}";
    }
}
