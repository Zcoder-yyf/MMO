using System.Collections;
using UnityEngine;

namespace JKFrame
{
    /// <summary>
    /// 协程工具，避免GC
    /// </summary>
    public static class CoroutineTool
    {
        private struct WaitForFrameStruct : IEnumerator
        {
            public object Current => null;

            public bool MoveNext() { return false; }

            public void Reset() { }
        }

        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        public static WaitForEndOfFrame WaitForEndOfFrame()
        {
            return waitForEndOfFrame;
        }
        public static WaitForFixedUpdate WaitForFixedUpdate()
        {
            return waitForFixedUpdate;
        }
        public static IEnumerator WaitForSeconds(float time)
        {
            float currTime = 0;
            while (currTime < time)
            {
                currTime += Time.deltaTime;
                yield return new WaitForFrameStruct();
            }
        }

        public static IEnumerator WaitForSecondsRealtime(float time)
        {
            float currTime = 0;
            while (currTime < time)
            {
                currTime += Time.unscaledDeltaTime;
                yield return new WaitForFrameStruct();
            }
        }

        public static IEnumerator WaitForFrame()
        {
            yield return new WaitForFrameStruct();
        }
        public static IEnumerator WaitForFrames(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForFrameStruct();
            }
        }

        /// <summary>
        /// 是否正在编译Shader。
        /// 仅Unity Editor下可查询，Player中始终返回false。
        /// </summary>
        public static bool IsShaderCompiling
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.ShaderUtil.anythingCompiling;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 等待Unity Editor异步Shader编译完成。
        /// Player中无法查询Editor的Shader编译状态，会直接结束。
        /// </summary>
        public static IEnumerator WaitForShaderCompilation()
        {
            while (IsShaderCompiling)
            {
                yield return new WaitForFrameStruct();
            }
        }
    }
}
