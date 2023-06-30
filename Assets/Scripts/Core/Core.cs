using Ju.Services;

public static class Core
{
    // Methods to get services from the service container

    public static T Get<T>() =>                                  ServiceContainer.Get<T>();
    public static T Get<T>(string id) =>                         ServiceContainer.Get<T>(id);

    // Core services

    public static IEventBusService Event =>                      ServiceContainer.Get<IEventBusService>();
    public static ITaskService Task =>                           ServiceContainer.Get<ITaskService>();
    public static ICoroutineService Coroutine =>                 ServiceContainer.Get<ICoroutineService>();
    public static ICacheService Cache =>                         ServiceContainer.Get<ICacheService>();

    // Unity related services
    // Note: Comment out if you are using Unity

    //public static ITimeService Time =>                         ServiceContainer.Get<ITimeService>();
    //public static IUnityService Unity =>                       ServiceContainer.Get<IUnityService>();
    public static IInputService Input =>                       ServiceContainer.Get<IInputService>();
    //public static IPrefabPoolService Pool =>                   ServiceContainer.Get<IPrefabPoolService>();

    // ... add more shorthands as you need
}