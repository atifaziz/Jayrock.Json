namespace JsonConversionsDemo
{
    #region Imports

    using System;
    using System.Reflection;
    using Jayrock.Json;
    using Jayrock.Json.Conversion;

    #endregion

    /// <summary>
    /// An importer for importing a duck-typed collection of elements from a
    /// JSON array.
    /// </summary>
    /// <remarks>
    /// The importer can infer the element type provided that
    /// the collection has an instance-base and public Add method that
    /// takes a single argument of the element type.
    /// </remarks>

    public class DuckCollectionImporter : DuckCollectionImporterBase
    {
        readonly MethodInfo _adder;

        public DuckCollectionImporter(Type outputType) :
            base(outputType, DuckCollectionReflector.InferElementType(outputType))
        {
            try
            {
                _adder = DuckCollectionReflector.FindAddMethod(outputType, ElementType);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message, nameof(outputType), e);
            }
        }

        protected override void InvokeAdd(object collection, object[] args)
        {
            _adder.Invoke(collection, args);
        }
    }

    /// <summary>
    /// This is the generic version of <see cref="DuckCollectionImporter"/>.
    /// </summary>

    public sealed class DuckCollectionImporter<TCollection, TElement> : DuckCollectionImporterBase
        where TCollection : new()
    {
        public DuckCollectionImporter() :
            base(typeof(TCollection), typeof(TElement)) { }

        protected override void ImportElements(object collection, ImportContext context, JsonReader reader)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var adder = DuckCollectionReflector.GetAdder<TElement>(collection);

            while (reader.TokenClass != JsonTokenClass.EndArray)
                adder((TElement) context.Import(typeof(TElement), reader));
        }

        protected override object CreateCollection()
        {
            return new TCollection();
        }

        protected override void InvokeAdd(object collection, object[] args)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Length != 1) throw new ArgumentException(null, nameof(args));

            //
            // NOTE! This implementation is horribly slow.
            // It is provided here only for sake of completeness but it
            // should never be needed for any practical reason.
            //

            DuckCollectionReflector.GetAdder<TElement>(collection)((TElement) args[0]);
        }
    }
}
