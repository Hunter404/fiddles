/// <summary>
/// Provides extension methods for enum types.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Retrieves the description attribute of an enum value, if it exists.
    /// </summary>
    /// <param name="enumerationValue">The enum value for which to retrieve the description.</param>
    /// <returns>The description attribute of the enum value, or the enum value itself if a description attribute does not exist.</returns>
    /// <exception cref="ArgumentException">Thrown if the parameter is not of type Enum.</exception>
    public static string GetDescription(this Enum enumerationValue)
    {
        var type = enumerationValue.GetType();
        if (!type.IsEnum)
        {
            throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");	
        }
        
        var memberInfo = type.GetMember(enumerationValue.ToString());
        if (memberInfo.Length <= 0)
        {
            return enumerationValue.ToString();
        }
        
        return (memberInfo[0].GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attribute)
            ? attribute.Description
            : enumerationValue.ToString();
    }

    /// <summary>
    /// Attempts to parse an enum value from a given description.
    /// </summary>
    /// <param name="description">The description for which to search.</param>
    /// <param name="result">When this method returns, contains the enum value associated with the given description, if the description is found; otherwise, the default value of the enum type. This parameter is passed uninitialized.</param>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <returns><c>true</c> if the description was found and successfully parsed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown if T is not of type Enum.</exception>
    public static bool TryParseEnum<T>(this string description, out T result)
    {
        var type = typeof(T);
        if (!type.IsEnum)
        {
            throw new ArgumentException("Generics type argument must be a Enum type.");	
        }
		
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (!attribute.Description.Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result = (T) field.GetValue(null)!;
                return true;
            }

            if (!field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result = (T) field.GetValue(null)!;
            return true;
        }
		
        result = default;
        return false;
    }
}
