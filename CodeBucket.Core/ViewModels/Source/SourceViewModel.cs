using System.Threading.Tasks;
using System;
using System.IO;

namespace CodeBucket.Core.ViewModels.Source
{
	public class SourceViewModel : FileSourceViewModel
    {
		private string _user;
		private string _repository;
		private string _branch;
		private string _path;
		private string _name;

		protected override async Task Load(bool forceCacheInvalidation)
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(_name));
            var source = this.GetApplication().Client.Users[_user].Repositories[_repository].Branches[_branch].Source;
            var file = await Task.Run(() => source.GetFile(_path));
            HtmlUrl = "http://bitbucket.org/" + source.Branch.Branches.Repository.Owner.Username + "/" + source.Branch.Branches.Repository.Slug + "/src/" + source.Branch.UrlSafeName + "/" + _path;
            IsText = file.Encoding == null;

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                if (IsText)
                {
                    await writer.WriteAsync(file.Data);
                }
                else if (string.Equals(file.Encoding, "base64", StringComparison.OrdinalIgnoreCase))
                {
                    var data = Convert.FromBase64String(file.Data);
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }

            FilePath = filePath;
        }

		public void Init(NavObject navObject)
		{
			_path = navObject.Path;
			_name = navObject.Name;
			_user = navObject.User;
			_repository = navObject.Repository;
			_branch = navObject.Branch;

			//Create the filename
			var fileName = System.IO.Path.GetFileName(_path);
			if (fileName == null)
				fileName = _path.Substring(_path.LastIndexOf('/') + 1);

			//Create the temp file path
			Title = fileName;
		}

		public class NavObject
		{
			public string Path { get; set; }
			public string Name { get; set; }
			public string User { get; set; }
			public string Repository { get; set; }
			public string Branch { get; set; }
		}
    }
}