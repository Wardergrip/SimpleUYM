using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SimpleUYM.ViewModel
{
	public class MainVM : ObservableObject
	{
		// Commands
		public RelayCommand MergetoolCommand { get; private set; }
		public RelayCommand ChangeGitBashPathCommand { get; private set; }
		public RelayCommand ChangeGitRepositoryPathCommand { get; private set; }

		// Data
		private string _pathToGitBash;
		public string PathToGitBash 
		{
			get => _pathToGitBash;
			private set
			{
				_pathToGitBash = value;
				OnPropertyChanged(nameof(PathToGitBash));
			}
		}
		private string _pathToRepository;
		public string PathToRepository
		{
			get => _pathToRepository;
			private set
			{
				_pathToRepository = value;
				OnPropertyChanged(nameof(PathToRepository));
			}
		}

		public MainVM() 
		{
			MergetoolCommand = new RelayCommand(OpenMergetool);
			ChangeGitBashPathCommand = new RelayCommand(SetGitBashPath);
			ChangeGitRepositoryPathCommand = new RelayCommand(SetGitRepositoryPath);
		}
		public void SetGitBashPath()
		{
			PathToGitBash = GetFilePathFromUser(filter: "Applications (*.exe)|*.exe|All files (*.*)|*.*");
		}
		public void SetGitRepositoryPath() 
		{
			PathToRepository = GetFolderPathFromUser();
		}
		public string GetFilePathFromUser(string filter = null)
		{
			string path = null;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (filter != null) openFileDialog.Filter = filter;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				path = openFileDialog.FileName;
			}
			return path;
		}
		public string GetFolderPathFromUser()
		{
			string path = null;
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				path = folderBrowserDialog.SelectedPath;
			}
			return path;
		}
		public void OpenMergetool()
		{
			RunGitBashCommand(PathToGitBash, PathToRepository, "git mergetool");
		}

		private static void RunGitBashCommand(string gitBashPath, string workingDirectory, string command)
		{
			using (Process process = new Process())
			{
				process.StartInfo.FileName = gitBashPath;
				process.StartInfo.WorkingDirectory = workingDirectory;
				process.StartInfo.Arguments = command;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;

				process.OutputDataReceived += (sender, eventArgs) =>
				{
					if (!string.IsNullOrEmpty(eventArgs.Data))
					{
						Console.WriteLine($"Output: {eventArgs.Data}");
					}
				};

				process.ErrorDataReceived += (sender, eventArgs) =>
				{
					if (!string.IsNullOrEmpty(eventArgs.Data))
					{
						Console.WriteLine($"Error: {eventArgs.Data}");
					}
				};

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}
		}
	}
}
