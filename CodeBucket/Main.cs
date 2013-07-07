using System.Linq;
using CodeBucket.Bitbucket.Controllers.Accounts;
using CodeBucket.Controllers;
using CodeBucket.Data;
using CodeFramework.Elements;
using CodeFramework.Views;
using CodeFramework.Controllers;
using CodeFramework.Cells;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CodeBucket
{
	
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow _window;
        SlideoutNavigationController _nav;

		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}
		
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			//Set the theme
			SetTheme();

			//Create the window
            _window = new UIWindow(UIScreen.MainScreen.Bounds);

            //Process the accounts
            ProcessAccounts();
            
			//Make what ever window visible.
			_window.MakeKeyAndVisible();

			//Fade the splash screen
			BeginSplashFade();

			//Always return true
			return true;
		}


		/// <summary>
		/// Sets the theme of the application.
		/// </summary>
		private void SetTheme()
		{
			//Set the status bar
			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.BlackOpaque, false);
			
			//Set the theming
			UINavigationBar.Appearance.SetBackgroundImage(Images.Titlebar.CreateResizableImage(new UIEdgeInsets(0, 0, 1, 0)), UIBarMetrics.Default);
			
			UIBarButtonItem.Appearance.SetBackgroundImage(Images.BarButton.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.Default);
			UISegmentedControl.Appearance.SetBackgroundImage(Images.BarButton.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.Default);
			
			UIBarButtonItem.Appearance.SetBackgroundImage(Images.BarButtonLandscape.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.LandscapePhone);
			UISegmentedControl.Appearance.SetBackgroundImage(Images.BarButtonLandscape.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.LandscapePhone);
			
			//BackButton
			UIBarButtonItem.Appearance.SetBackButtonBackgroundImage(Images.BackButton.CreateResizableImage(new UIEdgeInsets(0, 14, 0, 5)), UIControlState.Normal, UIBarMetrics.Default);
			
			UISegmentedControl.Appearance.SetDividerImage(Images.Divider, UIControlState.Normal, UIControlState.Normal, UIBarMetrics.Default);
			
			UIToolbar.Appearance.SetBackgroundImage(Images.Bottombar.CreateResizableImage(new UIEdgeInsets(0, 0, 0, 0)), UIToolbarPosition.Bottom, UIBarMetrics.Default);
			//UIBarButtonItem.Appearance.TintColor = UIColor.White;
			UISearchBar.Appearance.BackgroundImage = Images.Searchbar;
			
			var textAttrs = new UITextAttributes { TextColor = UIColor.White, TextShadowColor = UIColor.DarkGray, TextShadowOffset = new UIOffset(0, -1) };
			UINavigationBar.Appearance.SetTitleTextAttributes(textAttrs);
			UISegmentedControl.Appearance.SetTitleTextAttributes(textAttrs, UIControlState.Normal);
			
			SearchFilterBar.ButtonBackground = Images.BarButton.CreateResizableImage(new UIEdgeInsets(0, 6, 0, 6));
			SearchFilterBar.FilterImage = Images.Filter;
			
			DropbarView.Image = UIImage.FromFile("Images/Controls/Dropbar.png");
			WatermarkView.Image = Images.Background;
			HeaderView.Gradient = Images.CellGradient;
			StyledElement.BgColor = UIColor.FromPatternImage(Images.TableCell);
			ErrorView.AlertImage = UIImage.FromFile("Images/warning.png");
			UserElement.Default = Images.Anonymous;
			NewsFeedElement.DefaultImage = Images.Anonymous;
            NewsFeedElement.LinkColor = UIColor.FromRGB(0, 64, 128);
            NewsFeedElement.LinkFont = UIFont.BoldSystemFontOfSize(12f);
            TableViewSectionView.BackgroundImage = Images.Searchbar;

            WebViewController.BackButtonImage = Images.BackNavigationButton;
            WebViewController.ForwardButtonImage = Images.ForwardNavigationButton;

            IssueCellView.User = new UIImage(Images.Person.CGImage, 1.3f, UIImageOrientation.Up);
            IssueCellView.Priority = new UIImage(Images.Priority.CGImage, 1.3f, UIImageOrientation.Up);
            IssueCellView.Pencil = new UIImage(Images.Pencil.CGImage, 1.3f, UIImageOrientation.Up);
            IssueCellView.Cog = new UIImage(Images.Cog.CGImage, 1.3f, UIImageOrientation.Up);

            RepositoryCellView.Commit = new UIImage(Images.ScmType.CGImage, 1.3f, UIImageOrientation.Up);
            RepositoryCellView.Heart = new UIImage(Images.Heart.CGImage, 1.3f, UIImageOrientation.Up);
            RepositoryCellView.Fork = new UIImage(Images.Fork.CGImage, 1.3f, UIImageOrientation.Up);
			
			//Resize the back button only on the iPhone
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
			{
				UIBarButtonItem.Appearance.SetBackButtonBackgroundImage(Images.BackButtonLandscape.CreateResizableImage(new UIEdgeInsets(0, 14, 0, 6)), UIControlState.Normal, UIBarMetrics.LandscapePhone);
			}
		}

	    /// <summary>
	    /// Processes the accounts.
	    /// </summary>
	    private void ProcessAccounts()
		{
			var defaultAccount = GetDefaultAccount();

			//There's no accounts...
			if (GetDefaultAccount() == null)
			{
			    var login = new AddAccountController {AccountAdded = delegate { ShowMainWindow(); }};

			    //Make it so!
				_window.RootViewController = new UINavigationController(login);
			}
			else
			{
				//Don't remember, prompt for password
				if (defaultAccount.DontRemember)
				{
					var accountsController = new AccountsController();
					accountsController.AccountSelected += obj => {
						Application.SetUser(obj);
						ShowMainWindow();
					};

                    UIViewController loginController = null;
                    if (defaultAccount.AccountType == Account.Type.Bitbucket)
                    {
                        loginController = new LoginViewController {
                            Username = defaultAccount.Username, 
                            LoginComplete = delegate { ShowMainWindow(); }
                        };
                    }
                    else if (defaultAccount.AccountType == Account.Type.GitHub)
                    {
                        loginController = new GitHub.Controllers.Accounts.GitHubLoginController {
                            Username = defaultAccount.Username, 
                            LoginComplete = delegate { ShowMainWindow(); }
                        };
                    }

				    var navigationController = new UINavigationController(accountsController);

                    // This should never happen... But it's better to be prepared...
                    if (loginController != null)
					    navigationController.PushViewController(loginController, false);

					_window.RootViewController = navigationController;
				}
				//If the user wanted to remember the account
				else
				{
					ShowMainWindow();
				}
			}
		}

		/// <summary>
		/// Fade the splash screen
		/// </summary>
		private void BeginSplashFade()
		{
		    if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone) 
                return;

		    var killSplash = MonoTouch.Utilities.IsTall ? 
                new UIImageView(UIImageHelper.FromFileAuto("Default-568h", "png")) : 
                new UIImageView(UIImageHelper.FromFileAuto("Default", "png"));
				
		    _window.AddSubview(killSplash);
		    _window.BringSubviewToFront(killSplash);
				
		    UIView.Animate(0.8, () => { killSplash.Alpha = 0.0f; }, killSplash.RemoveFromSuperview);
		}

		/// <summary>
		/// Gets the default account. If there is not one assigned it will pick the first in the account list.
		/// If there isn't one, it'll just return null.
		/// </summary>
		/// <returns>The default account.</returns>
        private Account GetDefaultAccount()
        {
            var defaultAccount = Application.Accounts.GetDefault();
            if (defaultAccount == null)
            {
				defaultAccount = Application.Accounts.FirstOrDefault();
                Application.Accounts.SetDefault(defaultAccount);
            }
            return defaultAccount;
        }

        private void ShowMainWindow()
        {
            var defaultAccount = GetDefaultAccount();
            Application.SetUser(defaultAccount);

            //This supports the split view configuration of the iPad
//            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
//            {
//                _nav = new MySlideout() { SlideHeight = 999f };
//                _nav.SetMenuNavigationBackgroundImage(Images.TitlebarDark, UIBarMetrics.Default);
//
//                var menuNav = new UINavigationController(new iPadMenuViewController { Slideout = _nav });
//                menuNav.NavigationBar.SetBackgroundImage(Images.TitlebarDark, UIBarMetrics.Default);
//
//                _nav.MenuView = new MenuController();
//
//                var split = new iPadSplitView { Slideout = _nav };
//
//                split.ViewControllers = new UIViewController[] {
//                    menuNav,
//                    _nav,
//                };
//
//                window.RootViewController = split;
//            }
//            else
            {
				_nav = new SlideoutNavigationController();
                _nav.SetMenuNavigationBackgroundImage(Images.TitlebarDark, UIBarMetrics.Default);
                _window.RootViewController = _nav;
            }
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            //Remove everything from the cache
            Application.Cache.DeleteAll();

            //Pop back to the root view...
            if (_nav.TopView != null && _nav.TopView.NavigationController != null)
                _nav.TopView.NavigationController.PopToRootViewController(false);
        }

//        #region Stupid classes for the iPad
//        private class iPadSplitView : UISplitViewController
//        {
//            public SlideoutNavigationController Slideout;
//            public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
//            {
//                Slideout.Hide(false);
//                Slideout.MenuEnabled = (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
//                base.WillRotate(toInterfaceOrientation, duration);
//            }
//
//            public override void ViewWillAppear(bool animated)
//            {
//                Slideout.MenuEnabled = (this.InterfaceOrientation == UIInterfaceOrientation.Portrait || this.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
//                base.ViewWillAppear(animated);
//            }
//        }
//
//        private class iPadMenuViewController : MenuController
//        {
//            public SlideoutNavigationController Slideout;
//            protected override void DoShit(UIViewController controller)
//            {
//                Slideout.MenuView.NavigationController.PushViewController(controller, true);
//            }
//        }
//        #endregion
	}


}

