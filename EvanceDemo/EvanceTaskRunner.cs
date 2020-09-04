using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EvanceDemo
{
    public class EvanceTaskRunner : TaskRunnerBase
    {
        private TaskExecutor _taskExecutor;

        private int _taskTotalSteps;
        private int _workersCountForEachQueue;


        public EvanceTaskRunner(int taskTotalSteps = 3, int workersCountForEachQueue = 5)
        {
            _taskTotalSteps = taskTotalSteps;
            _workersCountForEachQueue = workersCountForEachQueue;

            _taskExecutor = new TaskExecutor(_taskTotalSteps, _workersCountForEachQueue);


            //_taskExecutor.Execute();
        }


        public override void Run(IEnumerable<MyTask> myTasks)
        {
            //_taskExecutor.InitializeAddTasks(myTasks);


            //foreach (var task in tasks)
            //{
            //    task.DoStepN(1);
            //    task.DoStepN(2);
            //    task.DoStepN(3);
            //    Console.WriteLine($"exec job: {task.ID} completed.");
            //}


            //Parallel.ForEach(myTasks, myTask =>
            //{
            //    _taskExecutor.AddTask(myTask);
            //});
            //foreach (var myTask in myTasks)
            //{
            //    _taskExecutor.AddTask(myTask);
            //}


            //Task.WaitAll();
            _taskExecutor.Execute(myTasks);

            //SpinWait.SpinUntil(() => myTasks.All(t => t.CurrentStep == _taskTotalSteps));
        }
    }

    //public class TaskExecutor_old
    //{
    //    private static readonly int _taskTotalSteps = 3;
    //    private BlockingCollection<MyTask> _taskQueue = new BlockingCollection<MyTask>();

    //    public void Execute()
    //    {
    //        foreach (var task in _taskQueue.GetConsumingEnumerable())
    //        {
    //            if (task.CurrentStep + 1 > _taskTotalSteps)
    //            {
    //                continue;
    //            }

    //            task.DoStepN(task.CurrentStep + 1);
    //        }
    //    }

    //    public void AddTask(MyTask task)
    //    {
    //        _taskQueue.Add(task);
    //    }
    //}



    public class TaskExecutor
    {
        private IEnumerable<MyTask> _myTasks;
        private int _taskTotalSteps;
        private int _workersCountForEachQueue;
        //private BlockingCollection<MyTask> _taskQueue = new BlockingCollection<MyTask>();

        private ConcurrentDictionary<int, BlockingCollection<MyTask>> _taskQueueDict = new ConcurrentDictionary<int, BlockingCollection<MyTask>>();
        private volatile int _completeTasks = 0;
        //private Interlocked _interlocked = new Interlocked();

        public TaskExecutor(int taskTotalSteps = 3, int workersCountForEachQueue = 3)
        {
            //_myTasks = myTasks;
            _taskTotalSteps = taskTotalSteps;
            _workersCountForEachQueue = workersCountForEachQueue;


            //this.Execute();
            //InitializeAddTasks();

            this.Initialize();
        }

        private void Initialize()
        {
            for (int i = 0; i < _taskTotalSteps; i++)
            {
                _taskQueueDict[i + 1] = new BlockingCollection<MyTask>();
            }

            foreach (var kv in _taskQueueDict)
            {
                for (int i = 0; i < _workersCountForEachQueue; i++)
                {
                    //var worker = new Thread(() =>
                    //{
                    //    foreach (var task in kv.Value.GetConsumingEnumerable())
                    //    {
                    //        task.DoStepN(task.CurrentStep + 1);

                    //        if (task.CurrentStep < _taskTotalSteps)
                    //        {
                    //            this.AddTask(task);
                    //        }
                    //    }
                    //});

                    //worker.Start();
                    //Console.WriteLine($"Started new worker...  TaskQueueStep: {kv.Key}, ManagedThreadId: {worker.ManagedThreadId}");

                    //Task.Run(() =>
                    //{
                    //    foreach (var task in kv.Value.GetConsumingEnumerable())
                    //    {
                    //        task.DoStepN(task.CurrentStep + 1);

                    //        if (task.CurrentStep < _taskTotalSteps)
                    //        {
                    //            this.AddTask(task);
                    //        }
                    //        else
                    //        {
                    //            Interlocked.Increment(ref _completeTasks);
                    //        }
                    //    }
                    //});

                    var worker = new Thread(() =>
                    {
                        foreach (var task in kv.Value.GetConsumingEnumerable())
                        {
                            task.DoStepN(task.CurrentStep + 1);

                            if (task.CurrentStep < _taskTotalSteps)
                            {
                                this.AddTask(task);
                            }
                            else
                            {
                                Interlocked.Increment(ref _completeTasks);
                            }
                        }
                    });
                    worker.Start();
                    Console.WriteLine($"Started new worker...  TaskQueueStep: {kv.Key}, ManagedThreadId: {worker.ManagedThreadId}");

                }
            }
        }



        public void Execute(IEnumerable<MyTask> myTasks)
        {
            this.InitializeAddTasks(myTasks);



            //Parallel.ForEach(_taskQueueDict, kv =>
            //{
            //    foreach (var task in kv.Value.GetConsumingEnumerable())
            //    {
            //        task.DoStepN(task.CurrentStep + 1);

            //        if (task.CurrentStep < _taskTotalSteps)
            //        {
            //            this.AddTask(task);
            //        }
            //    }

            //});

            SpinWait.SpinUntil(() => _completeTasks == _myTasks.Count());
        }

        private void InitializeAddTasks(IEnumerable<MyTask> myTasks)
        {
            _myTasks = myTasks;

            _myTasks.AsParallel().ForAll(myTask => this.AddTask(myTask));

            //foreach (var myTask in _myTasks)
            //{
            //    this.AddTask(myTask);
            //}

        }

        private bool AddTask(MyTask task)
        {
            if (task.CurrentStep >= _taskTotalSteps)
            {
                return false;
            }

            _taskQueueDict[task.CurrentStep + 1].Add(task);
            return true;
        }
    }







}
