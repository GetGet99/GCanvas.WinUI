// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Windows.UI;
using WinRT.Interop;
using Cursor = WinWrapper.Cursor;
using Win32Window = WinWrapper.Window;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIInkCanvas
{
    public partial class PenProperty : ObservableObject
    {
        
        Color color;
        public Color Color
        {
            get => color;
            set
            {
                SetProperty(ref color, value);
            }
        }

        double penSize;
        public double PenSize
        {
            get => penSize;
            set
            {
                SetProperty(ref penSize, value);
            }
        }

        bool isHighlighter;
        public bool IsHighlighter
        {
            get => isHighlighter;
            set
            {
                SetProperty(ref isHighlighter, value);
            }
        }
    }
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        Symbol
            Move = (Symbol)0xe7c2,
            MouseIcon = (Symbol)0xE962,
            StrokeErase = (Symbol)0xed60,
            PointErase = (Symbol)0xed61,
            ClearAllInk = (Symbol)0xed62,
            Pencil = (Symbol)0xed63,
            Marker = (Symbol)0xed64,
            Ruler = (Symbol)0xed5e,
            Protractor = (Symbol)0xf0b4,
            FingerInking = (Symbol)0xed5f,
            HighlightSymbol = (Symbol)0xe7e6,
            CloseSymbol = (Symbol)0xe8bb;
        
        PenProperty selected;
        PenProperty Selected
        {
            get => selected;
            set
            {
                selected = value;
                PropertyChanged?.Invoke(this, new(nameof(Selected)));
            }
        }

        public PenProperty Pen = new() { Color = Colors.Red, PenSize = 10 };
        public PenProperty Pen2 = new() { Color = Colors.Blue, PenSize = 10 };
        public PenProperty Highlight = new() { Color = Colors.Yellow, PenSize = 30, IsHighlighter = true };
        public PenProperty Highlight2 = new() { Color = Colors.Aqua, PenSize = 30, IsHighlighter = true };

        private void OpenOptions(object sender, RoutedEventArgs e)
        {
            var appWindow = AppWindow.GetFromWindowId(new((ulong)WindowNative.GetWindowHandle(OptionsWindow)));
            var bounds = System.Windows.Forms.Screen.FromHandle(WindowEx.Handle.Value).Bounds;
            appWindow.MoveAndResize(new(Math.Clamp(Cursor.Position.X, 0, bounds.Width - 350), Math.Clamp(Cursor.Position.Y, 0, bounds.Height - 565), 350, 565));
            OptionsWindow.Activate();
        }


        System.Drawing.Point prev;


        private void MoveWindowPressed(object sender, PointerRoutedEventArgs e)
        {
            prev = Cursor.Position;
        }

        private void MoveWindowMouseMove(object sender, PointerRoutedEventArgs e)
        {
            var next = Cursor.Position;

            if (!e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed) goto End;
            var bounds = WindowEx.Bounds;
            WindowEx.Bounds = bounds with
            {
                X = bounds.X + next.X - prev.X,
                Y = bounds.Y + next.Y - prev.Y
            };
        End:
            prev = next;
        }

        private void MarkerChecked(object sender, RoutedEventArgs e)
        {
            Selected = Pen;
        }
        private void Marker2Checked(object sender, RoutedEventArgs e)
        {
            Selected = Pen2;
        }

        private void HighlightChecked(object sender, RoutedEventArgs e)
        {
            Selected = Highlight;
        }
        private void Highlight2Checked(object sender, RoutedEventArgs e)
        {
            Selected = Highlight2;
        }

        private void ClearAllInkClicked(object sender, RoutedEventArgs e)
        {
            InkingWindow.ClearAllInk();
        }

        public const int WS_EX_TRANSPARENT = 0x20;
        const int dpi = 96;
        Win32Window WindowEx;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Selected = Pen;
            InitializeComponent();
            MouseButton.IsChecked = true;
            WindowEx = Win32Window.FromWindowHandle(WindowNative.GetWindowHandle(this));


            //Thread t = new(() =>
            //{
            //    System.Windows.Forms.Application.Run(InkingWindow.CompositionWindow.Form);
            //});
            //t.SetApartmentState(ApartmentState.STA);
            //t.Start();

            WindowEx.Owner = Win32Window.FromWindowHandle(InkingWindow.CompositionWindow.Handle);
            InkingWindow.CompositionWindow.Form.TopMost = true;

            var appWindow = AppWindow.GetFromWindowId(new((ulong)WindowEx.Handle.Value));
            var Presenter = (OverlappedPresenter)appWindow.Presenter;
            Presenter.IsMaximizable = Presenter.IsMinimizable = Presenter.IsResizable = false;
            Presenter.IsAlwaysOnTop = true;
            Presenter.SetBorderAndTitleBar(true, false);
            appWindow.Resize(new(504, 48));
            {
                var optionsappWindow = AppWindow.GetFromWindowId(new((ulong)WindowNative.GetWindowHandle(OptionsWindow)));
                var presenter = (OverlappedPresenter)optionsappWindow.Presenter;
                presenter.IsMaximizable = false;
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.SetBorderAndTitleBar(true, false);
                presenter.IsAlwaysOnTop = true;
                optionsappWindow.Closing += (o, e) =>
                {
                    e.Cancel = true;
                    optionsappWindow.Hide();
                };
                OptionsWindow.Activated += (o, e) =>
                {
                    if (e.WindowActivationState == WindowActivationState.Deactivated) optionsappWindow.Hide();
                };
            }
        }
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            InkingWindow.Invoke(() => InkingWindow.IsHitTestVisible = false);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            InkingWindow.Invoke(() => InkingWindow.IsHitTestVisible = true);
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
