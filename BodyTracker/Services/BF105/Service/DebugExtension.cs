using System;
using System.Diagnostics;
using System.Reflection;

namespace BF105.Service
{
    public static class DebugExtensions
    {
        /// <summary>
        /// Gibt alle öffentlichen Eigenschaften und deren Werte eines beliebigen Modells im Debug-Output aus.
        /// </summary>
        public static void DumpProperties<T>(this T model)
        {
#if DEBUG
            if (model == null)
            {
                Debug.WriteLine("[Debug] Das Modell ist null.");
                return;
            }

            Type type = model.GetType();
            Debug.WriteLine($"=== Start Dump: {type.Name} ===");

            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    object value = prop.GetValue(model);
                    Debug.WriteLine($"{prop.Name} = {value ?? "null"}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{prop.Name} = [Fehler beim Lesen: {ex.Message}]");
                }
            }

            Debug.WriteLine($"=== Ende Dump: {type.Name} ===");
#endif
        }
    }
}
