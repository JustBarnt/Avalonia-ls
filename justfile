build:
    mkdir -p bin/lsp
    dotnet build src/AvaloniaLSP/AvaloniaLanguageServer --output bin/lsp

    mkdir -p bin/solution-parser
    dotnet build src/SolutionParser/SolutionParser.csproj --output bin/solution-parser

    mkdir -p bin/xaml-styler
    dotnet build src/XamlStyler/src/XamlStyler.Console/XamlStyler.Console.csproj --output bin/xaml-styler


install:
    # just build
    # mkdir -p ~/.local/share/avalonia-ls
    # cp bin/* ~/.local/share/avalonia-ls -r
    echo -e "#!/bin/bash\n exec ~/.local/share/avalonia-ls/xaml-styler/xstyler \"@\"" >> ~/.local/bin/xaml-styler
    chmod +x ~/.local/bin/xaml-styler
    
    echo -e "#!/bin/bash\n exec ~/.local/share/avalonia-ls/lsp/AvaloniaLanguageServer \"@\"" >> ~/.local/bin/avalonia-ls
    chmod +x ~/.local/bin/avalonia-ls
    
    echo -e "#!/bin/bash\n exec ~/.local/share/avalonia-ls/solution-parser/SolutionParser \"@\"" >> ~/.local/bin/avalonia-solution-parser
    chmod +x ~/.local/bin/avalonia-solution-parser

    echo "INSTALLATION COMPLETE!"

    
uninstall:
    rm -rf ~/.local/share/avalonia-ls
    rm ~/.local/bin/xaml-styler ~/.local/bin/avalonia-ls ~/.local/bin/avalonia-solution-parser
    echo "UNINSTALLATION COMPLETE"
    
