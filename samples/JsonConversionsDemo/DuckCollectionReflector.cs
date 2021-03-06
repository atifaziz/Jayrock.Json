namespace JsonConversionsDemo
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    /// <summary>
    /// Provides duck-type reflection services for collection-like types.
    /// This means that a type in question does not have to be a real
    /// collection as long as it seems like one.
    /// </summary>

    public static class DuckCollectionReflector
    {
        public static Type InferElementType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var indexers = type.FindMembers(MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.Public, IsIndexer, null);

            if (indexers.Length == 0)
                throw new ArgumentException(string.Format("{0} does not appear to have an indexer property.", type.FullName), nameof(type));

            return ((PropertyInfo) indexers[0]).PropertyType;
        }

        static bool IsIndexer(MemberInfo m, object filterCriteria)
        {
            if (m.Name != "Item")
                return false;

            Debug.Assert(m is PropertyInfo);

            var property = (PropertyInfo) m;
            return property.CanRead && property.GetIndexParameters().Length == 1;
        }

        public static MethodInfo FindAddMethod(Type type)
        {
            return FindAddMethod(type, null);
        }

        public static MethodInfo FindAddMethod(Type type, Type elementType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (elementType == null)
                elementType = InferElementType(type);

            return type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance,
                /* binder */ null, new[] { elementType }, /* modifiers */ null);
        }

        public static MethodInfo GetAddMethod(Type type)
        {
            return GetAddMethod(type, null);
        }

        public static MethodInfo GetAddMethod(Type type, Type elementType)
        {
            var adder = FindAddMethod(type, elementType);

            if (adder == null)
            {
                throw new ArgumentException(string.Format(
                    "{0} has no public Add method that takes a single {1} argument.",
                    type.FullName, elementType.FullName), nameof(type));
            }

            return adder;
        }

        public static Action<Element> GetAdder<Element>(object collection)
        {
            var adder = FindAddder<Element>(collection);

            if (adder == null)
            {
                throw new ArgumentException(string.Format(
                    "{0} has no public Add method that takes a single {1} argument.",
                    collection.GetType().FullName, typeof(Element).FullName), nameof(collection));
            }

            return adder;
        }

        public static Action<Element> FindAddder<Element>(object collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var adder = FindAddMethod(collection.GetType(), typeof(Element));

            if (adder == null)
                return null;

            return (Action<Element>) Delegate.CreateDelegate(typeof(Action<Element>), collection, adder);
        }
    }
}
