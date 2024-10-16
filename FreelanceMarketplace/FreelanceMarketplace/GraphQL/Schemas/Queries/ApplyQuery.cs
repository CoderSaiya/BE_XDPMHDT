﻿using FreelanceMarketplace.GraphQL.Types;
using FreelanceMarketplace.Services.Interfaces;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using FreelanceMarketplace.GraphQL.Authorization;

namespace FreelanceMarketplace.GraphQL.Schemas.Queries
{
    public class ApplyQuery : ObjectGraphType
    {
        public ApplyQuery(IServiceProvider serviceProvider)
        {
            AddField(new FieldType
            {
                Name = "applies",
                Type = typeof(ListGraphType<ApplyType>),
                Arguments = new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "projectId" }
                ),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var applyService = scope.ServiceProvider.GetRequiredService<IApplyService>();
                        return await applyService.GetAppliesForProjectAsync(context.GetArgument<int>("projectId"));
                    }
                })
            }.AuthorizeWith("Freelancer", "Client"));

            AddField(new FieldType
            {
                Name = "applyById",
                Type = typeof(ApplyType),
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "applyId" }
                ),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    int applyId = context.GetArgument<int>("applyId");
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var applyService = scope.ServiceProvider.GetRequiredService<IApplyService>();
                        return await applyService.GetApplyByIdAsync(applyId);
                    }
                })
            }.AuthorizeWith("Freelancer", "Client"));
        }
    }
}
