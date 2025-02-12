## Avalonia-ls
 Simple standalone Language Server for Avalonia XAML files suitable to lightweight editors like neovim and helix with support for completions and xaml formatting via xaml styler

# Installation
To install from source, dotnet-sdk-9.0 and just must be installed. Clone recursively and run just install recipe( .local/bin must be in your path)
```
git clone https://www.github.com/eugenenoble2005/avalonia-ls.git --recursive
cd avalonia-ls
just install
```

You can also install from the AUR
```
yay -S avalonia-ls-git
```

After installing, you need only configure your editor of choice:
# Helix
```
[[language]]
name="xml"
scope="source.axaml"
injection-regex="axaml"
file-types=["axaml"]
language-servers=["avalonials"]
grammar="xml"
auto-format=false
formatter={command="xaml-styler",args=["--write-to-stdout" ,"--take-pipe"]}
```

# Neovim
```
vim.api.nvim_create_autocmd({ 'BufEnter', 'BufWinEnter' }, {
	pattern = { "*.axaml" },
	callback = function(event)
		vim.lsp.start {
			name = "avalonia",
			cmd = { "avalonia-ls" },
			root_dir = vim.fn.getcwd(),
		}
	end
})
vim.filetype.add({
	extension = {
		axaml = "xml",
	},
})

```

# Instructions
For completions to work on an avalonia project, you must generate metadata and build. If you install with just, an avalonia-solution-parser executable will be installed
```
cd my-avalonia-project
# you should run the following generally in the directory with your .csproj or .fsproj files
avalonia-solution-parser .
dotnet build
```
You need to run avalonia-solution-parser to generate metadata just once. Completions should work afterwards

# PREVIEWER
You can run a xaml previewer in a browser tab for any xaml file in your project:
```
avalonia-previewer --file path/to/axaml/file
```
On supported terminal emulators like kitty, ghostty or wezterm, you can run the previewer directly inside the terminal. This is experimental and does not yet capture input like mouse and keyboard.
```
avalonia-previewer --file path/to/axaml/file --target terminal
```
