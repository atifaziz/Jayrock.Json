namespace JsonConversionsDemo
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Jayrock.Json;
    using Jayrock.Json.Conversion;

    #endregion

    /// <summary>
    /// An importer for importing a collection of elements from a JSON array.
    /// </summary>

    public class CollectionImporter<TCollection, TElement> : CollectionImporterBase
        where TCollection : ICollection<TElement>, new()
    {
        public CollectionImporter() :
            base(typeof(TCollection), typeof(TElement)) { }

        protected override object CreateCollection()
        {
            return new TCollection();
        }

        protected override void ImportElements(object collection, ImportContext context, JsonReader reader)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            ImportElements((ICollection<TElement>) collection, context, reader);
        }

        static void ImportElements(ICollection<TElement> collection, ImportContext context, JsonReader reader)
        {
            Debug.Assert(collection != null);
            Debug.Assert(context != null);
            Debug.Assert(reader != null);

            while (reader.TokenClass != JsonTokenClass.EndArray)
                collection.Add((TElement) context.Import(typeof(TElement), reader));
        }
    }
}
