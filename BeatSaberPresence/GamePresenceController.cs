using System;
using System.Globalization;
using System.Linq;
using BeatSaberPresence.Config;
using Discord;
using SiraUtil.Submissions;
using Zenject;

namespace BeatSaberPresence;

internal class GamePresenceManager : IInitializable, ILateDisposable
{
    private readonly AudioTimeSyncController audioTimeSyncController;

    private readonly IGamePause gamePause;
    private readonly GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
    private readonly PluginConfig pluginConfig;
    private readonly PresenceController presenceController;
    private readonly Submission submission;
    private Activity? gameActivity;
    private Activity? pauseActivity;

    internal GamePresenceManager([InjectOptional] IGamePause gamePause, [InjectOptional] Submission submission,
        PluginConfig pluginConfig, PresenceController presenceController,
        AudioTimeSyncController audioTimeSyncController, GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
    {
        this.gamePause = gamePause;
        this.submission = submission;
        this.pluginConfig = pluginConfig;
        this.presenceController = presenceController;
        this.audioTimeSyncController = audioTimeSyncController;
        this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
    }

    public void Initialize()
    {
        if (gamePause != null)
        {
            gamePause.didPauseEvent += DidPause;
            gamePause.didResumeEvent += DidResume;
        }

        pluginConfig.Reloaded += ConfigReloaded;
        DidResume();
    }

    public void LateDispose()
    {
        if (gamePause != null)
        {
            gamePause.didPauseEvent -= DidPause;
            gamePause.didResumeEvent -= DidResume;
        }

        pluginConfig.Reloaded -= ConfigReloaded;
    }

    private void DidPause()
    {
        Set(true);
    }

    private void DidResume()
    {
        Set();
    }

    private void ConfigReloaded(PluginConfig _)
    {
        gameActivity = RebuildActivity();
        pauseActivity = RebuildActivity(true);
        Set();
    }

    private void Set(bool isPaused = false)
    {
        if (isPaused)
        {
            if (!pauseActivity.HasValue) pauseActivity = RebuildActivity(true);

            presenceController.SetActivity(pauseActivity.Value);
        }
        else
        {
            if (!gameActivity.HasValue) gameActivity = RebuildActivity();

            if (pluginConfig.InGameCountDown)
            {
                var activity = gameActivity.Value;
                var timestamps = gameActivity.Value.Timestamps;
                timestamps.End = DateTimeOffset.UtcNow
                    .AddSeconds(audioTimeSyncController.songLength - audioTimeSyncController.songTime)
                    .ToUnixTimeMilliseconds();
                activity.Timestamps = timestamps;
                gameActivity = activity;
            }

            presenceController.SetActivity(gameActivity.Value);
        }
    }

    private Activity RebuildActivity(bool paused = false)
    {
        var activity = new Activity
        {
            Details = Format(paused ? pluginConfig.PauseTopLine : pluginConfig.GameTopLine),
            State = Format(paused ? pluginConfig.PauseBottomLine : pluginConfig.GameBottomLine)
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
                LargeText = Format(paused ? pluginConfig.PauseLargeImageLine : pluginConfig.GameLargeImageLine)
            };

            if (pluginConfig.ShowSmallImages)
            {
                activity.Assets.SmallImage = "beat_saber_block";
                activity.Assets.SmallText =
                    Format(paused ? pluginConfig.PauseSmallImageLine : pluginConfig.GameSmallImageLine);
            }
        }

        return activity;
    }

    private string Format(string rpcString)
    {
        var result = rpcString;

        if (presenceController.User != null)
        {
            result = result.Replace("{DiscordName}", presenceController.User.Value.Username);
        }

        if (presenceController.User != null)
        {
            result = result.Replace("{DiscordDiscriminator}",
                presenceController.User.Value.Discriminator);
        }

        var beatmapKey = gameplayCoreSceneSetupData.beatmapKey;
        var modifiers = gameplayCoreSceneSetupData.gameplayModifiers;
        var level = gameplayCoreSceneSetupData.beatmapLevel;
        var audioClip = gameplayCoreSceneSetupData.songAudioClip;

        var totalTime = new TimeSpan(0, 0, (int)Math.Floor(audioClip.length));

        result = result.Replace("{SongName}", level.songName);
        result = result.Replace("{SongSubName}", level.songSubName);
        result = result.Replace("{SongAuthorName}", level.songAuthorName);
        result = result.Replace("{SongDuration}", totalTime.ToString(@"mm\:ss"));
        result = result.Replace("{SongDurationSeconds}", totalTime.ToString());
        result = result.Replace("{LevelAuthorName}", level.allMappers.FirstOrDefault() ?? level.allLighters.FirstOrDefault() ?? "Unknown");
        result = result.Replace("{Difficulty}", beatmapKey.difficulty.Name());
        result = result.Replace("{SongBPM}", level.beatsPerMinute.ToString(CultureInfo.CurrentCulture));
        result = result.Replace("{LevelID}", level.levelID);
        result = result.Replace("{EnvironmentName}", gameplayCoreSceneSetupData.targetEnvironmentInfo.environmentName);
        result = result.Replace("{Submission}", submission != null ? submission.Tickets().Length == 0 ? "Disabled" : "Enabled" : "Disabled");


        result = result.Replace("{NoFail}", modifiers.noFailOn0Energy ? "On" : "Off");
        result = result.Replace("{NoBombs}", modifiers.noBombs ? "On" : "Off");
        result = result.Replace("{NoObsticles}", modifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles ? "On" : "Off");
        result = result.Replace("{NoArrows}", modifiers.noArrows ? "On" : "Off");
        result = result.Replace("{SlowerSong}", modifiers.songSpeed == GameplayModifiers.SongSpeed.Slower ? "On" : "Off");
        result = result.Replace("{InstaFail}", modifiers.instaFail ? "On" : "Off");
        result = result.Replace("{BatteryEnergy}", modifiers.energyType == GameplayModifiers.EnergyType.Battery ? "On" : "Off");
        result = result.Replace("{GhostNotes}", modifiers.ghostNotes ? "On" : "Off");
        result = result.Replace("{DisappearingArrows}", modifiers.disappearingArrows ? "On" : "Off");
        result = result.Replace("{FasterSong}", modifiers.songSpeed == GameplayModifiers.SongSpeed.Faster ? "On" : "Off");

        return result;
    }
}