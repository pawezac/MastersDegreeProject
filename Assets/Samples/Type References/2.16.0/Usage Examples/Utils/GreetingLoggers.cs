﻿namespace TypeReferences.Demo.Utils
{
    using UnityEngine;

    public interface IGreetingLogger
    {
        void LogGreeting();
    }

    public class DefaultGreetingLogger : IGreetingLogger
    {
        public void LogGreeting()
        {
            Debug.Log("Hello, World!");
        }
    }

    public class AnotherGreetingLogger : IGreetingLogger
    {
        public void LogGreeting()
        {
            Debug.Log("Greetings!");
        }
    }

    internal class InternalGreetingLogger : IGreetingLogger
    {
        public void LogGreeting()
        {
            Debug.Log("I am internal.");
        }
    }

    public abstract class AbstractGreetingLogger : IGreetingLogger
    {
        public abstract void LogGreeting();
    }

    public interface ILoggerChildInterface : IGreetingLogger
    {
        void AdditionalMethod();
    }
}
