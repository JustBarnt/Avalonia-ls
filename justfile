build:
    mkdir -p bin/lsp
    dotnet build src/AvaloniaLSP/AvaloniaLanguageServer --output bin/lsp

    mkdir -p bin/solution-parser
    dotnet build src/SolutionParser/SolutionParser.csproj --output bin/solution-parser

    mkdir -p bin/xaml-styler
    dotnet build src/XamlStyler/src/XamlStyler.Console/XamlStyler.Console.csproj --output bin/xaml-styler

    mkdir -p bin/avalonia-preview
    dotnet build src/AvaloniaPreview --output bin/avalonia-preview

install-unix:
    #!/usr/bin/env nu
    just build
    mkdir ~/.local/share/avalonia-ls
    cp bin/* ~/.local/share/avalonia-ls -r

    echo -e "#!/bin/bash\n exec ~/.local/share/avalonia-ls/xaml-styler/xstyler \"\$@\"" > ~/.local/bin/xaml-styler
    chmod +x ~/.local/bin/xaml-styler

    echo -e "#!/bin/bash\n exec ~/.local/share/avalonia-ls/lsp/AvaloniaLanguageServer \"\$@\"" > ~/.local/bin/avalonia-ls
    chmod +x ~/.local/bin/avalonia-ls

    echo -e "#!/bin/bash\n exec ~/.local/share/avalonia-ls/solution-parser/SolutionParser \"\$@\"" > ~/.local/bin/avalonia-solution-parser
    chmod +x ~/.local/bin/avalonia-solution-parser

    echo -e "#/!bin/bash\n exec ~/.local/share/avalonia-ls/avalonia-preview/AvaloniaPreview \"\$@\"" > ~/.local/bin/avalonia-preview
    chmod +x ~/.local/bin/avalonia-preview

install-windows:
    #! nu

    just build
    mkdir ~/.local/share/avalonia-ls
    cp bin/* ~/.local/share/avalonia-ls -r

    let bin = $"($nu.home-path)\\.local\\bin"
    let output_dir = $"($nu.home-path)\\.local\\share\\avalonia-ls"
    let link_names = ["xaml-styler.exe", "avalonia-ls.exe", "avalonia-solution-parser.exe", "avalonia-preview.exe"]
    mut exe = ""
    mut link = ""

    ## Ensure our $bin dir exists
    mkdir $bin

    ## xaml-styler
    $exe = $output_dir | path join xaml-styler xstyler.exe
    $link = $bin | path join $link_names.0
    print $"Linking ($exe) to: ($link)"
    rm -f $link
    mklink $link $exe

    # avalonia-ls.cmd
    $exe = $output_dir | path join lsp AvaloniaLanguageServer.exe
    $link = $bin | path join $link_names.1
    print $"Linking ($exe) to: ($link)"
    rm -f $link
    mklink $link $exe

    # avalonia-solution-parser.cmd
    $exe = $output_dir | path join solution-parser SolutionParser.exe
    $link = $bin | path join $link_names.2
    print $"Linking ($exe) to: ($link)"
    rm -f $link
    mklink $link $exe

    # avalonia-preview.cmd
    $exe = $output_dir | path join avalonia-preview AvaloniaPreview.exe
    $link = $bin | path join $link_names.3
    print $"Linking ($exe) to: ($link)"
    rm -f $link
    mklink $link $exe

install:
    @just --choose

uninstall:
    rm -rf ~/.local/share/avalonia-ls
    rm ~/.local/bin/xaml-styler ~/.local/bin/avalonia-ls ~/.local/bin/avalonia-solution-parser ~/.local/bin/avalonia-preview
    echo "UNINSTALLATION COMPLETE"
