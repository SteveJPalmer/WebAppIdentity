using Microsoft.AspNetCore.Authorization;

namespace WebAppIdentity.Authorization;

/* Requirement class */
public class HRManagerProbationRequirement : IAuthorizationRequirement
{
    public int ProbationMonths { get; }

    public HRManagerProbationRequirement(int probationMonths)
    {
        ProbationMonths = probationMonths;
    }   
}

/* Handler class */
public class HRManagerProbationRequirementHandler : AuthorizationHandler<HRManagerProbationRequirement>
{
    protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, HRManagerProbationRequirement requirement)
    {
        if (!context.User.HasClaim(x => x.Type == "EmploymentDate"))
        {
            return Task.CompletedTask;   // dont just return failure, as may chain with other Policies that may later pass
        }

        var empDate = DateTime.Parse(context.User.FindFirst(x => x.Type == "EmploymentDate").Value);
        var period = DateTime.Now - empDate;
        if (period.Days > 30 * requirement.ProbationMonths)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
