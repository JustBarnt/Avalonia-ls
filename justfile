build:
    mkdir -p bin/lsp
    dotnet build src/AvaloniaLSP/AvaloniaLanguageServer --output bin/lsp

    mkdir -p bin/solution-parser
    dotnet build src/SolutionParser/SolutionParser.csproj --output bin/solution-parser

    mkdir -p bin/xaml-styler
    dotnet build src/XamlStyler/src/XamlStyler.Console/XamlStyler.Console.csproj --output bin/xaml-styler

