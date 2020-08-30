using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace PapagoLib
{
  public class PapagoBrowser : IDisposable
  {
    private const string PAPAGO_URL = "https://papago.naver.com";
    private ChromeDriver Chrome;
    private ChromeDriverService ChromeService;
    private string BeforeText;
    private string BeforeResult;
    private LanguageType BeforeSourceType;
    private LanguageType BeforeTargetType;
    private bool IsInitTranslated = false;
    private bool Disposed;
    public bool IsDebug { get; private set; }
    public ChromeOptions Options { get; private set; }

    public PapagoBrowser(bool isDebug = false)
    {
      Init(new ChromeOptions(), isDebug);
    }
    public PapagoBrowser(ChromeOptions options, bool isDebug = false)
    {
      Init(options, isDebug);
    }

    private void Init(ChromeOptions options, bool isDebug)
    {
      ChromeService = ChromeDriverService.CreateDefaultService();

      if (!isDebug)
      {
        options.AddArgument("headless");
        ChromeService.HideCommandPromptWindow = true;
      }

      Chrome = new ChromeDriver(ChromeService, options);
      Options = options;
      IsDebug = isDebug;
      IsInitTranslated = false;
      Disposed = false;
    }

    public void Refresh(bool isForce = false)
    {
      if (isForce)
      {
        Dispose();
        Init(Options, IsDebug);
      }
      else
        Chrome.Navigate().GoToUrl(PAPAGO_URL);
      IsInitTranslated = false;
    }

    /// <summary>
    /// Translate into Papago
    /// </summary>
    /// <param name="text">Text to translate (not to exceed 5,000 characters)</param>
    /// <param name="sourceLanguage">Source Language</param>
    /// <param name="targetLanguage">Target languages other than "Auto" type</param>
    /// <param name="isUrlParser">URL code parsing or not</param>
    /// <param name="delatMilliSeconds">Time limit for verification of translation completion</param>
    /// <param name="intervalMilliSeconds">Time interval between checking if the Ui is loaded</param>
    /// <returns></returns>
    public async Task<TranslateResult> TranslatorAsync(string text, LanguageType sourceLanguage, LanguageType targetLanguage, bool isUrlParser = false, int delatMilliSeconds = 5000, int intervalMilliSeconds = 100)
    {
      if(string.IsNullOrWhiteSpace(text))
        return new TranslateResult(false, "Text is null");
      if(sourceLanguage == targetLanguage)
        return new TranslateResult(false, "SourceLanguage and TargetLanguage cannot be the same.");
      if(!CheckForInternetConnection())
        return new TranslateResult(false, "No internet connection.");
      if (text.Length > 5000)
        return new TranslateResult(false, "Text exceeds 5000 characters.");
      if (targetLanguage == LanguageType.Auto)
        return new TranslateResult(false, "Target language does not support \"auto\" type.");

      if (IsInitTranslated && text.Equals(BeforeText) && sourceLanguage == BeforeSourceType && targetLanguage == BeforeTargetType)
        return new TranslateResult(true, BeforeResult);

      Chrome.Navigate().GoToUrl(UrlParser(text, sourceLanguage, targetLanguage, isUrlParser));

      await WaitTranslateAsync(delatMilliSeconds, intervalMilliSeconds);

      try
      {
        var resultTag = Chrome.FindElementByXPath("//*[@id=\"txtTarget\"]");

        if (resultTag != null)
        {
          BeforeText = text;
          BeforeResult = resultTag.Text;
          BeforeSourceType = sourceLanguage;
          BeforeTargetType = targetLanguage;
          IsInitTranslated = true;
          return new TranslateResult(true, BeforeResult);
        }
        else
          throw new NullReferenceException("Tag is Null");
      }
      catch (NoSuchElementException)
      {
        return new TranslateResult(false, "Element not found (may be caused by short DelayTime)");
      }
      catch (Exception e)
      {
        return new TranslateResult(false, e.Message);
      }
    }

    private async Task<bool> WaitTranslateAsync(int TimeOutMilliSeconds, int intervalMilliSeconds)
    {
      DateTime beforeDateTime = DateTime.Now;
      bool isWait = true;
      IWebElement element = null;
      var wait = new WebDriverWait(Chrome, TimeSpan.FromSeconds(intervalMilliSeconds));

      do
      {
        element = wait.Until((d) =>
        {
          try
          {
            return d.FindElement(By.XPath("//*[@id=\"txtTarget\"]"));
          }
            //catch (NoSuchElementException) { return null; }
            catch (Exception) { return null; }
        });

        if (element != null && !element.Text.Equals(BeforeResult))
          isWait = false;
        else
          await Task.Delay(intervalMilliSeconds);

        // TimeOut
        if ((DateTime.Now - beforeDateTime).TotalMilliseconds > TimeOutMilliSeconds)
          return false;
      }
      while (isWait);

      return true;
    }

    private string UrlParser(string text, LanguageType sourceLanguage, LanguageType targetLanguage, bool isUrlParser = false)
    {
      if (isUrlParser)
        text = Uri.EscapeUriString(text);

      return $"{PAPAGO_URL}/?sk={StringEnum.GetStringValue(sourceLanguage)}&tk={StringEnum.GetStringValue(targetLanguage)}&st={text}";
      //return $"{PAPAGO_URL}/?sk={sourceLanguage}&tk={targetLanguage}&hn=0&st={text}";
    }
    private static bool CheckForInternetConnection()
    {
      try
      {
        using (var client = new WebClient())
        using (client.OpenRead("http://clients3.google.com/generate_204"))
        { return true; }
      }
      catch { return false; }
    }

    public void Dispose()
    {
      if (Disposed)
        return;
      else
        Disposed = true;

      if (Chrome != null)
        Chrome.Dispose();
      if (ChromeService != null)
        ChromeService.Dispose();
      GC.SuppressFinalize(this);
    }
  }

  public class TranslateResult
  {
    public TranslateResult()
    {
      this.Success = false;
      this.Result = string.Empty;
    }
    public TranslateResult(bool success, string result)
    {
      this.Success = success;
      this.Result = result;
    }
    public string Result { get; }
    public bool Success { get; }
  }

  internal class StringValue : Attribute
  {
    public StringValue(string value)
    {
      Value = value;
    }

    public string Value { get; }
  }

  internal static class StringEnum
  {
    public static string GetStringValue(LanguageType value)
    {
      string output = null;

      Type type = value.GetType();

      FieldInfo fi = type.GetField(value.ToString());
      StringValue[] attrs = fi.GetCustomAttributes(typeof(StringValue), false) as StringValue[];

      if (attrs.Length > 0)
        output = attrs[0].Value;

      return output;
    }
  }

  public enum LanguageType
  {
    [StringValue("auto")]
    Auto,
    [StringValue("ko")]
    Korean,
    [StringValue("en")]
    English,
    [StringValue("ja")]
    Japanese,
    [StringValue("zh-CN")]
    Chinese_Simplified,
    [StringValue("zh-TW")]
    Chinese_Traditional,
    [StringValue("es")]
    Spanish,
    [StringValue("fr")]
    French,
    [StringValue("de")]
    German,
    [StringValue("ru")]
    Russian,
    [StringValue("pt")]
    Portuguese,
    [StringValue("it")]
    Italian,
    [StringValue("vi")]
    Vietnamese,
    [StringValue("th")]
    Thai,
    [StringValue("id")]
    Indonesian,
    [StringValue("hi")]
    Hindi,
  }
}

