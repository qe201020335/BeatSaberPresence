using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace BeatSaberPresence;

internal class MenuButtonManager : IInitializable, IDisposable
{
    private readonly MainFlowCoordinator mainFlowCoordinator;
    private readonly MenuButton menuButton;
    private readonly ModFlowCoordinator modFlowCoordinator;

    internal MenuButtonManager(ModFlowCoordinator modFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
    {
        this.modFlowCoordinator = modFlowCoordinator;
        this.mainFlowCoordinator = mainFlowCoordinator;
        menuButton = new MenuButton("BeatSaberPresence", "", SummonFlowCoordinator);
    }

    public void Dispose()
    {
        if (MenuButtons.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
        {
            MenuButtons.instance.UnregisterButton(menuButton);
        }
    }

    public void Initialize()
    {
        MenuButtons.instance.RegisterButton(menuButton);
    }

    private void SummonFlowCoordinator()
    {
        mainFlowCoordinator.PresentFlowCoordinator(modFlowCoordinator);
    }
}