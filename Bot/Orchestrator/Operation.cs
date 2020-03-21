﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MamaBot
{
    class Orchestrator
    {
    }
    public static class Transversal
    {
        public static Perf Checker { get; set; } = new Perf();
        public static bool IsShutingDown { get; set; } = false;
    }
    public class Management
    {
        public enum Operation
        {
            CheckIfEnoughRessources,
            CheckifUserIsAway,
            ForceOperation,
            SlowDependanciesForce,
            Bulk
        }
        public enum OperationStatus
        {
            Started,
            Paused,
            Finished,
            Exception,
            Pending
        }
        public class PlanifiedOperation : Perf
        {

            public string ClassName { get; set; }
            public DateTime Start { get; set; }
            public string OperationName { get; set; }
            public TimeSpan EstimatedDuration { get; set; }
            public TimeSpan RealDuration { get; set; }
            public bool LargeMemoryOperation { get; set; } = false;
            public bool LargeNetworkOperation { get; set; } = false;
            public bool HighlyConsumeCPU { get; set; } = false;
            public bool ShouldgetPressure { get; set; } = false;
            public Operation TypeOFApproach { get; set; } = Operation.ForceOperation;
            public Action ContiniousAction { get; set; }
            public DateTime End { get; set; }
            public bool RecurentOperation { get; set; }
            public Action OperationCode { get; set; }
            public List<Task> OperationThreads { get; set; }
            public bool OperationIsSTA { get; set; }
            public List<Exception> Errors { get; set; } = new List<Exception>();
            public bool AllowMultipleException { get; set; }
            public bool ContiniousOperation { get; set; }
            public object ResultedObject { get; set; } = new object();
            public OperationStatus State { get; set; } = OperationStatus.Pending;
            public string Comment { get; set; }
            public TimeSpan Every { get; set; }
        }
        public class Perf

        {
            public ObservableCollection<PlanifiedOperation> ToManage { get; set; } = new ObservableCollection<PlanifiedOperation>();
            public ObservableCollection<PlanifiedOperation> ToManage2 { get; set; } = new ObservableCollection<PlanifiedOperation>();
            public ObservableCollection<PlanifiedOperation> Finished { get; set; } = new ObservableCollection<PlanifiedOperation>();
            public long MemoryUsage { get; set; } = 0;
            public DateTime LastCheck { get; set; } = DateTime.Now;
            public DateTime LastClean { get; set; } = DateTime.Now;
            public int GenerationSupported { get; set; } = GC.MaxGeneration;
            public long Threeshold { get; set; } = 50747520;
            public long Freedmemory { get; set; } = 0;
            public int NumberOfRunningOperation { get; set; } = 0;
            public string Report { get; set; } = "";
            /// <summary>
            /// Prepare Memory for large allocation before doing it
            /// Could check if memory wanted is present before doing it and should be used before call with hasenoughmemory
            /// </summary>
            public void PrepareLargeOperation()
            {
                try
                {
                    GC.AddMemoryPressure(5000000);
                }
                catch
                {

                }
            }
            public Perf()
            {
                ToManage.CollectionChanged += ToManage_CollectionChanged;
                ToManage2.CollectionChanged += ToManage_CollectionChanged;
                MonitoringRunningThread();
                Task.Factory.StartNew(() =>
                {
                    while (MamaBot.GlobalShared.Vars.IsRunning)
                    {
                        Process Me = Process.GetCurrentProcess();
                        MemoryUsage = Me.PrivateMemorySize64;
                        LastCheck = DateTime.Now;
                        if (MemoryUsage > Threeshold)
                        {
                            MamaBot.GlobalShared.Vars.Logger.Info(string.Format("Perf - Memory : Last check was : {0}", LastCheck));
                            MamaBot.GlobalShared.Vars.Logger.Info(string.Format("Perf - Memory : Check {0} bytes is higher than Threeshold : {1} bytes", MemoryUsage, Threeshold));
                            MamaBot.GlobalShared.Vars.Logger.Info(string.Format("Perf - Memory : Starting Memory Garbage Collection"));
                            MamaBot.GlobalShared.Vars.Logger.Info(string.Format("Perf : Garbage Collection deleted {0} bytes ", Freedmemory));
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            LastClean = DateTime.Now;
                            Freedmemory = MemoryUsage - Process.GetCurrentProcess().PrivateMemorySize64;

                        }
                        Thread.Sleep(new TimeSpan(0, 0, 50));
                    }

                });
            }


            private void MonitoringRunningThread()
            {
                Task.Factory.StartNew(() =>
                {
                    int CurrentOperationRunning = 0;
                    int LastOperationCheck = 0;
                    bool HasChanged = CurrentOperationRunning != LastOperationCheck;

                    while (!Transversal.IsShutingDown)
                    {
                        if (HasChanged)
                        {
                            CurrentOperationRunning = this.NumberOfRunningOperation;
                        }
                        LastOperationCheck = NumberOfRunningOperation;

                        System.Threading.Thread.Sleep(30000);
                    }
                });

            }
            private void ToManage_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {

                    PlanifiedOperation Operation = (PlanifiedOperation)e.NewItems[0];
                    if (Operation != null)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            object lo = new object();
                            System.Threading.Monitor.Enter(lo);
                            bool OperationExecuted = false;
                            while (!OperationExecuted)
                            {
                                DateTime StartOperationDate = DateTime.Parse(Operation.Start.ToString("HH:mm:ss"));
                                DateTime Current = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss"));
                                if (StartOperationDate <= Current & StartOperationDate.Minute == Current.Minute & Operation.State == OperationStatus.Pending)
                                {
                                    try
                                    {
                                        NumberOfRunningOperation++;
                                        Exception ex = new Exception();
                                        Stopwatch RealOperationTime = new Stopwatch();
                                        RealOperationTime.Start();
                                        Operation.OperationCode.Invoke();
                                        RealOperationTime.Stop();
                                        Operation.End = DateTime.Now;
                                        Operation.RealDuration = RealOperationTime.Elapsed;
                                        Operation.ResultedObject = Operation.OperationCode.Target;
                                        Operation.State = OperationStatus.Finished;
                                        OperationExecuted = true;
                                        Finished.Add(Operation);
                                        ToManage.Remove(Operation);

                                        if (Operation.ContiniousOperation & Operation.State == OperationStatus.Finished)
                                        {

                                            Operation.Start = DateTime.Now.Add(Operation.Every);
                                            Operation.State = OperationStatus.Pending;
                                            ToManage.Add(Operation);
                                            Console.WriteLine("Perf - TaskManagement : scheduling new operation for continious logic at : {0}", Operation.Start);

                                            break;
                                        }
                                        else
                                        {
                                            //Operation.OperationCode.Dispose();

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        if (Operation.AllowMultipleException)
                                        {
                                            Stopwatch Retry = new Stopwatch();
                                            Retry.Start();

                                            Operation.OperationCode.Invoke();
                                            Retry.Stop();
                                            Operation.End = DateTime.Now;
                                            Operation.RealDuration = Retry.Elapsed;
                                            Operation.ResultedObject = Operation.OperationCode.Target;
                                            Operation.State = OperationStatus.Exception;
                                            OperationExecuted = true;
                                            Operation.Errors.Add(ex);
                                        }
                                        else
                                        {
                                            Operation.ResultedObject = Operation.OperationCode.Target;
                                            Operation.State = OperationStatus.Exception;
                                            OperationExecuted = true;
                                            Operation.Errors.Add(ex);
                                        }


                                    }
                                }
                                else
                                {
                                    Thread.Sleep(200);
                                }
                            }
                            NumberOfRunningOperation--;
                            System.Threading.Monitor.Exit(lo);
                        });
                    }
                }

            }
        }
    }

}
