using UnityEngine;
using Ju.Services;

public static class Bootstrap
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		// Core services

		ServiceContainer.RegisterService<ITaskService, TaskService>();
		ServiceContainer.RegisterService<ICoroutineService, CoroutineService>();
		ServiceContainer.RegisterService<ICacheService, CacheService>();

		// Unity related services

		ServiceContainer.RegisterService<ILogUnityService, LogUnityService>();
		ServiceContainer.RegisterService<IInputService, InputUnityService>();
		ServiceContainer.RegisterService<ITimeService, UnityTimeService>();
		ServiceContainer.RegisterService<IUnityService, UnityService>();
		ServiceContainer.RegisterService<IPrefabPoolService, PrefabPoolService>();

		// Register your custom services here
		ServiceContainer.RegisterService<IAnimatorHelperService, AnimatorHelperService>();
		ServiceContainer.RegisterService<DialogueService, DialogueService>();
		ServiceContainer.RegisterService<PositionRecorderService, PositionRecorderService>();
		ServiceContainer.RegisterService<AudioService, AudioService>();
		ServiceContainer.RegisterService<CameraEffectsService, CameraEffectsService>();
	}
}