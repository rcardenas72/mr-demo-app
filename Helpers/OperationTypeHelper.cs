using DemoApp.Web.Models.Enums;

namespace DemoApp.Web.Helpers
{
    public static class OperationTypeHelper
    {
        public static string GetOperationName(OperationType operation)
        {
            return operation switch
            {
                OperationType.Consultar => "Consultar",
                OperationType.Agregar => "Agregar",
                OperationType.Editar => "Editar",
                OperationType.Eliminar => "Eliminar",
                _ => ""
            };
        }

        public static List<KeyValuePair<string, string>> GetOperationList()
        {
            return ((OperationType[])System.Enum.GetValues(typeof(OperationType)))
                .Select(op => new KeyValuePair<string, string>(
                    GetOperationName(op), ((int)op).ToString()))
                .ToList();
        }

        public static bool TryParse(int value, out OperationType operation)
        {
            operation = default;
            if (System.Enum.IsDefined(typeof(OperationType), value))
            {
                operation = (OperationType)value;
                return true;
            }
            return false;
        }
    }
}
