using CoreProductionModel.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CoreProductionModel.Extensions;
using System.Collections.ObjectModel;
using System.Text;
using CoreProductionModel.Tools;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static CoreProductionModel.Extensions.ProdDataBaseExtension;

namespace AppProductionModel
{
    public sealed partial class MainPage : Page
    {
        private readonly static Regex parseRules = new Regex(@"\[([^\]]*)\]->\[([^\]]*)\];", RegexOptions.Compiled);
        private readonly static Regex parseOption = new Regex(@"<options>(.*)</options>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private readonly static Regex delecteComments = new Regex(@"[\n\s]+#.*\n", RegexOptions.Compiled);
        public ObservableCollection<Rule> Rules { get; private set; } = new ObservableCollection<Rule>();
        private ProdDataBase _dataBase;
        private FrameworkElement[] _defaultConsoleContent;
        private TypeDetectConflict typeDetectConflict = DefaultTypeDetectConflict;
        private bool stateConsole = false;

        public MainPage()
        {
            SetDataBase(new ProdDataBase());
            InitializeComponent();
            DataContext = this;
        }

        private enum TypeDirectionRuleFile
        {
            Default,
            Reverse
        }

        private class Options
        {
            public TypeDirectionRuleFile TypeDirection { get; set; } = TypeDirectionRuleFile.Default;
            public TypeDetectConflict TypeDetectConflict { get; set; } = DefaultTypeDetectConflict;
            public string SignsDefault { get; set; } = string.Empty;
            public JArray HelpConsoleData { get; set; } = null;
            public bool StartSearch { get; set; } = false;
        }
        public void ConsoleClear()
        {
            Console.Children.Clear();
            if (_defaultConsoleContent != null && _defaultConsoleContent.Any())
            {
                foreach(var elem in _defaultConsoleContent) Console.Children.Add(elem);
                stateConsole = true;
            }
        }
        public void AddElemConsole(FrameworkElement element)
        {
            if (stateConsole)
            {
                Console.Children.Clear();
                stateConsole = false;
            }
            Console.Children.Add(element);
        }
        public async Task LoadFile(StorageFile file)
        {
            if (file != null)
            {
                using (StreamReader sr = new StreamReader(await file.OpenStreamForReadAsync()))
                {
                    string textFile = await sr.ReadToEndAsync();
                    textFile = delecteComments.Replace(textFile, string.Empty);
                    string optionsJson = null;
                    textFile = parseOption.Replace(textFile, (t) =>
                    {
                        optionsJson = t.Groups[1].Value;
                        return string.Empty;
                    });
                    Options options = new Options();
                    if (!string.IsNullOrWhiteSpace(optionsJson))
                        options = JsonConvert.DeserializeObject<Options>(optionsJson);
                    typeDetectConflict = options.TypeDetectConflict;
                    _defaultConsoleContent = CreateHelpElem(options.HelpConsoleData)?.ToArray();
                    SerchText.Text = options.SignsDefault ?? string.Empty;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        ConsoleClear();
                        ProdDataBase prodDataBase = new ProdDataBase();
                        var matches = parseRules.Matches(textFile).Cast<Match>();
                        if (options.TypeDirection == TypeDirectionRuleFile.Reverse) matches = matches.Reverse();
                        foreach (Match match in matches)
                        {
                            try
                            {
                                if (!prodDataBase.TryAddDefaultCreateSign(match.Groups[1].Value, match.Groups[2].Value, out Rule ruleCreated, out Rule ruleConflict))
                                    AddConflictToConsole(ruleCreated, ruleConflict);
                            }
                            catch (Exception ex)
                            {
                                AddElemConsole(DecorateElem(CreateTitle($"{ex.Message}: {match.Groups[0].Value}")));
                            }
                        }
                        SetDataBase(prodDataBase);
                    });
                    if (options.StartSearch) SearchButton_Click(null, null);
                }
            }
        }

        private void SetDataBase(ProdDataBase prodDataBase)
        {
            _dataBase = prodDataBase;
            Rules.Clear();
            foreach (var rule in _dataBase.Rules) Rules.Add(rule);
        }
        private FrameworkElement CreateViewDot(string dotContent)
        {
            WebView webView = FullScreenDot.CreateViewDot(dotContent);
            webView.HorizontalAlignment = HorizontalAlignment.Stretch;
            webView.MinHeight = 350;
            Button button = new Button()
            {
                Content = "Отобразить в полный экран",
                Style = (Style)Application.Current.Resources[App.NameStyleButton],
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            StackPanel stackPanel = new StackPanel()
            {
                Children =
                {
                    webView,
                    button
                },
                Spacing = 5
            };
            button.Click += (d, dd) =>
            {
                Frame.Navigate(typeof(FullScreenDot), dotContent);
            };
            return stackPanel;
        }
        private static FrameworkElement DecorateElem(IEnumerable<FrameworkElement> elements)
        {
            StackPanel stackPanel = new StackPanel() { Spacing = 5 };
            foreach (var elem in elements) stackPanel.Children.Add(elem);
            return new Frame()
            {
                Content = stackPanel,
                Style = (Style)Application.Current.Resources[App.NameStyleFramePanel]
            };
        }
        private static FrameworkElement DecorateElem(params FrameworkElement[] elements) => DecorateElem(elements.AsEnumerable());
        private static FrameworkElement CreateTitle(string text) => new TextBlock()
        {
            Text = text,
            Style = (Style)Application.Current.Resources[App.NameStyleTitle]
        };
        private static FrameworkElement CreateText(string text) => new TextBlock()
        {
            Text = text,
            Style = (Style)Application.Current.Resources[App.NameStyleTextBlock]
        };
        private async void AddRule_Click(object sender, RoutedEventArgs e)
        {
            string title;
            object content = null;
            bool isViewDetail = false;
            Rule ruleCreated = null, ruleConflict = null;
            try
            {
                if (!_dataBase.TryAddDefaultCreateSign(RuleCondition.Text, RuleConsequence.Text, out ruleCreated, out ruleConflict))
                {
                    bool typeConflict = ruleCreated.Equals(ruleConflict);
                    title = $"Добавляемое правило {(typeConflict ? "полностью" : "частично")} совпадает с уже имеющимся";
                    content = $"Добавляемое {ruleCreated} конфликтует с уже существующим {ruleConflict}.\nДля отображение данных и/или графов нажмите \"Детально\".";
                    isViewDetail = true;
                }
                else
                {
                    Rules.Add(ruleCreated);
                    return;
                }
            }
            catch (Exception error)
            {
                title = error.Message;
            }
            ContentDialog contentDialog = new ContentDialog()
            {
                Style = (Style)Application.Current.Resources[App.NameStyleContentDialog],
                Title = title,
                CloseButtonText = "Ясно",
                Content = content,
                CloseButtonStyle = (Style)Application.Current.Resources[App.NameStyleButton]
            };
            if (isViewDetail)
            {
                contentDialog.PrimaryButtonText = "Детально";
                contentDialog.PrimaryButtonStyle = (Style)Application.Current.Resources[App.NameStyleButton];
            }
            var resultDialog = await contentDialog.ShowAsync();
            if (isViewDetail && resultDialog == ContentDialogResult.Primary)
                AddConflictToConsole(ruleCreated, ruleConflict);
        }
        private void ClearConsole_Click(object sender, RoutedEventArgs e) => ConsoleClear();

        private static Rule GetRuleContext(object element) => element is FrameworkElement frameworkElement && frameworkElement.DataContext is Rule rule ? rule : null;
        private void DeleteRule_Click(object sender, RoutedEventArgs e)
        {
            Rule rule = GetRuleContext(sender);
            if (_dataBase.TryRemove(rule)) Rules.Remove(rule);
        }
        private void ShowRule_Click(object sender, RoutedEventArgs e)
        {
            Rule rule = GetRuleContext(sender);
            AddElemConsole(DecorateElem(CreateTitle(rule.ToString()), CreateViewDot(DotTreeDrawOperator.CreateDot(rule.Operator))));
        }
        private void AddConflictToConsole(Rule ruleCreate, Rule ruleConflict)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"digraph G {{subgraph cluster_0{{ label=\"Добавляемое правило: {ruleCreate}\"");
            int endIndex = DotTreeDrawOperator.DrawTree((line) => stringBuilder.Append(line), ruleCreate.Operator);
            stringBuilder.Append($"}}subgraph cluster_1{{ label=\"Конфликтует с: {ruleConflict}\"");
            _ = DotTreeDrawOperator.DrawTree((line) => stringBuilder.Append(line), ruleConflict.Operator, endIndex);
            stringBuilder.Append("}}");
            AddElemConsole(DecorateElem(CreateTitle($"{ruleCreate} конфликтует с {ruleConflict}"), CreateViewDot(stringBuilder.ToString())));
        }
        private async void LoaddataBase_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(App.TypeFileName);
            await LoadFile(await picker.PickSingleFileAsync());
        }
        private async void SaveDataBase_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.FileTypeChoices.Add(App.TypeFileName, new List<string>() { App.TypeFileName });
            savePicker.SuggestedFileName = "fileName";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (StreamWriter sw = new StreamWriter(await file.OpenStreamForWriteAsync()))
                {
                    foreach (var rule in Rules)
                        sw.WriteLine($"[{rule.Operator}]->[{rule.Sign}];");
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SerchText.Text)) { AddElemConsole(DecorateElem(CreateTitle("Задайте начальные признаки"))); return; }
                ConsoleClear();
                List<string> signs = new List<string>();
                var startSigns = SerchText.Text.Split(',').Select(x => DefaultSign.Create(x));
                signs.AddRange(startSigns.Select(x => x.GetValue()));
                int indexIteration = 0;
                bool isFoundNewSign = false;
                var result = _dataBase.Check(startSigns, (selectRule, detectConflictRule) =>
                {
                    signs.Add($"{selectRule.Sign} получено из правила {selectRule}");
                    List<FrameworkElement> elems = new List<FrameworkElement>
                    {
                        CreateTitle($"Итерация №{++indexIteration}"),
                        DecorateElem(CreateText($"Было добавлено [{selectRule.Sign}] в результате срабатывания [{selectRule.Operator}]")),
                        DecorateElem(CreateText(string.Format("Уже знаю о следующем:\n{0}", string.Join("\n", signs))))
                    };
                    if (detectConflictRule != null)
                    {
                        StringBuilder stringBuilder = new StringBuilder("digraph G {");
                        int counterRule = 0;
                        int indexatorElem = 0;
                        foreach (var ruleInfo in detectConflictRule)
                        {
                            stringBuilder.Append($"subgraph cluster_{counterRule++}{{label=\"Характеристика разрешения: {ruleInfo.MaxMandatorySigns}, Следствие {ruleInfo.Rule.Sign}\"");
                            indexatorElem = DotTreeDrawOperator.DrawTree((line) => stringBuilder.Append(line), ruleInfo.Rule.Operator, indexatorElem);
                            stringBuilder.Append('}');
                        }
                        stringBuilder.Append('}');
                        elems.Add(DecorateElem(CreateText("Был обнаружен и разрешён конфликт"), CreateViewDot(stringBuilder.ToString())));
                    }
                    AddElemConsole(DecorateElem(elems));
                    isFoundNewSign = true;
                }, typeDetectConflict);
                if (isFoundNewSign)
                    AddElemConsole(DecorateElem(CreateTitle("В результате работы были полученные следующие данные"), CreateText(string.Join('\n', signs))));
                else
                    AddElemConsole(DecorateElem(CreateTitle("Результатов нет")));
            }catch(Exception ex)
            {
                ConsoleClear();
                AddElemConsole(DecorateElem(CreateTitle($"{ex.Message}")));
            }
        }
    
        private static IEnumerable<FrameworkElement> CreateHelpElem(JArray jArray)
        {
            if(jArray == null || !jArray.Any()) yield break;
            foreach (var jToken in jArray)
            {
                if (jToken is JObject jobject)
                {
                    if (jobject.TryGetValue("Type", StringComparison.OrdinalIgnoreCase, out JToken typeToken) &&
                        jobject.TryGetValue("Value", StringComparison.OrdinalIgnoreCase, out JToken valueToken)
                        )
                    {
                        string typeName = typeToken.ToObject<string>().ToLower();
                        switch (typeName)
                        {
                            case "text":
                                yield return CreateText(valueToken.ToObject<string>());
                                break;
                            case "title":
                                yield return CreateTitle(valueToken.ToObject<string>());
                                break;
                            case "decorate":
                                if(valueToken is JArray jarrayValue)
                                    yield return DecorateElem(CreateHelpElem(jarrayValue));
                                break;
                        }
                    }
                }
            }
            yield break;
        }
    }
}
