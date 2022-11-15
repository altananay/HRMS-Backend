using Domain.Common;

namespace Domain.Entities
{
    public class UserOperationClaim
    {
        public string UserId { get; set; }
        public string[] UserClaims { get; set; }
    }
}