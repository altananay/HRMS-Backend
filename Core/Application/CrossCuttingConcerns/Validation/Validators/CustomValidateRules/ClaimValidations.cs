namespace Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules
{
    public static class ClaimValidations
    {
        public static bool ClaimLengthValidate(string claims)
        {
            if (claims.Length == 0)
            {
                return false;
            }
            return true;
        }

        public static bool ClaimArrayValidate(string[] claims)
        {
            if (claims.Length == 0)
            {
                return false;
            }
            return true;
        }
    }
}