namespace JsonConversionsDemo
{
    #region Imports

    using System;
    using Jayrock.Json;
    using Jayrock.Json.Conversion;
    using Jayrock.Json.Conversion.Converters;

    #endregion

    /// <summary>
    /// An abstract base class for importer implementations that can import
    /// a concrete collection instance from a JSON array.
    /// </summary>

    public abstract class CollectionImporterBase : ImporterBase
    {
        protected CollectionImporterBase(Type outputType, Type elementType) :
            base(outputType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public Type ElementType { get; }

        protected override object ImportFromArray(ImportContext context, JsonReader reader)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var collection = CreateCollection();

            reader.ReadToken(JsonTokenClass.Array);

            ImportElements(collection, context, reader);

            if (reader.TokenClass != JsonTokenClass.EndArray)
                throw new Exception("Implementation error.");

            reader.Read();
            return collection;
        }

        protected abstract object CreateCollection();
        protected abstract void ImportElements(object collection, ImportContext context, JsonReader reader);
    }
}
