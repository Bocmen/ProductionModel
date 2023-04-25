using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AppProductionModel
{
    public sealed partial class FullScreenDot : Page
    {
        public FullScreenDot()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter is string content)
            {
                Content = CreateViewDot(content);
            }
        }
        public static WebView CreateViewDot(string dotContent)
        {
            WebView webView = new WebView();
            webView.NavigateToString($"<!DOCTYPE><html><head><script src=\"https://d3js.org/d3.v5.min.js\"></script><script src=\"https://unpkg.com/@hpcc-js/wasm@0.3.11/dist/index.min.js\"></script><script src=\"https://unpkg.com/d3-graphviz@3.0.5/build/d3-graphviz.js\"></script> <style type=\"text/css\"> body, html, div, svg {{ height: 100%; width: 100%; margin: 0 auto; padding: 0; }} </style> </head> <body> <div id=\"graph\" style=\"text-align: center;\"></div><script>graphviz = d3.select(\"#graph\").graphviz().renderDot(`{dotContent}`);</script></body></html>");
            return webView;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack) rootFrame.GoBack();
        }
    }
}
