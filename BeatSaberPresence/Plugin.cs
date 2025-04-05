using BeatSaberPresence.Config;
using BeatSaberPresence.Installers;
using IPA;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace BeatSaberPresence;

[Plugin(RuntimeOptions.DynamicInit)]
public class Plugin
{
    internal const string Name = "BeatSaberPresence";

    [Init]
    public Plugin(Conf conf, IPALogger logger, Zenjector zenjector)
    {
        zenjector.UseLogger(logger);
        zenjector.Install<PluginInstaller>(Location.App, conf.Generated<PluginConfig>());
        zenjector.Install<PresenceMenuInstaller>(Location.Menu);
        zenjector.Expose<CoreGameHUDController>("Gameplay");
        zenjector.Install<PresenceGameInstaller>(Location.StandardPlayer);
        zenjector.Install<PresenceGameInstaller>(Location.CampaignPlayer);
        zenjector.Install<PresenceTutorialInstaller>(Location.Tutorial);
        zenjector.Install<PresenceMultiplayerInstaller>(Location.MultiPlayer);
    }

    [OnEnable]
    [OnDisable]
    public void OnState()
    {
        /* We don't need this, but BSIPA logs a warning if there is nothing. */
    }
}