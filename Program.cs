using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Orientation = System.Windows.Controls.Orientation;
using FontFamily = System.Windows.Media.FontFamily;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace RoundedEdge
{
    public sealed class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            var settingsWindow = new SettingsWindow();
            settingsWindow.Hide();
        }
    }

    public sealed class SettingsWindow : Window
    {
        private const int DefaultCornerRadius = 12;
        private const int DefaultOpacityPercent = 100;
        private const int MaxCornerRadius = 200;

        private readonly OverlayWindow overlayWindow;
        private readonly Slider roundnessSlider;
        private readonly Slider opacitySlider;
        private readonly TextBox roundnessInput;
        private readonly TextBox opacityInput;
        private readonly CheckBox startupCheck;
        private readonly Forms.NotifyIcon trayIcon;
        private bool exitRequested;

        public SettingsWindow()
        {
            Title = "Rounded Edge";
            Width = 500;
            Height = 560;
            MinWidth = 500;
            MinHeight = 560;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
            Background = UiResources.WindowBackground;
            FontFamily = new FontFamily("Poppins");
            FontWeight = FontWeights.Light;

            Resources.Add(typeof(Button), UiResources.RoundedButtonStyle);

            var rootGrid = new Grid
            {
                Background = Brushes.Transparent
            };

            var windowFrame = new Border
            {
                Background = UiResources.WindowBackground,
                CornerRadius = new CornerRadius(28),
                SnapsToDevicePixels = true,
                ClipToBounds = true,
                Child = rootGrid
            };

            roundnessInput = CreateValueTextBox(DefaultCornerRadius.ToString());
            opacityInput = CreateValueTextBox(DefaultOpacityPercent.ToString());

            roundnessSlider = CreateSlider(0, MaxCornerRadius, DefaultCornerRadius);
            opacitySlider = CreateSlider(0, 100, DefaultOpacityPercent);

            roundnessSlider.ValueChanged += OnRoundnessChanged;
            opacitySlider.ValueChanged += OnOpacityChanged;
            roundnessInput.LostFocus += OnRoundnessTextChanged;
            opacityInput.LostFocus += OnOpacityTextChanged;
            roundnessInput.KeyDown += OnValueInputKeyDown;
            opacityInput.KeyDown += OnValueInputKeyDown;

            var roundnessGrid = CreateControlGrid("Corner Radius", roundnessSlider, roundnessInput);
            var opacityGrid = CreateControlGrid("Opacity", opacitySlider, opacityInput);

            startupCheck = new CheckBox
            {
                Content = "Start with Windows",
                Foreground = UiResources.TextBrush,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 24),
                IsChecked = IsStartupEnabled(),
                VerticalAlignment = VerticalAlignment.Center
            };
            startupCheck.Checked += (s, e) => SetStartup(true);
            startupCheck.Unchecked += (s, e) => SetStartup(false);

            var titleBar = CreateTitleBar();
            var buttonPanel = CreateButtonPanel();

            var developerCredit = new TextBlock
            {
                Text = "Developed by F4r1z-fz",
                FontSize = 12,
                FontWeight = FontWeights.Light,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 32, 0, 0)
            };

            var mainContent = new StackPanel
            {
                Children =
                {
                    titleBar,
                    roundnessGrid,
                    opacityGrid,
                    startupCheck,
                    buttonPanel
                }
            };

            var mainPanel = new Border
            {
                Background = UiResources.CardBackground,
                CornerRadius = new CornerRadius(32),
                BorderBrush = UiResources.CardBorderBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(30),
                Margin = new Thickness(24),
                Child = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    Children =
                    {
                        mainContent,
                        developerCredit
                    }
                }
            };

            Grid.SetRow(mainContent, 0);
            Grid.SetRow(developerCredit, 1);

            rootGrid.Children.Add(mainPanel);
            Content = windowFrame;

            overlayWindow = new OverlayWindow();
            overlayWindow.Show();
            overlayWindow.UpdateSettings(DefaultCornerRadius, UiResources.PercentToAlpha(DefaultOpacityPercent));

            trayIcon = CreateTrayIcon();

            StateChanged += OnStateChanged;
            Closing += OnClosing;
            SourceInitialized += OnSourceInitialized;
        }

        private static TextBox CreateValueTextBox(string text) => new()
        {
            Text = text,
            Width = 72,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = UiResources.TextBoxBackground,
            BorderBrush = UiResources.TextBoxBorderBrush,
            Foreground = UiResources.TextBrush,
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Center,
            Padding = new Thickness(8),
            BorderThickness = new Thickness(1)
        };

        private static Slider CreateSlider(double min, double max, double value) => new()
        {
            Minimum = min,
            Maximum = max,
            Value = value,
            TickFrequency = 1,
            IsSnapToTickEnabled = false,
            Margin = new Thickness(0, 8, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Height = 28
        };

        private static Grid CreateControlGrid(string label, Slider slider, TextBox textBox)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var textBlock = new TextBlock
            {
                Text = label,
                FontSize = 15,
                FontWeight = FontWeights.Regular,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 8)
            };

            grid.Children.Add(textBlock);
            Grid.SetColumnSpan(textBlock, 3);

            grid.Children.Add(slider);
            Grid.SetRow(slider, 1);
            Grid.SetColumn(slider, 0);
            Grid.SetColumnSpan(slider, 2);

            grid.Children.Add(textBox);
            Grid.SetRow(textBox, 1);
            Grid.SetColumn(textBox, 2);

            return grid;
        }

        private Grid CreateTitleBar()
        {
            var titleText = new TextBlock
            {
                Text = "Rounded Edge",
                FontSize = 26,
                FontWeight = FontWeights.SemiBold,
                Foreground = UiResources.TitleBrush,
                VerticalAlignment = VerticalAlignment.Center
            };

            var minimizeButton = CreateTitleButton("—", UiResources.MinimizeButtonBackground);
            var closeButton = CreateTitleButton("✕", UiResources.CloseButtonBackground);

            var titleBar = new Grid
            {
                Margin = new Thickness(0, 0, 0, 24),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            titleBar.Children.Add(titleText);
            titleBar.Children.Add(minimizeButton);
            titleBar.Children.Add(closeButton);
            Grid.SetColumn(minimizeButton, 1);
            Grid.SetColumn(closeButton, 2);

            titleBar.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            closeButton.Click += (s, e) => ExitApplication();

            return titleBar;
        }

        private static Button CreateTitleButton(string content, Brush background) => new()
        {
            Content = content,
            Width = 34,
            Height = 34,
            Margin = new Thickness(0, 0, 8, 0),
            Background = background,
            Foreground = UiResources.TitleBrush,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.Bold,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Cursor = Cursors.Hand
        };

        private StackPanel CreateButtonPanel()
        {
            var exitButton = CreateActionButton("Exit", UiResources.DangerButtonBrush, Brushes.White);
            var resetButton = CreateActionButton("Reset", UiResources.LightButtonBrush, UiResources.TextBrush);
            var minimizeButton = CreateActionButton("Minimize", UiResources.PrimaryButtonBrush, Brushes.White);

            exitButton.Margin = new Thickness(12, 0, 12, 0);
            resetButton.Margin = new Thickness(0, 0, 12, 0);
            minimizeButton.Margin = new Thickness(0, 0, 12, 0);

            exitButton.Click += (s, e) => ExitApplication();
            resetButton.Click += (s, e) => ResetToDefaults();
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 32, 0, 0),
                Children = { exitButton, resetButton, minimizeButton }
            };
        }

        private static Button CreateActionButton(string text, Brush background, Brush foreground) => new()
        {
            Content = text,
            Width = 120,
            Height = 44,
            Margin = new Thickness(6, 0, 12, 0),
            Background = background,
            Foreground = foreground,
            Style = UiResources.RoundedButtonStyle
        };

        private void OnRoundnessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int radius = (int)e.NewValue;
            roundnessInput.Text = radius.ToString();
            overlayWindow.UpdateSettings(radius, overlayWindow.OverlayAlpha);
        }

        private void OnOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int percent = (int)e.NewValue;
            opacityInput.Text = percent.ToString();
            overlayWindow.UpdateSettings(overlayWindow.CornerRadius, UiResources.PercentToAlpha(percent));
        }

        private void OnRoundnessTextChanged(object sender, RoutedEventArgs? e)
        {
            if (int.TryParse(roundnessInput.Text, out var radius))
            {
                radius = Math.Clamp(radius, 0, MaxCornerRadius);
                roundnessSlider.Value = radius;
                overlayWindow.UpdateSettings(radius, overlayWindow.OverlayAlpha);
                roundnessInput.Text = radius.ToString();
            }
            else
            {
                roundnessInput.Text = ((int)roundnessSlider.Value).ToString();
            }
        }

        private void OnOpacityTextChanged(object sender, RoutedEventArgs? e)
        {
            if (int.TryParse(opacityInput.Text, out var percent))
            {
                percent = Math.Clamp(percent, 0, 100);
                opacitySlider.Value = percent;
                overlayWindow.UpdateSettings(overlayWindow.CornerRadius, UiResources.PercentToAlpha(percent));
                opacityInput.Text = percent.ToString();
            }
            else
            {
                opacityInput.Text = ((int)opacitySlider.Value).ToString();
            }
        }

        private static void OnValueInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void OnStateChanged(object? sender, EventArgs? e)
        {
            if (WindowState == WindowState.Minimized && !exitRequested)
            {
                Hide();
                trayIcon.Visible = true;
            }
        }

        private void OnSourceInitialized(object? sender, EventArgs? e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var region = CreateRoundRectRgn(0, 0, (int)Width, (int)Height, 36, 36);
            SetWindowRgn(hwnd, region, true);
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs? e)
        {
            if (!exitRequested)
            {
                if (e != null)
                {
                    e.Cancel = true;
                }

                Hide();
                WindowState = WindowState.Minimized;
                trayIcon.Visible = true;
            }
            else
            {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                }

                overlayWindow.Close();
                Application.Current.Shutdown();
            }
        }

        private void ShowWindowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ResetToDefaults()
        {
            roundnessSlider.Value = DefaultCornerRadius;
            opacitySlider.Value = DefaultOpacityPercent;
            roundnessInput.Text = DefaultCornerRadius.ToString();
            opacityInput.Text = DefaultOpacityPercent.ToString();
            overlayWindow.UpdateSettings(DefaultCornerRadius, UiResources.PercentToAlpha(DefaultOpacityPercent));
            startupCheck.IsChecked = IsStartupEnabled();
        }

        private bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                if (key == null)
                {
                    return false;
                }

                var value = key.GetValue("RoundedEdge") as string;
                return !string.IsNullOrEmpty(value);
            }
            catch
            {
                return false;
            }
        }

        private void SetStartup(bool enabled)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue("RoundedEdge", $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue("RoundedEdge", false);
                }
            }
            catch
            {
                // ignore startup setting failures
            }
        }

        private void ExitApplication()
        {
            exitRequested = true;
            Close();
        }

        private Forms.NotifyIcon CreateTrayIcon()
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var notifyIcon = new Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath),
                Text = "Rounded Edge",
                Visible = true
            };

            var trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("Show", null, (s, e) => ShowWindowFromTray());
            trayMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
            notifyIcon.ContextMenuStrip = trayMenu;
            notifyIcon.DoubleClick += (s, e) => ShowWindowFromTray();

            return notifyIcon;
        }
    }

    public sealed class OverlayWindow : Window
    {
        private int cornerRadius = 12;
        private byte overlayAlpha = 255;
        private readonly Path overlayPath;
        private readonly SolidColorBrush overlayBrush;

        public int CornerRadius => cornerRadius;
        public byte OverlayAlpha => overlayAlpha;

        public OverlayWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            overlayBrush = new SolidColorBrush(Color.FromArgb(overlayAlpha, 0, 0, 0));
            overlayPath = new Path
            {
                Fill = overlayBrush,
                IsHitTestVisible = false
            };

            Content = new Grid
            {
                Background = Brushes.Transparent,
                Children = { overlayPath }
            };

            Loaded += OnLoaded;
            SizeChanged += (s, e) => UpdateOverlay();
        }

        public void UpdateSettings(int radius, byte alpha)
        {
            cornerRadius = radius;
            overlayAlpha = alpha;
            overlayBrush.Color = Color.FromArgb(overlayAlpha, 0, 0, 0);
            UpdateOverlay();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).EnsureHandle();
            var style = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
            style = new IntPtr(style.ToInt64() | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, style);
            UpdateOverlay();
        }

        private void UpdateOverlay()
        {
            if (ActualWidth <= 0 || ActualHeight <= 0) return;

            overlayPath.Data = CreateOverlayGeometry(ActualWidth, ActualHeight, cornerRadius);
        }

        private static Geometry CreateOverlayGeometry(double width, double height, double radius)
        {
            var geometry = new CombinedGeometry
            {
                Geometry1 = new RectangleGeometry(new Rect(0, 0, width, height)),
                Geometry2 = new RectangleGeometry(new Rect(0, 0, width, height), radius, radius),
                GeometryCombineMode = GeometryCombineMode.Exclude
            };

            if (geometry.CanFreeze) geometry.Freeze();
            return geometry;
        }

        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const long WS_EX_TOPMOST = 0x00000008L;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }

    internal static class UiResources
    {
        public static readonly Brush TextBrush;
        public static readonly Brush TitleBrush;
        public static readonly Brush WindowBackground;
        public static readonly Brush CardBackground;
        public static readonly Brush CardBorderBrush;
        public static readonly Brush TextBoxBackground;
        public static readonly Brush TextBoxBorderBrush;
        public static readonly Brush PrimaryButtonBrush;
        public static readonly Brush DangerButtonBrush;
        public static readonly Brush LightButtonBrush;
        public static readonly Brush MinimizeButtonBackground;
        public static readonly Brush CloseButtonBackground;
        public static readonly Style RoundedButtonStyle;

        static UiResources()
        {
            TextBrush = new SolidColorBrush(Color.FromRgb(30, 33, 43));
            TitleBrush = new SolidColorBrush(Color.FromRgb(24, 28, 41));
            WindowBackground = LoadWindowBackground();
            CardBackground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255));
            CardBorderBrush = new SolidColorBrush(Color.FromRgb(225, 231, 241));
            TextBoxBackground = new SolidColorBrush(Color.FromRgb(249, 249, 251));
            TextBoxBorderBrush = new SolidColorBrush(Color.FromRgb(216, 221, 233));
            PrimaryButtonBrush = new SolidColorBrush(Color.FromRgb(84, 99, 243));
            DangerButtonBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            LightButtonBrush = new SolidColorBrush(Color.FromRgb(246, 247, 250));
            MinimizeButtonBackground = new SolidColorBrush(Color.FromArgb(20, 84, 99, 243));
            CloseButtonBackground = new SolidColorBrush(Color.FromArgb(20, 239, 68, 68));

            FreezeBrushes();

            RoundedButtonStyle = new Style(typeof(Button));
            RoundedButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, PrimaryButtonBrush));
            RoundedButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            RoundedButtonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            RoundedButtonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            RoundedButtonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(14, 10, 14, 10)));
            RoundedButtonStyle.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            RoundedButtonStyle.Setters.Add(new Setter(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            RoundedButtonStyle.Setters.Add(new Setter(Button.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            RoundedButtonStyle.Setters.Add(new Setter(Button.MinHeightProperty, 40.0));
            RoundedButtonStyle.Setters.Add(new Setter(Button.MinWidthProperty, 80.0));

            var buttonTemplate = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(14));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            border.SetValue(Border.SnapsToDevicePixelsProperty, true);

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
            presenter.SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(Button.HorizontalContentAlignmentProperty));
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(Button.VerticalContentAlignmentProperty));
            presenter.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Button.PaddingProperty));
            presenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
            border.AppendChild(presenter);

            buttonTemplate.VisualTree = border;
            RoundedButtonStyle.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));

            // Removed DropShadowEffect for the card to lower runtime memory usage.
        }

        private static void FreezeBrushes()
        {
            if (TextBrush.CanFreeze) TextBrush.Freeze();
            if (TitleBrush.CanFreeze) TitleBrush.Freeze();
            if (CardBackground.CanFreeze) CardBackground.Freeze();
            if (CardBorderBrush.CanFreeze) CardBorderBrush.Freeze();
            if (TextBoxBackground.CanFreeze) TextBoxBackground.Freeze();
            if (TextBoxBorderBrush.CanFreeze) TextBoxBorderBrush.Freeze();
            if (PrimaryButtonBrush.CanFreeze) PrimaryButtonBrush.Freeze();
            if (DangerButtonBrush.CanFreeze) DangerButtonBrush.Freeze();
            if (LightButtonBrush.CanFreeze) LightButtonBrush.Freeze();
            if (MinimizeButtonBackground.CanFreeze) MinimizeButtonBackground.Freeze();
            if (CloseButtonBackground.CanFreeze) CloseButtonBackground.Freeze();
        }

        private static Brush LoadWindowBackground()
        {
            try
            {
                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.UriSource = new Uri("pack://application:,,,/background.png", UriKind.Absolute);
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.EndInit();
                imageSource.Freeze();

                var brush = new ImageBrush(imageSource)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0.96
                };

                brush.Freeze();
                return brush;
            }
            catch
            {
                return new LinearGradientBrush(
                    Color.FromRgb(248, 249, 251),
                    Color.FromRgb(235, 239, 247),
                    new System.Windows.Point(0, 0),
                    new System.Windows.Point(1, 1));
            }
        }

        public static byte PercentToAlpha(int percent)
        {
            percent = Math.Clamp(percent, 0, 100);
            return (byte)(percent * 255 / 100);
        }
    }
}
