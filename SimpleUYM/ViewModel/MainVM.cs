using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using SimpleUYM.Model;

namespace SimpleUYM.ViewModel
{
	public class MainVM : ObservableObject
	{
		// Commands
		public RelayCommand MergetoolCommand { get; private set; }
		public RelayCommand ChangeGitBashPathCommand { get; private set; }
		public RelayCommand ChangeGitRepositoryPathCommand { get; private set; }
		public RelayCommand SetupRepositoryCommand { get; private set; }

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

		private string _cmdOutput;
		public string CmdOutput
		{
			get => _cmdOutput;
			set
			{
				_cmdOutput = value;
				OnPropertyChanged(nameof(CmdOutput));
			}
		}

		// Internal data events
		private event Action OnRepositoryPathUpdate;

		public MainVM()
		{
			LoadConfig($"{Environment.CurrentDirectory}\\simpleUYMconfig.json");
			MergetoolCommand = new RelayCommand(OpenMergetool);
			ChangeGitBashPathCommand = new RelayCommand(SetGitBashPath);
			ChangeGitRepositoryPathCommand = new RelayCommand(SetGitRepositoryPath);
			SetupRepositoryCommand = new RelayCommand(SetupRepository);

			OnRepositoryPathUpdate += UpdateIsSetupAvailable;
			// Force its update
			UpdateIsSetupAvailable();
		}
		~MainVM()
		{
			SaveConfig($"{Environment.CurrentDirectory}\\simpleUYMconfig.json");
		}
		private void UpdateIsSetupAvailable() => IsSetupAvailable = !string.IsNullOrEmpty(PathToRepository);

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
					string pathToUnityYAMLMerge = GetFilePathFromUser(filter: "Applications (*.exe)|*.exe|All files (*.*)|*.*")
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
				Console.WriteLine($"Error updating Git configuration: {ex.Message}");
			}
		}
		private void SetGitBashPath()
		{
			PathToGitBash = GetFilePathFromUser(filter: "Applications (*.exe)|*.exe|All files (*.*)|*.*");
		}
		private void SetGitRepositoryPath()
		{
			PathToRepository = GetFolderPathFromUser();
		}

		private void SaveConfig(string filePath)
		{
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}
			ConfigData configData = new ConfigData()
			{
				PathToGitBash = this.PathToGitBash,
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
			PathToGitBash = configData.PathToGitBash;
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
			CmdOutput = CmdHelper.Run("git", "mergetool", PathToRepository);
		}
	}
	public static class CmdHelper
	{
		public static string Run(string command, string commandParameters = "", string workingDir = null)
		{
			// https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
			//Create process
			System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
			//strCommand is path and file name of command to run
			pProcess.StartInfo.FileName = command;
			//strCommandParameters are parameters to pass to program
			pProcess.StartInfo.Arguments = commandParameters;
			pProcess.StartInfo.UseShellExecute = false;
			//Set output of program to be written to process output stream
			pProcess.StartInfo.RedirectStandardOutput = true;
			//Optional
			if (workingDir != null) pProcess.StartInfo.WorkingDirectory = workingDir;
			//Start the process
			pProcess.Start();
			//Get program output
			string output = pProcess.StandardOutput.ReadToEnd();
			//Wait for process to finish
			pProcess.WaitForExit();
			return output;
		}
	}
}
