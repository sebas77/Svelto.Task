
namespace Svelto.Tasks
{
    public class SyncRunner : IRunner
    {
        private bool _sleepInBetween;

        public bool paused { set; get; }
        public bool stopped { private set; get; }

        public SyncRunner(bool sleepInBetween)
        {
            _sleepInBetween = sleepInBetween;
        }

        public void StartCoroutineThreadSafe(IPausableTask task)
        {
            StartCoroutine(task);
        }

        public void StartCoroutine(IPausableTask task)
        {
            if (_sleepInBetween)
                while (task.MoveNext() == true) System.Threading.Thread.Sleep(1);
            else
                while (task.MoveNext() == true);
        }

        /// <summary>
        /// TaskRunner doesn't stop executing tasks between scenes
        /// it's the final user responsability to stop the tasks if needed
        /// </summary>
        public void StopAllCoroutines()
        {}

        public int numberOfRunningTasks { get { return -1; } }
    }
}