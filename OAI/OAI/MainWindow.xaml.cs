using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using OAI.Pages;

namespace OAI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static string consoleOutput = string.Empty;
    
    public MainWindow()
    {
        string pickedFilePath = null;
        InitializeComponent();
        SetCursor(InstallButton);
        SetCursor(MinimizeButton);
        SetCursor(CloseButton);

        CloseButton.Click += (sender, e) =>
        {
            KillAdbServer();
            Application.Current.MainWindow.Close();
        };
        MinimizeButton.Click += (sender, e) => WindowState = WindowState.Minimized;
        
        DragablePanel.Drop += (sender, e) =>
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            DragableText.Text = files[0].ToString();
            pickedFilePath = files[0].ToString();
        };
        DragablePanel.DragEnter += (sender, e) =>
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        };
        
        InstallButton.Click += async (sender, e) =>
        {
            await InstallApp(pickedFilePath);
            
            MainFrame.Content = null;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new NotificationPage(MainFrame));
        };
        
        var thread = new Thread(async () =>
        {
            while (true)
            {
                bool isConnected = await CheckAdbConnection();
                Dispatcher.Invoke(() =>
                {
                    if (isConnected)
                    {
                        ConnectionLabel.Content = "Connected";
                        ConnectionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#87b76c"));
                        InstallButton.IsEnabled = true;
                    }
                    else
                    {
                        ConnectionLabel.Content = "Disconnected";
                        ConnectionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ba1a1a"));
                        InstallButton.IsEnabled = false;
                    }
                });
                await Task.Delay(500);   
            }
        });
        thread.Start();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        
        this.DragMove();
    }
    
    void SetCursor(UIElement uiElement)
    {
        uiElement.MouseEnter += (sender, e) => Cursor = Cursors.Hand;
        uiElement.MouseLeave += (sender, e) => Cursor = Cursors.Arrow;
    }

    Task CallAdbCommand(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.Start();
            consoleOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
        return Task.CompletedTask;
    }

    async Task InstallApp(string path)
    {
        await CallAdbCommand($"adb install --bypass-low-target-sdk-block {path}");
    }
    
    async void KillAdbServer()
    {
        await CallAdbCommand("adb kill-server");
    }

    async Task<bool> CheckAdbConnection()
    {
        try
        {
            await CallAdbCommand("adb devices");
            StringReader st = new StringReader(consoleOutput);
            st.ReadLine();
            return st.ReadLine().Contains("device");
        }
        catch(Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            return false;
        }
    }
}