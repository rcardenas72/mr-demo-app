
namespace DemoApp.Web.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotAuditedAttribute : Attribute
    {
    }
}
