using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.Storage.File;
using HAgent.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace HWorld.HAgent
{
    /// <summary>
    /// Opens the HAgent configuration UI using HWorld's explicitly registered
    /// external HAgent services and provider adapters.
    /// </summary>
    public static class HAgentConfiguration
    {
        public static void Show(IWin32Window owner = null)
        {
            var options = new HAgentStorageOptions
            {
                ApplicationName = Process.GetCurrentProcess().ProcessName,
                RootPath = AppContext.BaseDirectory
            };

            options.Validate();

            var basePath = options.GetEffectiveRootPath();
            Directory.CreateDirectory(basePath);

            IAiStore store = new FileAiStore(
                Path.Combine(basePath, "configuration", "settings.json"));
            ISecretStore secrets = new ProtectedDataSecretStore(
                Path.Combine(basePath, "secrets"));
            IToolStore toolStore = new FileToolStore(
                Path.Combine(basePath, "configuration", "tools", "tools.json"));

            var adapters = new List<IAiProviderAdapter>
            {
                new OpenAICompatibleProviderAdapter()
            };

            AISettings.ShowMainAISettingsForm(
                store,
                secrets,
                owner,
                adapters,
                toolStore);
        }
    }
}
