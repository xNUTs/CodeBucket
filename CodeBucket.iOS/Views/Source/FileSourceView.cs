using System;
using CodeBucket.Views;
using UIKit;
using Foundation;
using MvvmCross.Platform;
using CodeBucket.Core.ViewModels;
using CodeBucket.Core.Services;

namespace CodeBucket.Views.Source
{
	public abstract class FileSourceView : WebView
    {
		private bool _loaded = false;

		public new FileSourceViewModel ViewModel
		{ 
			get { return (FileSourceViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected FileSourceView()
			: base(false)
		{
		}

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			//Stupid but I can't put this in the ViewDidLoad...
			if (!_loaded)
			{
				ViewModel.LoadCommand.Execute(null);
				_loaded = true;
			}

			Title = ViewModel.Title;
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
		}

		private void ShowExtraMenu()
		{
            var sheet = new UIActionSheet();
			var openButton = sheet.AddButton("Open In");
			var shareButton = ViewModel.HtmlUrl != null ? sheet.AddButton("Share") : -1;
			var showButton = ViewModel.HtmlUrl != null ? sheet.AddButton("Show in Bitbucket") : -1;
			var cancelButton = sheet.AddButton("Cancel");
			sheet.CancelButtonIndex = cancelButton;
			sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Dismissed += (s, e) => {
                BeginInvokeOnMainThread(() =>
                {
				if (e.ButtonIndex == openButton)
				{
					var ctrl = new UIDocumentInteractionController();
					ctrl.Url = NSUrl.FromFilename(ViewModel.FilePath);
					ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
				}
				else if (e.ButtonIndex == shareButton)
				{
                    Mvx.Resolve<IShareService>().ShareUrl(ViewModel.HtmlUrl);
				}
				else if (e.ButtonIndex == showButton)
				{
					ViewModel.GoToHtmlUrlCommand.Execute(null);
				}
                });

                sheet.Dispose();
			};

            sheet.ShowFrom(NavigationItem.RightBarButtonItem, true);
		}
    }
}

