using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Models
{
    internal sealed class InputQueue
    {
        private readonly List<QueuedInput> _queue;
        private readonly object _lock;

        public InputQueue()
        {
            _queue = new List<QueuedInput>();
            _lock = new object();
        }

        private bool _paused { get; set; }
        public bool Paused => _paused;

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public IReadOnlyList<QueuedInput> GetSnapshot()
        {
            lock (_lock)
            {
                return _queue.ToList();
            }
        }

        public void Enqueue(ChatMessageContent content)
        {
            lock (_lock)
            {
                _queue.Add(new QueuedInput(content));
            }
        }

        public void EnqueueAt(int index, ChatMessageContent content)
        {
            lock (_lock)
            {
                _queue.Insert(index, new QueuedInput(content));
            }
        }

        public bool Remove(QueuedInput? instance)
        {
            if (instance is null)
                return false;

            lock (_lock)
            {
                return _queue.Remove(instance);
            }
        }

        public bool Remove(string id)
        {
            lock (_lock)
            {
                var instance = _queue.FirstOrDefault(x => x.Id == id);
                if (instance == null)
                    return false;
                return _queue.Remove(instance);
            }
        }

        public QueuedInput? Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                    return null;

                var input = _queue[0];
                _queue.RemoveAt(0);
                return input;
            }
        }

        public QueuedInput? Peek()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                    return null;

                return _queue[0];
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                _paused = true;
            }
        }

        public void Continue()
        {
            lock (_lock)
            {
                _paused = false;
            }
        }
    }
}
