namespace Main;
using Terminal.Gui;

//this is only used for tracking mouse events
public class TerminalGui
{
    public void Start()
    {
        Application.Init();
        Application.RootMouseEvent += (MouseEvent args) =>
        {
            switch (args.Flags)
            {
                case MouseFlags.Button1Clicked:
                    Console.WriteLine(args.Y);
                    break;
            }
        };

        Application.Top.KeyPress += (View.KeyEventEventArgs args) =>
       {
           // Check for Ctrl+C
           if (args.KeyEvent.Key == Key.a)
           {
               Application.Shutdown();
           }
       };
        Application.Run();
    }
}
