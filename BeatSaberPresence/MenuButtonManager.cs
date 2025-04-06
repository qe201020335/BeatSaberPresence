using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace BeatSaberPresence;

internal class MenuButtonManager : IInitializable
{
    private readonly MainFlowCoordinator mainFlowCoordinator;
    private readonly MenuButton menuButton;
    private readonly ModFlowCoordinator modFlowCoordinator;
    private readonly MenuButtons menuButtons;

    internal MenuButtonManager(ModFlowCoordinator modFlowCoordinator, MainFlowCoordinator mainFlowCoordinator, MenuButtons menuButtons)
    {
        this.modFlowCoordinator = modFlowCoordinator;
        this.mainFlowCoordinator = mainFlowCoordinator;
        this.menuButtons = menuButtons;
        menuButton = new MenuButton("BeatSaberPresence", "", SummonFlowCoordinator);
    }

    public void Initialize()
    {
        menuButtons.RegisterButton(menuButton);
    }

    private void SummonFlowCoordinator()
    {
        mainFlowCoordinator.PresentFlowCoordinator(modFlowCoordinator);
    }
}