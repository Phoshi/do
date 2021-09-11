using System.Windows.Input;

namespace Do.Commands
{
    public static class UiCommands
    {
        public static RoutedCommand LeftMajorCommand = new RoutedCommand();

        public static RoutedCommand LeftMinorCommand = new RoutedCommand();

        public static RoutedCommand RightMinorCommand = new RoutedCommand();

        public static RoutedCommand RightMajorCommand = new RoutedCommand();
        
        public static RoutedCommand ActivationCommand = new RoutedCommand();
    }
}