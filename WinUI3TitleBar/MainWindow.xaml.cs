using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3TitleBar
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		public AppWindow AW { get; set; }
		public bool IsCustomizationSupported { get; set; } = false;
		public string WindowTitle { get; set; }
		public MainWindow()
		{
			InitializeComponent();
			// By commenting the following line you can see the output in W10
			IsCustomizationSupported = AppWindowTitleBar.IsCustomizationSupported(); 

			ThemeBox.SelectedValue = Application.Current.RequestedTheme.ToString();

			if (IsCustomizationSupported)
			{
				AW = GetAppWindowForCurrentWindow();
				AW.TitleBar.ExtendsContentIntoTitleBar = true;
				CustomDragRegion.Height = AW.TitleBar.Height;
				WindowTitle = AW.Title = "Window Title";
				AW.Closing += AW_Closing;
				AW.SetIcon(Path.Combine(Package.Current.Installed­Location.Path, @"Assets/", @"StoreLogo.png"));
			}
			else
			{
				CustomDragRegion.BackgroundTransition = null;
				CustomDragRegion.Background = null;
				ExtendsContentIntoTitleBar = true;
				CustomDragRegion.Height = 28;
				SetTitleBar(CustomDragRegion);

				WindowTitle = Title = "Window Tilte";
			}

			UpdateColors(RootGrid.RequestedTheme);
			ThemeBox.SelectionChanged += ThemeBox_SelectionChanged;
		}

		private AppWindow GetAppWindowForCurrentWindow()
		{
			IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
			return AppWindow.GetFromWindowId(myWndId);
		}

		private async void AW_Closing(AppWindow sender, AppWindowClosingEventArgs args)
		{
			args.Cancel = true;
			bool canceled = await CloseRequested();
			if (!canceled)
				App.Current.Exit();
		}

		public async Task<bool> CloseRequested()
		{
			var save = new ContentDialog() { XamlRoot = RootGrid.XamlRoot, CloseButtonText = "Cancel", Title = "Do you want to save the open unsaved files before closing?", PrimaryButtonText = "Yes", SecondaryButtonText = "No", DefaultButton = ContentDialogButton.Primary };
			var result = await save.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				//await DoSaveTheFiles();
			}
			return result == ContentDialogResult.None;
		}

		private void UpdateColors(ElementTheme theme)
		{
			if (IsCustomizationSupported)
			{
				AW.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
				AW.TitleBar.ButtonBackgroundColor = Colors.Transparent;
				AW.TitleBar.ButtonHoverBackgroundColor = (Color)App.Current.Resources["SystemAccentColor"];
				AW.TitleBar.ButtonPressedBackgroundColor = theme == ElementTheme.Light ? (Color)App.Current.Resources["SystemAccentColorLight1"] : (Color)App.Current.Resources["SystemAccentColorDark1"];
				AW.TitleBar.ButtonForegroundColor = theme == ElementTheme.Light ? Colors.Black : Colors.White;
				AW.TitleBar.ButtonInactiveForegroundColor = theme == ElementTheme.Light ? Color.FromArgb(255, 50, 50, 50) : Color.FromArgb(255, 200, 200, 200);
			}
		}

		private async void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RootGrid.RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), (string)e.AddedItems[0]);
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			localSettings.Values["Theme"] = RootGrid.RequestedTheme.ToString();
			UpdateColors(RootGrid.RequestedTheme);

			if (!IsCustomizationSupported)
			{
				var cd = new ContentDialog() { XamlRoot = RootGrid.XamlRoot, Title = "You need to restart the app.", PrimaryButtonText = "OK", DefaultButton = ContentDialogButton.Primary };
				await cd.ShowAsync();
			}
		}
	}
}
