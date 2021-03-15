namespace Simple.EventStore.Internal.Helpers
{
	using System;
	using System.Reflection;

	internal static class ReflectionHelpers
    {
        /// <summary>
        /// Returns a PRIVATE Property Value from a given Object using reflection
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="instance">Object from where the Property Value is returned</param>
        /// <param name="propertyName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivatePropertyValue<T>(this object instance, string propertyName)
        {
            if (instance == null) 
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var propertyInfo = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), $"Property {propertyName} was not found in Type {instance.GetType().FullName}");
            }

            return (T) propertyInfo.GetValue(instance, null);
        }

        /// <summary>
        /// Returns a PRIVATE Property Value from a given Object using reflection
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="instance">Object from where the Property Value is returned</param>
        /// <param name="propertyName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivateFieldValue<T>(this object instance, string propertyName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var instanceType = instance.GetType();
            FieldInfo fi = null;
            while (fi == null && instanceType != null)
            {
                fi = instanceType.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                instanceType = instanceType.BaseType;
            }

            if (fi == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), $"Field {propertyName} was not found in Type {instance.GetType().FullName}");
            }

            return (T) fi.GetValue(instance);
        }

        /// <summary>
        /// Sets a PRIVATE Property Value from a given Object using reflection
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="instance">Object from where the Property Value is set</param>
        /// <param name="propertyName">Propertyname as string.</param>
        /// <param name="newValue">Value to set.</param>
        /// <returns>PropertyValue</returns>
        public static void SetPrivatePropertyValue<T>(this object instance, string propertyName, T newValue)
        {
            var instanceType = instance.GetType();
            if (instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), $"Property {propertyName} was not found in Type {instance.GetType().FullName}");
            }

            instanceType.InvokeMember(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, instance, new object[] {newValue});
        }

        /// <summary>
        /// Sets a PRIVATE Property Value on a given Object using reflection
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="instance">Object from where the Property Value is returned</param>
        /// <param name="propertyName">Propertyname as string.</param>
        /// <param name="newValue">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        public static void SetPrivateFieldValue<T>(this object instance, string propertyName, T newValue)
        {
            if (instance == null) 
            {
                throw new ArgumentNullException(nameof(instance));
            }
            
            var instanceType = instance.GetType();
            FieldInfo fieldInfo = null;
            
            while (fieldInfo == null && instanceType != null)
            {
                fieldInfo = instanceType.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                instanceType = instanceType.BaseType;
            }

            if (fieldInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), $"Field {propertyName} was not found in Type {instance.GetType().FullName}");
            }

            fieldInfo.SetValue(instance, newValue);
        }
    }
}