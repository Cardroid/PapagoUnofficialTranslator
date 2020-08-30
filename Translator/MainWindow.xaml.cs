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
    private List<LanguageType> SourceLanguageTypes = new List<LanguageType>();
    private List<LanguageType> TargetLanguageTypes = new List<LanguageType>();
    private Timer TextChangeTimer;
    private PapagoBrowser Papago = new PapagoBrowser();
    private object LockObj = new object();

    public MainWindow()
    {
      InitializeComponent();

      TextChangeTimer = new Timer(1000);

      this.Closing += (_, e) =>
      {
        if (Papago != null)
          Papago.Dispose();
      };

      for (int i = 0; i < Enum.GetNames(typeof(LanguageType)).Length; i++)
      {
        SourceLanguageTypes.Add((LanguageType)i);

        // Auto Exclude
        if (i > 0)
          TargetLanguageTypes.Add((LanguageType)i);
      }

      SourceType.ItemsSource = SourceLanguageTypes;
      TargetType.ItemsSource = TargetLanguageTypes;

      SourceType.SelectedIndex = 0;
      TargetType.SelectedIndex = 2;

      Target.IsReadOnly = true;
      Target.IsReadOnlyCaretVisible = false;

      this.Loaded += (_, e) =>
      {
        TargetType.SelectionChanged += TargetType_SelectionChanged;

        TextChangeTimer.Elapsed += TextChangeTimer_Elapsed;
        Source.TextChanged += (_, e) => { TextChangeTimer.Stop(); TextChangeTimer.Start(); };
      };
    }

    private void TextChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      string source = string.Empty;
      LanguageType sourceType = LanguageType.Auto;
      LanguageType targetType = LanguageType.English;

      Dispatcher.Invoke(() =>
      {
        source = Source.Text;
        sourceType = SourceLanguageTypes[SourceType.SelectedIndex];
        targetType = TargetLanguageTypes[TargetType.SelectedIndex];
      });

      lock (LockObj)
      {
        var result = Papago.Translate(source, sourceType, targetType).Result;

        Dispatcher.Invoke(() => { Target.Text = result; });
      }
    }

    private void TargetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (sender is ComboBox comboBox)
        Target.Text = Papago.Translate(Source.Text, SourceLanguageTypes[SourceType.SelectedIndex], TargetLanguageTypes[TargetType.SelectedIndex]).Result;
    }
  }
}
