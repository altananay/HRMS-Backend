namespace Application.Abstractions
{
    public enum PasswordVerificationOutcome
    {
        Failed = 0,
        Success = 1,

        SuccessRehashNeeded = 2
    }

    public interface IPasswordHasher
    {
        string Hash(string password);

        PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword);
    }
}
