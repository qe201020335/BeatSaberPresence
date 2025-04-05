using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace BeatSaberPresence;

internal class ModFlowCoordinator : FlowCoordinator
{
    private MainFlowCoordinator mainFlowCoordinator;
    private Settings settings;

    [Inject]
    public void Construct(Settings settings, MainFlowCoordinator mainFlowCoordinator)
    {
        this.settings = settings;
        this.mainFlowCoordinator = mainFlowCoordinator;
    }

    protected override void DidActivate(bool firstActivation, bool _, bool __)
    {
        if (firstActivation)
        {
            SetTitle(Plugin.Name);
            showBackButton = true;
        }

        ProvideInitialViewControllers(settings);
    }

    protected override void BackButtonWasPressed(ViewController _)
    {
        mainFlowCoordinator.DismissFlowCoordinator(this);
    }
}