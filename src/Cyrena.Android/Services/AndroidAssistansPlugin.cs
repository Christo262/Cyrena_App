using Cyrena.Android.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;

namespace Cyrena.Android.Services
{
    internal class AndroidAssistansPlugin : IAssistantPlugin
    {
        public string Id => "android.defaults";
        public string[] Modes => [];
        public int Priority => 4;
        public bool Required => true;
        public string Title => "Android Services";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.GetFeatureOption<InterfaceOverrides>().UseFileAttacher<MauiFileUpload>();
            return Task.CompletedTask;
        }
    }
}
