using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Runtime.Plugins
{
    internal class Queue
    {
        private readonly IIterationService _its;
        private readonly IChatMessageService _chat;
        public Queue(IIterationService its, IChatMessageService chat)
        {
            _its = its;
            _chat = chat;
        }

        [KernelFunction("pause")]
        [Description("Immediately pauses the prompt queue. While paused, queued instructions will not be automatically sent after your response completes. " +
            "The user must manually resume the queue or send a reply before execution continues. " +
            "Use this when you need critical clarification from the user before proceeding with remaining queued tasks.")]
        public ToolResult PauseQueue()
        {
            _its.PauseQueue(true);
            _chat.LogInfo("Model paused prompt queue");
            return new ToolResult(true, "Prompt queue paused");
        }

        [KernelFunction("count")]
        [Description("Returns the number of instructions currently queued for execution. " +
            "Use this to determine if there are upcoming queued tasks that may be affected by your response, " +
            "or to help decide whether to pause the queue for critical user input.")]
        public ToolResult Count()
        {
            return new ToolResult(true, $"{_its.QueueCount} queued instructions");
        }
    }
}
