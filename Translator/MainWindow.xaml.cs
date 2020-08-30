using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using PapagoLib;

namespace Translator
{
  public partial class MainWindow : Window
  {
    public List<LanguageType> LanguageTypes = new List<LanguageType>();
    public Timer TextChangeTimer;
    public PapagoBrowser Papago;

    public MainWindow()
    {
      InitializeComponent();

      TextChangeTimer = new Timer(500);

      this.Closing += (_, e) =>
      {
        if (Papago != null)
          Papago.Dispose();
      };

      for (int i = 0; i < Enum.GetNames(typeof(LanguageType)).Length; i++)
        LanguageTypes.Add((LanguageType)i);

      SourceType.ItemsSource = LanguageTypes;
      TargetType.ItemsSource = LanguageTypes;

      SourceType.SelectedIndex = 0;
      TargetType.SelectedIndex = 2;

      Target.IsReadOnly = true;
      Target.IsReadOnlyCaretVisible = false;

      this.Loaded += (_, e) =>
      {
        TextChangeTimer.Elapsed += async (_, e) =>
        {
          if (Papago == null)
            Papago = new PapagoBrowser();

          string source = string.Empty;
          LanguageType sourceType = LanguageType.Auto;
          LanguageType targetType = LanguageType.English;

          Dispatcher.Invoke(() =>
          {
            source = Source.Text;
            sourceType = LanguageTypes[SourceType.SelectedIndex];
            targetType = LanguageTypes[TargetType.SelectedIndex];
          },
           DispatcherPriority.Background);

          var result = (await Papago.TranslatorAsync(source, sourceType, targetType)).Result;

          Dispatcher.Invoke(() =>
          {
            Target.Text = result;
          },
           DispatcherPriority.Background);
        };
        Source.TextChanged += (_, e) => { TextChangeTimer.Stop(); TextChangeTimer.Start(); };
      };
    }
  }
}
