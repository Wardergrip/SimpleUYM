using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using SimpleUYM.Model;
using System.Text;

namespace SimpleUYM.ViewModel
{
	public class MainVM : ObservableObject
	{
		// Commands
		public RelayCommand MergetoolCommand { get; private set; }
		public RelayCommand ChangeUnityYAMLMergePathCommand { get; private set; }
		public RelayCommand ChangeGitRepositoryPathCommand { get; private set; }
		public RelayCommand SetupRepositoryCommand { get; private set; }
		public RelayCommand OpenGithubReposCommand { get; private set; }

		// Data
		private string _pathToUnityYAMLMerge;
		public string PathToUnityYAMLMerge
		{
			get => _pathToUnityYAMLMerge;
			private set
			{
				_pathToUnityYAMLMerge = value;
				OnPropertyChanged(nameof(PathToUnityYAMLMerge));
				OnUnityYAMLMergePathUpdate?.Invoke();
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
				OnRepositoryPathUpdate?.Invoke();
			}
		}
		private bool _isSetupAvailable;
		public bool IsSetupAvailable
		{
			get => _isSetupAvailable;
			set
			{
				_isSetupAvailable = value;
				OnPropertyChanged(nameof(IsSetupAvailable));
			}
		}

		private string BatFilePath { get => $"{Environment.CurrentDirectory}\\mergetool.bat"; }

		// Internal data events
		private event Action OnRepositoryPathUpdate;
		private event Action OnUnityYAMLMergePathUpdate;

		public MainVM()
		{
			LoadConfig($"{Environment.CurrentDirectory}\\simpleUYMconfig.json");
			MergetoolCommand = new RelayCommand(OpenMergetool);
			ChangeUnityYAMLMergePathCommand = new RelayCommand(SetPathToUnityYAMLMerge);
			ChangeGitRepositoryPathCommand = new RelayCommand(SetGitRepositoryPath);
			SetupRepositoryCommand = new RelayCommand(SetupRepository);
			OpenGithubReposCommand = new RelayCommand(OpenGithubRepository);

			OnRepositoryPathUpdate += UpdateIsSetupAvailable;
			OnUnityYAMLMergePathUpdate += UpdateIsSetupAvailable;
			// Force its update
			UpdateIsSetupAvailable();
		}
		~MainVM()
		{
			SaveConfig($"{Environment.CurrentDirectory}\\simpleUYMconfig.json");

			if (File.Exists(BatFilePath))
			{
				File.Delete(BatFilePath);
			}
		}
		private void UpdateIsSetupAvailable() => IsSetupAvailable = !string.IsNullOrEmpty(PathToRepository) && !string.IsNullOrEmpty(PathToUnityYAMLMerge);

		private void OpenGithubRepository()
		{
			Process.Start(new ProcessStartInfo("https://github.com/Wardergrip/SimpleUYM"));
		}

		private void SetupRepository()
		{
			try
			{
				string contentToCheck = "[merge]";
				string filePath = $"{PathToRepository}\\.git\\config";
				// Read the content of the file
				string fileContent = File.ReadAllText(filePath);

				// Check if the content is present
				if (!fileContent.Contains(contentToCheck))
				{
					string pathToUnityYAMLMerge = PathToUnityYAMLMerge
						.Replace("\\", "\\\\"); // Necessary for .git/config file
					if (pathToUnityYAMLMerge == null)
					{
						return;
					}
					string contentToAdd = $"[merge]\n\ttool = unityyamlmerge\n\n[mergetool \"unityyamlmerge\"]\n\ttrustexitcode = false\n\tcmd='{pathToUnityYAMLMerge}' merge -p \"$BASE\" \"$REMOTE\" \"$LOCAL\" \"$MERGED\"";
					// Append the content if not present
					File.AppendAllText(filePath, contentToAdd);
					System.Windows.MessageBox.Show("Repository is sucessfully setup", "[SimpleUYM] Setup", MessageBoxButton.OK);
				}
				else
				{
					System.Windows.MessageBox.Show("Repository is already setup", "[SimpleUYM] Setup", MessageBoxButton.OK);
				}
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show($"Error updating Gitconfiguration: {ex.Message}","[SimpleUYM] Error", MessageBoxButton.OK);
			}
		}
		private void SetPathToUnityYAMLMerge()
		{
			string userInput = GetFilePathFromUser(filter: "Applications (*.exe)|*.exe|All files (*.*)|*.*");
			PathToUnityYAMLMerge = string.IsNullOrEmpty(userInput) ? PathToUnityYAMLMerge : userInput;
		}
		private void SetGitRepositoryPath()
		{
			string userInput = GetFolderPathFromUser();
			PathToRepository = string.IsNullOrEmpty(userInput) ? PathToRepository : userInput;
		}

		private void SaveConfig(string filePath)
		{
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}
			ConfigData configData = new ConfigData()
			{
				PathToUnityYAMLMerge = this.PathToUnityYAMLMerge,
				PathToRepository = this.PathToRepository
			};
			File.WriteAllText(filePath, JsonConvert.SerializeObject(configData));
		}
		private bool LoadConfig(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return false;
			}
			ConfigData configData = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(filePath));
			if (configData == null)
			{
				return false;
			}
			PathToUnityYAMLMerge = configData.PathToUnityYAMLMerge;
			PathToRepository = configData.PathToRepository;
			return true;
		}

		private string GetFilePathFromUser(string filter = null)
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
		private string GetFolderPathFromUser()
		{
			string path = null;
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				path = folderBrowserDialog.SelectedPath;
			}
			return path;
		}
		private void OpenMergetool()
		{
			if (!File.Exists(BatFilePath))
			{
				File.Create(BatFilePath).Close();
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("git mergetool\npause");
			File.WriteAllText(BatFilePath, sb.ToString());

			using (Process process = new Process())
			{
				process.StartInfo.WorkingDirectory = PathToRepository;
				process.StartInfo.FileName = BatFilePath;
				process.Start();
			}
		}
	}
}
