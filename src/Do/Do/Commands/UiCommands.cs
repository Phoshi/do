using System.Windows.Input;

namespace Do.Commands
{
    public enum Command
    {
        LeftMajor,
        LeftMinor,
        RightMajor,
        RightMinor
    }
    public static class UiCommands
    {
        public static RoutedCommand LeftMajorCommand = new RoutedCommand();

        public static RoutedCommand LeftMinorCommand = new RoutedCommand();

        public static RoutedCommand RightMinorCommand = new RoutedCommand();

        public static RoutedCommand RightMajorCommand = new RoutedCommand();

        public delegate void ActionHandler(Command action);

        public static event ActionHandler ActionRaised;

        public static void Raise(Command command)
        {
            ActionRaised?.Invoke(command);
        }
    }
}