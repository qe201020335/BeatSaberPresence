using System;
using BeatSaberPresence.Config;
using Discord;
using DiscordCore;
using SiraUtil.Logging;
using Zenject;

namespace BeatSaberPresence;

internal class PresenceController : IInitializable, IDisposable
{
    internal const long clientID = 708741346403287113;

    private readonly bool didInstantiateUserManagerProperly = true;
    private readonly DiscordInstance discordInstance;
    private readonly PluginConfig pluginConfig;
    private readonly SiraLog siraLog;
    private readonly UserManager userManager;

    internal PresenceController(SiraLog siraLog, PluginConfig pluginConfig)
    {
        this.siraLog = siraLog;
        this.pluginConfig = pluginConfig;
        discordInstance = DiscordManager.instance.CreateInstance(new DiscordSettings
        {
            appId = clientID,
            handleInvites = false,
            modId = nameof(BeatSaberPresence),
            modName = nameof(BeatSaberPresence)
        });
        userManager = DiscordClient.GetUserManager();

        if (userManager == null) didInstantiateUserManagerProperly = false;
    }

    public User? User { get; private set; }

    public void Dispose()
    {
        if (DiscordManager.IsSingletonAvailable && DiscordCore.UI.Settings.IsSingletonAvailable)
        {
            discordInstance.DestroyInstance();
        }

        if (didInstantiateUserManagerProperly) userManager.OnCurrentUserUpdate -= CurrentUserUpdated;
    }

    public void Initialize()
    {
        siraLog.Debug("Initializing Presence Controller");
        if (didInstantiateUserManagerProperly) userManager.OnCurrentUserUpdate += CurrentUserUpdated;
    }

    private void CurrentUserUpdated()
    {
        if (didInstantiateUserManagerProperly) User = userManager.GetCurrentUser();
    }

    internal void SetActivity(Activity activity)
    {
        discordInstance.ClearActivity();
        if (!pluginConfig.Enabled) return;
        discordInstance.UpdateActivity(activity);
    }

    internal void ClearActivity()
    {
        discordInstance.ClearActivity();
    }
}