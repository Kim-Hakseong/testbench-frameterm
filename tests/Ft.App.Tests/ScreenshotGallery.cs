using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Ft.Core.Project;
using Ft.App.ViewModels;
using Ft.App.Views;
using Xunit;

namespace Ft.App.Tests;

/// <summary>
/// 소개용 추가 스크린샷. <see cref="ScreenshotCapture"/> 가 데모 모드가 도는 메인
/// 화면을 담당하고, 여기서는 그 한 장으로는 보이지 않는 것을 찍는다.
///
/// 프레임 정의 대화상자가 첫 번째다. "프레임을 한 번 정의하면 나머지는 알아서
/// 파싱된다"가 이 앱의 존재 이유인데, 메인 화면에는 그 결과만 보이고 정의하는
/// 화면은 안 보인다. 데모 프로토콜의 실제 설정을 그대로 띄운다.
/// </summary>
public sealed class ScreenshotGallery
{
    private static async Task SettleAsync(int passes = 5)
    {
        for (var i = 0; i < passes; i++)
        {
            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Save(Window window, string path)
    {
        var bitmap = window.CaptureRenderedFrame();
        Assert.NotNull(bitmap);
        using var stream = File.Create(path);
        bitmap!.Save(stream);
    }

    [AvaloniaFact]
    public async Task CaptureFrameDefinitionScreenshot()
    {
        // 데모 프로토콜의 실제 정의 — 프레이밍, 체크섬, 필드, 하이라이트가 모두
        // 채워진 상태라 빈 폼이 아니라 쓰이고 있는 화면이 찍힌다.
        var project = MainWindowViewModel.DemoProject();
        Assert.NotEmpty(project.Fields);

        var dialog = new FrameDefinitionDialog(project);
        dialog.Show();
        await SettleAsync();

        Save(dialog, "/tmp/ft-frame-definition.png");
        dialog.Close();
    }

    [AvaloniaFact]
    public async Task CaptureMacroDialogScreenshot()
    {
        // 데모 프로토콜에는 매크로가 없어서 빈 표가 찍힌다. 빈 화면은 아무것도
        // 설명하지 못하므로, 이 대화상자가 실제로 무엇을 담는지 — 전송 시점에
        // 계산되는 {len}/{crc16} 플레이스홀더 — 가 보이도록 채운다. 사용자가
        // 직접 입력했을 값과 같은 것이고, 어느 것도 앱 동작을 바꾸지 않는다.
        var project = MainWindowViewModel.DemoProject();
        project.Macros.Add(new MacroConfig
        {
            Name = "Poll status",
            Text = "A5 01 {len} \"STAT\" {crc16}",
            Hotkey = "F1",
        });
        project.Macros.Add(new MacroConfig
        {
            Name = "Read temp",
            Text = "A5 02 {len} \"TEMP\" {crc16}",
            Hotkey = "F2",
        });
        project.Macros.Add(new MacroConfig
        {
            Name = "Reset counters",
            Text = "A5 10 {len} 00 00 {crc16}",
            Hotkey = "F3",
        });

        var dialog = new MacroDialog(project);
        dialog.Show();
        await SettleAsync();

        Save(dialog, "/tmp/ft-macros.png");
        dialog.Close();
    }
}
