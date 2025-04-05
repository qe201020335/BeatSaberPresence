using System;
using BeatSaberPresence.Config;
using Discord;
using UnityEngine;
using Zenject;

namespace BeatSaberPresence;

internal class MenuPresenceManager : MonoBehaviour, IInitializable, IDisposable
{
    private Activity? menuActivity;
    private PluginConfig pluginConfig;
    private PresenceController presenceController;

    protected void OnEnable()
    {
        if (presenceController != null)
        {
            menuActivity = RebuildActivity();
            Set();
        }
    }

    [Inject]
    internal void Construct(PluginConfig pluginConfig, PresenceController presenceController)
    {
        this.pluginConfig = pluginConfig;
        this.presenceController = presenceController;
        Set();
    }

    private void Set()
    {
        Debug.Log("BeatSaberPresence set menu presence");
        if (!menuActivity.HasValue) menuActivity = RebuildActivity();
        var activity = menuActivity.Value;
        var timestamps = menuActivity.Value.Timestamps;
        timestamps.Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        activity.Timestamps = timestamps;
        menuActivity = activity;
        presenceController.SetActivity(menuActivity.Value);
    }

    private Activity RebuildActivity()
    {
        var activity = new Activity
        {
            State = Format(pluginConfig.MenuBottomLine),
            Details = Format(pluginConfig.MenuTopLine)
        };
        if (pluginConfig.ShowTimes)
        {
            activity.Timestamps = new ActivityTimestamps
            {
                Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        if (pluginConfig.ShowImages)
        {
            activity.Assets = new ActivityAssets
            {
                LargeImage = "beat_saber_logo",
                LargeText = Format(pluginConfig.MenuLargeImageLine)
            };

            if (pluginConfig.ShowSmallImages)
            {
                activity.Assets.SmallImage = "beat_saber_block";
                activity.Assets.SmallText = Format(pluginConfig.MenuSmallImageLine);
            }
        }

        return activity;
    }

    private string Format(string rpcString)
    {
        var formattedString = rpcString;

        if (presenceController.User != null)
        {
            formattedString = formattedString.Replace("{DiscordName}", presenceController.User.Value.Username);
        }

        if (presenceController.User != null)
        {
            formattedString = formattedString.Replace("{DiscordDiscriminator}",
                presenceController.User.Value.Discriminator);
        }

        return formattedString;
    }

    #region Config Reloading

    public void Initialize()
    {
        pluginConfig.Reloaded += ConfigReloaded;
    }

    public void Dispose()
    {
        pluginConfig.Reloaded -= ConfigReloaded;
    }

    private void ConfigReloaded(PluginConfig _)
    {
        menuActivity = RebuildActivity();
    }

    #endregion
}