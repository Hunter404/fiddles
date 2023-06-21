// Copyright (c) Fredrik Larsson. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Threading;

// Console program for simulating multiple threads solving tasks where they occasionally have to merge or run in series
// Which order executed does not matter
namespace ParallelTest
{
    public enum TaskType
    {
        Async,
        SyncOnce,
        SyncAll,
    }
    
    public class Task
    {
        public string Name;
        
        public TaskType Type;

        public Task(string name, TaskType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class Instance
    {
        public int Index;

        public Task[] Sequence;

        public Thread Thread;

        public bool Running = true;

        public void Execute(Task task)
        {
            Thread.Sleep(100);
            Console.Write($" => {Index}:{task.Name}");
        }

        public void Run()
        {
            foreach (var item in Sequence)
            {
                if (item.Type != TaskType.Async)
                {
                    Monitor.Enter(item);
                    Program.Reset.Reset();
                }
                
                if (item.Type != TaskType.SyncOnce)
                {
                    Execute(item);
                }

                if (item.Type != TaskType.Async)
                {
                    Running = false;

                    if (Program.Instances.Any(x => x.Running))
                    {
                        Monitor.Exit(item);
                        Console.Write($" => {Index}!");
                        Program.Reset.WaitOne();
                    }
                    else
                    {
                        if (item.Type == TaskType.SyncOnce)
                        {
                            Execute(item);
                        }
                        
                        Console.Write($" => {Index}>");
                        Program.Reset.Set();
                        Monitor.Exit(item);
                    }

                    Running = true;
                }
            }
        }
    }
    
    internal class Program
    {
        public static ManualResetEvent Reset;

        public static Instance[] Instances;
        
        public static void Main(string[] args)
        {
            Reset = new ManualResetEvent(false);
            
            var sequence = new []
            {
                new Task("A", TaskType.Async),
                new Task("B", TaskType.Async),
                new Task("C", TaskType.SyncAll),
                new Task("D", TaskType.Async),
                new Task("E", TaskType.Async),
                new Task("F", TaskType.SyncOnce),
                new Task("G", TaskType.Async),
            };

            Instances = new []
            {
                new Instance {Index = 1, Sequence = sequence},
                new Instance {Index = 2, Sequence = sequence},
                new Instance {Index = 3, Sequence = sequence},
                // new Instance {Index = 4, Sequence = sequence},
            };

            foreach (var instance in Instances)
            {
                instance.Thread = new Thread(instance.Run);
                instance.Thread.Start();

                Thread.Sleep(100);
            }

            foreach (var instance in Instances)
            {
                instance.Thread.Join();
            }

            Reset.Dispose();
        }
    }
}
