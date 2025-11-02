// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Randomization;
using Nalix.Launcher.Objects.Notifications;
using Nalix.Launcher.Scenes.Menu.Main.Model;
using Nalix.Launcher.Scenes.Menu.Main.View;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Runtime;

namespace Nalix.Launcher.Scenes.Menu.Main.Controller;

internal sealed class MainSceneController(
    MainMenuModel model,
    ISceneNavigator nav,
    ISfxPlayer sfx,
    IParallaxFactory parallaxFactory,
    IUiTheme theme)
{
    private readonly IParallaxFactory _parallaxFactory = parallaxFactory ?? throw new System.ArgumentNullException(nameof(parallaxFactory));
    private readonly IUiTheme _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
    private readonly MainMenuModel _model = model ?? throw new System.ArgumentNullException(nameof(model));
    private readonly ISceneNavigator _nav = nav ?? throw new System.ArgumentNullException(nameof(nav));
    private readonly ISfxPlayer _sfx = sfx ?? throw new System.ArgumentNullException(nameof(sfx));

    // Lắp ráp MVC vào scene (composition root)
    public void Compose(MainScene scene)
    {
        // View: hiệu ứng mở đầu
        scene.AddObject(new RectRevealEffectView(_theme));

        // View: logo
        scene.AddObject(new LauncherLogoView(_theme));

        // View: icon 12+
        scene.AddObject(new TwelveIconView(_theme));

        // Model parallax + view
        System.Int32 variant = SecureRandom.GetInt32(1, 4); // giữ nguyên tính ngẫu nhiên
        var parallaxModel = _parallaxFactory.Create(GraphicsEngine.ScreenSize, variant);
        scene.AddObject(new ParallaxLayerView(_theme, parallaxModel));

        // View: menu
        var menu = new MainMenuView(_theme);
        WireMenu(menu);
        scene.AddObject(menu);

        // View: banner cuộn (giữ nguyên text/tốc độ cũ)
        scene.AddObject(new ScrollingBanner("⚠ Chơi quá 180 phút mỗi ngày sẽ ảnh hưởng xấu đến sức khỏe ⚠", 200f));
    }

    // Gắn sự kiện từ View -> hành vi (điều hướng + âm thanh)
    private void WireMenu(MainMenuView menu)
    {
        menu.LoginRequested += () =>
        {
            if (_model.IsBusy)
            {
                return;
            }

            _sfx.Play("1");
            _nav.Change(SceneNames.Login);
        };

        menu.RegisterRequested += () =>
        {
            if (_model.IsBusy)
            {
                return;
            }

            _sfx.Play("1");
            _nav.Change(SceneNames.Register);
        };

        menu.NewsRequested += () =>
        {
            if (_model.IsBusy)
            {
                return;
            }

            _sfx.Play("1");
            _nav.Change(SceneNames.News);
        };

        menu.ExitRequested += () =>
        {
            if (_model.IsBusy)
            {
                return;
            }

            _sfx.Play("1");
            _nav.CloseWindow();
        };
    }
}
