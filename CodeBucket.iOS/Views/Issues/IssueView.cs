using System;
using CodeBucket.Core.ViewModels.Issues;
using UIKit;
using CodeBucket.ViewControllers;
using CodeBucket.Utils;
using CodeBucket.DialogElements;
using System.Linq;
using Humanizer;
using CodeBucket.Core.Utils;
using System.Collections.Generic;
using CodeBucket.Utilities;
using CodeBucket.Services;

namespace CodeBucket.Views.Issues
{
    public class IssueView : PrettyDialogViewController
    {
        private HtmlElement _descriptionElement = new HtmlElement("description");
        private HtmlElement _commentsElement = new HtmlElement("comments");

		public new IssueViewModel ViewModel
		{
			get { return (IssueViewModel) base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public IssueView()
		{
            OnActivation(d =>
            {
                d(_descriptionElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
                d(_commentsElement.UrlRequested.BindCommand(ViewModel.GoToUrlCommand));
            });
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            Title = "Issue #" + ViewModel.Id;
            HeaderView.SetImage(null, Images.Avatar);

            ViewModel.Bind(x => x.Issue).Subscribe(_ => RenderIssue());
            ViewModel.BindCollection(x => x.Comments).Subscribe(_ => RenderComments());

            var compose = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Compose) { Enabled = false };
            OnActivation(d => d(compose.GetClickedObservable().BindCommand(ViewModel.GoToEditCommand)));
		}

		public void RenderComments()
		{
            var comments = ViewModel.Comments
                .Where(x => !string.IsNullOrEmpty(x.Content))
                .Select(x => new CommentViewModel(x.AuthorInfo.Username, ViewModel.ConvertToMarkdown(x.Content), x.UtcCreatedOn.Humanize(), x.AuthorInfo.Avatar));

            _commentsElement.SetValue(new CommentsRazorView { Model = comments.ToList() }.GenerateString());
            InvokeOnMainThread(RenderIssue);
		}


        SplitViewElement _split1 = new SplitViewElement(AtlassianIcon.Configure.ToImage(), AtlassianIcon.Error.ToImage());
        SplitViewElement _split2 = new SplitViewElement(AtlassianIcon.Flag.ToImage(), AtlassianIcon.Spacedefault.ToImage());
        SplitViewElement _split3 = new SplitViewElement(AtlassianIcon.Copyclipboard.ToImage(), AtlassianIcon.Calendar.ToImage());

		public void RenderIssue()
		{
			if (ViewModel.Issue == null)
				return;

            var avatar = new Avatar(ViewModel.Issue.ReportedBy?.Avatar);

			NavigationItem.RightBarButtonItem.Enabled = true;
            HeaderView.Text = ViewModel.Issue.Title;
            HeaderView.SetImage(avatar.ToUrl(), Images.Avatar);
            HeaderView.SubText = ViewModel.Issue.Content ?? "Updated " + ViewModel.Issue.UtcLastUpdated.Humanize();
            RefreshHeaderView();

            var split = new SplitButtonElement();
            split.AddButton("Comments", ViewModel.Comments.Items.Count.ToString());
            split.AddButton("Watches", ViewModel.Issue.FollowerCount.ToString());

            ICollection<Section> root = new LinkedList<Section>();
            root.Add(new Section { split });

			var secDetails = new Section();

			if (!string.IsNullOrEmpty(ViewModel.Issue.Content))
			{
                _descriptionElement.SetValue(new MarkdownRazorView { Model = ViewModel.Issue.Content }.GenerateString());
				secDetails.Add(_descriptionElement);
			}

            _split1.Button1.Text = ViewModel.Issue.Status;
            _split1.Button2.Text = ViewModel.Issue.Priority;
            secDetails.Add(_split1);

            _split2.Button1.Text = ViewModel.Issue.Metadata.Kind;
            _split2.Button2.Text = ViewModel.Issue.Metadata.Component ?? "No Component";
			secDetails.Add(_split2);

            _split3.Button1.Text = ViewModel.Issue.Metadata.Version ?? "No Version";
            _split3.Button2.Text = ViewModel.Issue.Metadata.Milestone ?? "No Milestone";
            secDetails.Add(_split3);

            var assigneeElement = new StringElement("Assigned", ViewModel.Issue.Responsible != null ? ViewModel.Issue.Responsible.Username : "Unassigned", UITableViewCellStyle.Value1) {
                Image = AtlassianIcon.User.ToImage(),
			};
            assigneeElement.Clicked.BindCommand(ViewModel.GoToAssigneeCommand);
			secDetails.Add(assigneeElement);

			root.Add(secDetails);

            if (ViewModel.Comments.Any(x => !string.IsNullOrEmpty(x.Content)))
			{
				root.Add(new Section { _commentsElement });
			}

            var addComment = new StringElement("Add Comment") { Image = AtlassianIcon.Addcomment.ToImage() };
            addComment.Clicked.Subscribe(_ => AddCommentTapped());
			root.Add(new Section { addComment });
            Root.Reset(root);
		}

		void AddCommentTapped()
		{
			var composer = new Composer();
			composer.NewComment(this, async (text) => {
				try
				{
					await composer.DoWorkAsync("Commenting...", () => ViewModel.AddComment(text));
					composer.CloseComposer();
				}
				catch (Exception e)
				{
                    AlertDialogService.ShowAlert("Unable to post comment!", e.Message);
				}
				finally
				{
					composer.EnableSendButton = true;
				}
			});
		}

		public override UIView InputAccessoryView
		{
			get
			{
				var u = new UIView(new CoreGraphics.CGRect(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
				return u;
			}
		}
    }
}

