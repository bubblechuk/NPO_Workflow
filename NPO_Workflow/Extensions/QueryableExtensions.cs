using System.Linq.Expressions;

namespace NPO_Workflow.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(
            this IQueryable<T> source,
            string propertyName,
            bool descending = false)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";
            
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            var result = method.Invoke(null, new object[] { source, lambda })!;
            
            return (IQueryable<T>)result;
        }
    }
}