using DemoApp.Web.Models.Attributes;
using DemoApp.Web.Models.Interfaces;
using DemoApp.Web.Models.Shared;
using System.Collections.Concurrent;
using System.Reflection;

namespace DemoApp.Web.Helpers
{
    public static class ReflectionCacheHelper
    {
        private static readonly ConcurrentDictionary<Type, HashSet<string>> _ignoredPropsCache = new();

        public static HashSet<string> GetNotAuditedProperties(Type type)
        {
            return _ignoredPropsCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.GetCustomAttribute<NotAuditedAttribute>() != null)
                 .Select(p => p.Name)
                 .ToHashSet()
            );
        }
        private static readonly Lazy<List<Type>> _auditableEntities = new(() =>
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(AuditableEntity).IsAssignableFrom(t) &&
                    !typeof(INotAuditableEntity).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();
        });

        public static List<Type> GetAuditableEntityTypes()
        {
            return _auditableEntities.Value;
        }

    }


}
