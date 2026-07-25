namespace Application.Abstractions
{
    /// <summary>
    /// The authenticated caller, as seen by the Application layer.
    /// </summary>
    /// <remarks>
    /// Replaces <c>ServiceTool.ServiceProvider.GetService&lt;IHttpContextAccessor&gt;()</c>, which the
    /// SecuredOperation and LogAspect attributes used to reach the current request.
    ///
    /// That call resolved from a second, orphaned root container built by
    /// <c>services.BuildServiceProvider()</c> inside AddDependencyResolvers — before the real host
    /// container existed. It only ever worked because HttpContextAccessor stores the context in a
    /// static AsyncLocal, so the orphan's instance happened to observe the right request. That
    /// accident was load-bearing for the entire authorization model.
    ///
    /// Implemented in WebAPI over IHttpContextAccessor and registered scoped, so Application depends
    /// on an interface it owns rather than on ASP.NET Core.
    /// </remarks>
    public interface ICurrentUserService
    {
        /// <summary>Authenticated user id, or <c>null</c> for anonymous requests.</summary>
        string? UserId { get; }

        string? Email { get; }

        IReadOnlyCollection<string> Roles { get; }

        bool IsAuthenticated { get; }

        /// <summary>Caller's IP, recorded on issued and revoked refresh tokens for audit.</summary>
        string? IpAddress { get; }

        bool IsInRole(string role);
    }
}
