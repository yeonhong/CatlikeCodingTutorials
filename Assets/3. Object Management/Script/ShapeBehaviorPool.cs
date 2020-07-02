using System.Collections.Generic;
using UnityEngine;

namespace ObjectManagement
{
	// Unity 컴포넌트를 사용하지 않는 단점은 셰이프 동작이 더 이상 핫 리로드에서 살아남지 못한다는 것입니다.
	// 재 컴파일이 끝나면 모든 동작이 사라집니다.
	// 이것은 빌드에는 문제가 아니지만 편집기에서 작업하는 동안 성가실 수 있습니다.
	public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new()
	{
		private static Stack<T> stack = new Stack<T>();

		public static T Get() {
			if (stack.Count > 0) {
				T behavior = stack.Pop();
#if UNITY_EDITOR
				behavior.IsReclaimed = false;
#endif
				return behavior;
			}
#if UNITY_EDITOR
			return ScriptableObject.CreateInstance<T>();
#else
			return new T();
#endif
		}

		public static void Reclaim(T behavior) {
#if UNITY_EDITOR
			behavior.IsReclaimed = true;
#endif
			stack.Push(behavior);
		}
	}
}