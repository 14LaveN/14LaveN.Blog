namespace Application.Core.Abstractions.Helpers.Architecture;

/// <summary>
/// A statically compiled "singleton" used to store objects throughout the 
/// lifetime of the app domain. Not so much singleton in the pattern's 
/// sense of the word as a standardized way to store single instances.
/// </summary>
/// <typeparam name="T">The type of object to store.</typeparam>
/// <remarks>Access to the instance is not synchronized.</remarks>
public sealed class Singleton<T> 
    : BaseSingleton
    where T : class
{
    private static T _instance;

    /// <summary>
    /// The singleton instance for the specified type T. Only one instance (at the time) of this object for each type of T.
    /// </summary>
    public static T Instance
    {
        get => _instance;
        set
        {
            _instance = value;
            AllSingletons[typeof(T)] = value;
        }
    }
}

public sealed class ConcurrentSingleton
{
    private static readonly Lazy<ConcurrentSingleton> Lazy =
        new(() => new ConcurrentSingleton());

    public static ConcurrentSingleton Instance =>
        Lazy.Value;

    private ConcurrentSingleton()
    {
    }
}

