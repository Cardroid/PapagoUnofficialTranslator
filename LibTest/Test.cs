using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PapagoLib;

namespace LibTest
{
  [TestClass]
  public class Test
  {
    [TestMethod]
    public void TranslateWaitTest()
    {
      bool success = false;
      string[] test = {
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
        "�ȳ��ϼ���",
      };
      using PapagoBrowser papagoBrowser = new PapagoBrowser(true);

      for (int i = 0; i < test.Length; i++)
      {
        var result = papagoBrowser.TranslatorAsync(test[i], LanguageType.Auto, LanguageType.English).GetAwaiter().GetResult();
        if (result.Success)
        {
          success = true;
          Trace.WriteLine(result.Result);
        }
      }
      Assert.IsTrue(success);
    }

    [TestMethod]
    public void TranslateSpeedTest()
    {
      bool success = false;
      string[] test = {
        "�ȳ��ϼ���",
        "�׽�Ʈ",
        "�ȳ��ϼ���",
        "�׽�Ʈ",
        "�ȳ��ϼ���",
        "�׽�Ʈ",
        "�ȳ��ϼ���",
        "�׽�Ʈ",
        "�ȳ��ϼ���",
        "�׽�Ʈ",
        "�ȳ��ϼ���",
        "�׽�Ʈ",
        "�ȳ��ϼ���",
      };
      using PapagoBrowser papagoBrowser = new PapagoBrowser(true);

      for (int i = 0; i < test.Length; i++)
      {
        var result = papagoBrowser.TranslatorAsync(test[i], LanguageType.Auto, LanguageType.English).GetAwaiter().GetResult();
        if (result.Success)
        {
          success = true;
          Trace.WriteLine(result.Result);
        }
      }
      Assert.IsTrue(success);
    }

    [TestMethod]
    public void TranslateLanguageTypeTest()
    {
      bool success = false;
      using PapagoBrowser papagoBrowser = new PapagoBrowser(true);

      foreach (var item in Enum.GetNames(typeof(LanguageType)))
      {
        var result = papagoBrowser.TranslatorAsync("�ȳ��ϼ���", LanguageType.Korean, (LanguageType)Enum.Parse(typeof(LanguageType), item)).GetAwaiter().GetResult();
        Trace.WriteLine($"{item,-20}   {result.Result}");

        if (result.Success)
          success = true;
      }

      Assert.IsTrue(success);
    }
  }
}
