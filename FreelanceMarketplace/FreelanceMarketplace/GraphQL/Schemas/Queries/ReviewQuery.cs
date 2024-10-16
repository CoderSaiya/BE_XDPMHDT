﻿using FreelanceMarketplace.GraphQL.Authorization;
using FreelanceMarketplace.GraphQL.Types;
using FreelanceMarketplace.Services.Interface;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.VisualBasic;

namespace FreelanceMarketplace.GraphQL.Schemas.Queries
{
    public class ReviewQuery : ObjectGraphType
    {
        public ReviewQuery(IServiceProvider serviceProvider)
        {
            AddField(new FieldType
            {
                Name = "review",
                Type = typeof(ListGraphType<ReviewType>),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var reviewService = scope.ServiceProvider.GetRequiredService<IReviewService>();
                    return await reviewService.GetAllReviewsAsync();
                })
            }.AuthorizeWith("Admin"));

            AddField(new FieldType
            {
                Name = "reviewById",
                Type = typeof(ReviewType),
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "reviewId" }
                ),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    int reviewId = context.GetArgument<int>("reviewId");
                    using var scope = serviceProvider.CreateScope();
                    var reviewService = scope.ServiceProvider.GetRequiredService<IReviewService>();
                    return await reviewService.GetReviewByIdAsync(reviewId);
                })
            }.AuthorizeWith("Admin", "Client", "Freelancer"));

        }
    }
}
