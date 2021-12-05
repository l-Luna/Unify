using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Octokit;

namespace Unify;

internal class MainWindow : Window {

	public MainWindow() {
		InitializeComponent();
	}

	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);

		// Get Quintessential and MonoMod versions
		UpdateAvailableVersions();
	}

	public void UpdateAvailableVersions() {
		var github = new GitHubClient(new ProductHeaderValue("Quintessential-Unify-Installer"));

		async void setupVersions(string comboboxName, string repoOwner, string repoName) {
			ComboBox verBox = this.FindControl<ComboBox>(comboboxName);
			var versions = await GetReleases(github, repoOwner, repoName);
			verBox.Items = versions;
			verBox.SelectedItem = versions[0];
		}

		setupVersions("QuintessentialVersionPicker", "l-Luna", "Quintessential");
		setupVersions("MonoModVersionPicker", "MonoMod", "MonoMod");
		setupVersions("OpusMutatumVersionPicker", "l-Luna", "OpusMutatum");
	}

	public async Task<List<string>> GetReleases(GitHubClient client, string repoOwner, string repoName) {
		var releases = await client.Repository.Release.GetAll(repoOwner, repoName);
		return releases.Select(k => k.TagName).ToList();
	}

	public async Task<string?> GetPath(string name, string ext, string dir) {
		OpenFileDialog dialog = new();
		dialog.Filters.Add(new FileDialogFilter() { Name = name, Extensions = { ext } });
		dialog.AllowMultiple = false;
		if(!string.IsNullOrWhiteSpace(dir))
			dialog.Directory = dir;

		string[]? result = await dialog.ShowAsync(this);

		return result != null ? result[0] : null;
	}

	public async void OMFolderSelect(object sender, RoutedEventArgs e) {
		TextBox textBox = this.FindControl<TextBox>("OMFolder");
		string? path = await GetPath("Lightning executable", "exe", textBox.Text);
		if(path != null)
			textBox.Text = path;
	}

	public async void QuintessentialFileSelect(object sender, RoutedEventArgs e) {
		TextBox textBox = this.FindControl<TextBox>("QuintessentialFile");
		string? path = await GetPath("Quintessential file", "dll", textBox.Text);
		if(path != null)
			textBox.Text = path;
	}

	public async void MonoModFileSelect(object sender, RoutedEventArgs e) {
		TextBox textBox = this.FindControl<TextBox>("MonoModFile");
		string? path = await GetPath("MonoMod executable", "exe", textBox.Text);
		if(path != null)
			textBox.Text = path;
	}

	public async void OpusMutatumFileSelect(object sender, RoutedEventArgs e) {
		TextBox textBox = this.FindControl<TextBox>("OpusMutatumFile");
		string? path = await GetPath("Opus Mutatum executable", "exe", textBox.Text);
		if(path != null)
			textBox.Text = path;
	}

	public async void InstallQuintessential(object sender, RoutedEventArgs e) {
		GitHubClient github = new(new ProductHeaderValue("Quintessential-Unify-Installer"));
		using HttpClient client = new();

		// download specified quintessential version
		var url = (await github.Repository.Release.GetLatest("l-Luna", "Quintessential")).Assets[0].BrowserDownloadUrl;
		using(var quintResponse = await client.GetAsync(url)) {
			File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "QuintessentialLatest.dll"), await quintResponse.Content.ReadAsByteArrayAsync());
		}
	}
}