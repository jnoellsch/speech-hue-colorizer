using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SpeechHueColorizer.App
{
    using System.Diagnostics;
    using System.Reflection;
    using Windows.ApplicationModel;
    using Windows.Media.SpeechRecognition;
    using Windows.UI;
    using Windows.UI.Core;

    public sealed partial class MainPage : Page
    {
        private SpeechRecognizer _speechRecognizer;
        private IDictionary<string, Color> _colorNames;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeColors();
            this.InitializeSpeechRecognizer();
        }

        private void InitializeColors()
        {
            this._colorNames = new Dictionary<string, Color>();
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                this._colorNames[color.Name.ToLower()] = (Color)color.GetValue(null);
                Debug.WriteLine(color.Name.ToLower());
            }
        }

        private async void InitializeSpeechRecognizer()
        {
            // wire up events
            this._speechRecognizer = new SpeechRecognizer();
            this._speechRecognizer.StateChanged += this.SpeechRecognizerOnStateChanged;
            this._speechRecognizer.ContinuousRecognitionSession.ResultGenerated += this.SpeechRecognizerOnResultGenerated;

            // load grammar
            var grammarFile = await Package.Current.InstalledLocation.GetFileAsync("Grammar\\ColorGrammar.xml");
            var grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarFile);
            this._speechRecognizer.Constraints.Add(grammarConstraint);

            // compile/start grammar file
            var compileResult = await this._speechRecognizer.CompileConstraintsAsync();
            if (compileResult.Status == SpeechRecognitionResultStatus.Success)
            {
                await this._speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void SpeechRecognizerOnResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string command = args.Result.Text;
            Debug.WriteLine(command);

            if (command.StartsWith("color"))
            {
                string spokenColor = command.Replace("color ", string.Empty).Replace(" ", "");
                Color color;

                if (this._colorNames.TryGetValue(spokenColor, out color))
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.Grid.Background = new SolidColorBrush(color));
                }

                // TODO: set light color
            }

            if (command.Equals("light off"))
            {
                // TODO: turn light off
            }

            if (command.Equals("light on"))
            {
                // TODO: turn light on
            }
        }

        private void SpeechRecognizerOnStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine(args.State);
        }
    }
}
