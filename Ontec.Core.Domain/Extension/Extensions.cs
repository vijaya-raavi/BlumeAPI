namespace Ontec.Core.Domain.Extension
{
    public static class Extensions
    {
        public static void TrimAllStrings<TSelf>(this TSelf obj)
        {
            var stringProperties = obj.GetType().GetProperties()
                                .Where(p => p.PropertyType == typeof(string));
            foreach (var stringProperty in stringProperties)
            {
                string currentValue = (string)stringProperty.GetValue(obj, null);
                if (currentValue != null)
                {
                    stringProperty.SetValue(obj, currentValue.Trim(), null);
                }

            }
        }
    }
}
