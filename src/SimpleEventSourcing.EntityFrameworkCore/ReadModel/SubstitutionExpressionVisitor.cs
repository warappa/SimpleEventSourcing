using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public partial class ReadRepository<TDbContext> where TDbContext : DbContext
    {
        private class SubstitutionExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression before;
            private readonly Expression after;

            public SubstitutionExpressionVisitor(Expression before, Expression after)
            {
                this.before = before;
                this.after = after;
            }

            public override Expression Visit(Expression node)
            {
                return node == before ? after : base.Visit(node);
            }
        }
    }
}
