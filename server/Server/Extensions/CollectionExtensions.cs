namespace Server.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Adds many items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="collection">The collection to add to.</param>
    /// <param name="items">The object to add to the <see cref="ICollection{T}"/>.</param>
    public static void AddMany<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }
}
