using System;
using UIKit;
using System.Linq;
using CodeBucket.ViewControllers;
using CodeBucket.Core.ViewModels.App;
using CodeBucket.DialogElements;

namespace CodeBucket.Views.App
{
	public class DefaultStartupView : ViewModelCollectionDrivenDialogViewController
    {
		public DefaultStartupView()
        {
			Title = "Default Startup View";
			EnableSearch = false;
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var vm = (DefaultStartupViewModel)ViewModel;
			BindCollection(vm.StartupViews, x => {
                var e = new StringElement(x);
                e.Clicked.Subscribe(_ => vm.SelectedStartupView = x);
				if (string.Equals(vm.SelectedStartupView, x))
					e.Accessory = UITableViewCellAccessory.Checkmark;
				return e;
			}, true);

            vm.Bind(x => x.SelectedStartupView, true).Subscribe(x =>
			{
				if (Root.Count == 0)
					return;
                foreach (var m in Root[0].Elements.Cast<StringElement>())
					m.Accessory = (string.Equals(m.Caption, x)) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			});
		}
    }
}

