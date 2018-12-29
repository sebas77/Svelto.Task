#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.Tasks.Unity.Internal;

#if TASKS_PROFILER_ENABLED
using Svelto.Tasks.Profiler;
#endif

namespace Svelto.Tasks.Unity
{
    /// <summary>
    /// Remember, unless you are using the StandardSchedulers, nothing hold your runners. Be careful that if you
    /// don't hold a reference, they will be garbage collected even if tasks are still running
    /// </summary>

    public abstract class MonoRunner<T> : IRunner<T> where T:IEnumerator
    {
        public bool isPaused
        {
            get { return _flushingOperation.paused; }
            set { _flushingOperation.paused = value; }
        }

        public bool isStopping { get { return _flushingOperation.stopped; } }
        public bool isKilled { get {return _flushingOperation.kill;} }
        public int  numberOfRunningTasks { get { return _coroutines.Count; } }
        public int numberOfQueuedTasks { get { return _newTaskRoutines.Count; } }
        
        protected MonoRunner(string name)
        {
            _name = name;
        }

        ~MonoRunner()
        {
            Utilities.Console.LogWarning("MonoRunner has been garbage collected, this could have serious" +
                                                "consequences, are you sure you want this? ".FastConcat(_name));
            
            StopAllCoroutines();
        }
        
        /// <summary>
        /// TaskRunner doesn't stop executing tasks between scenes
        /// it's the final user responsibility to stop the tasks if needed
        /// </summary>
        public virtual void StopAllCoroutines()
        {
            isPaused = false;

            UnityCoroutineRunner<T>.StopRoutines(_flushingOperation);

            _newTaskRoutines.Clear();
        }

        public virtual void StartCoroutine(ISveltoTask<T> task)
        {
            isPaused = false;

            _newTaskRoutines.Enqueue(task); //careful this could run on another thread!
        }

        public virtual void Dispose()
        {
            StopAllCoroutines();

            _flushingOperation.kill = true;
            
            GC.SuppressFinalize(this);
        }
        
        protected readonly ThreadSafeQueue<ISveltoTask<T>> _newTaskRoutines = new ThreadSafeQueue<ISveltoTask<T>>();
        protected readonly FasterList<ISveltoTask<T>> _coroutines =
            new FasterList<ISveltoTask<T>>(NUMBER_OF_INITIAL_COROUTINE);
        internal UnityCoroutineRunner<T>.FlushingOperation _flushingOperation =
            new UnityCoroutineRunner<T>.FlushingOperation();

        readonly string _name;

        const int NUMBER_OF_INITIAL_COROUTINE = 3;
    }
}
#endif
