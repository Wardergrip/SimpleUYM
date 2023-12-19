﻿using CommunityToolkit.Mvvm.ComponentModel;
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

		// Internal data events
		private event Action OnRepositoryPathUpdate;

		public MainVM() 
		{
			LoadConfig($"{Environment.CurrentDirectory}\\simpleUYMconfig.json");
			MergetoolCommand = new RelayCommand(OpenMergetool);
			ChangeGitBashPathCommand = new RelayCommand(SetGitBashPath);
			ChangeGitRepositoryPathCommand = new RelayCommand(SetGitRepositoryPath);
			SetupRepositoryCommand = new RelayCommand(SetupRepository);

			OnRepositoryPathUpdate += () => IsSetupAvailable = !string.IsNullOrEmpty(PathToRepository);
			// Force its update
			IsSetupAvailable = !string.IsNullOrEmpty(PathToRepository);
		}
		~MainVM()
		{
			SaveConfig($"{Environment.CurrentDirectory}\\simpleUYMconfig.json");
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
					string pathToUnityYAMLMerge = GetFilePathFromUser(filter: "Applications (*.exe)|*.exe|All files (*.*)|*.*")
						.Replace("\\","\\\\"); // Necessary for .git/config file
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
			RunGitBashCommand(PathToGitBash, PathToRepository, "git mergetool");
		}

		private void RunGitBashCommand(string gitBashPath, string workingDirectory, string command)
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
