using VocabularyWidget.Forms;

namespace VocabularyWidget;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new WidgetForm());
    }
}
