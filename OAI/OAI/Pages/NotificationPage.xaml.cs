using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OAI.Pages;

public partial class NotificationPage : Page
{
    public NotificationPage(Frame mainFrame)
    {
        InitializeComponent();
        SetCursor(OkButton);
        OkButton.Click += (sender, e) =>
        {
            mainFrame.Visibility = Visibility.Collapsed;
        };
    }
    
    void SetCursor(UIElement uiElement)
    {
        uiElement.MouseEnter += (sender, e) => Cursor = Cursors.Hand;
        uiElement.MouseLeave += (sender, e) => Cursor = Cursors.Arrow;
    }

}