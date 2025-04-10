﻿using BeatSaberPresence.Config;
using Zenject;

namespace BeatSaberPresence.Installers;

internal class PluginInstaller : Installer<PluginConfig, PluginInstaller>
{
    private readonly PluginConfig pluginConfig;

    internal PluginInstaller(PluginConfig pluginConfig)
    {
        this.pluginConfig = pluginConfig;
    }

    public override void InstallBindings()
    {
        Container.BindInstance(pluginConfig);
        Container.BindInterfacesAndSelfTo<PresenceController>().AsSingle();
    }
}