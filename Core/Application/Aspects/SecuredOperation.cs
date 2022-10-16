using Application.Constants;
using Application.Utilities.Interceptors;
using Application.Utilities.IoC;
using Application.Extensions;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Aspects
{
    public class SecuredOperation : MethodInterception
    {
        private string[] _roles;
        //Web tabanlı bir projede birden çok kişi aynı anda istekte bulunabilir.
        //her ayrı kişi için bir httpcontext oluşturacak. Herkese ayrı bir thread.
        private IHttpContextAccessor _httpContextAccessor;

        public SecuredOperation(string roles)
        {
            _roles = roles.Split(',');
            _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();

        }

        protected override void OnBefore(IInvocation invocation)
        {
            var roleClaims = _httpContextAccessor.HttpContext.User.ClaimRoles();
            foreach (var role in _roles)
            {
                if (roleClaims.Contains(role))
                {
                    return;
                }
            }
            throw new Exception(Messages.AuthorizationDenied);
        }
    }
}