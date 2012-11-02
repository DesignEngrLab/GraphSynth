using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GraphSynth.UI
{
    /// <summary>
    /// Interaction logic for AdditionalFunctionToFileDialog.xaml
    /// </summary>
    public partial class AdditionalFunctionToFileDialog : Window
    {
        private readonly List<string> functionNames;
        private readonly string[] sourceFilePaths;
        private readonly string[] origFilesWithFunc;
        private readonly bool[] localfound;
        private readonly int numFuncs;
        private readonly string[] sourceFileNames;
        private readonly int numSources;
        public string[] FilesWithFunc { get; private set; }


        public AdditionalFunctionToFileDialog(Boolean isThisRecognize, List<string> newFunctions, string[] filesWithFunc, bool[] found, string[] sourceFiles)
        {
            if (isThisRecognize)
                Title = "Locations of Recognize Functions in Files";
            else Title = "Locations of Apply Functions in Files";
            numFuncs = newFunctions.Count;
            functionNames = newFunctions;
            origFilesWithFunc = new string[numFuncs];
            filesWithFunc.CopyTo(origFilesWithFunc, 0);
            FilesWithFunc = filesWithFunc;
            localfound = new Boolean[numFuncs];
            found.CopyTo(localfound,0);
            numSources = sourceFiles.GetLength(0);
            sourceFilePaths = sourceFiles;
            sourceFileNames = sourceFiles.Select(Path.GetFileName).ToArray();
            InitializeComponent();

            for (int i = 0; i < numFuncs; i++)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                if (found[i])
                {
                    row.Children.Add(new TextBlock(new Run(i + ". " + newFunctions[i] + " was found in: ")));
                    row.Children.Add(new TextBlock(new Run(Path.GetFileName(filesWithFunc[i]))) { Background = new SolidColorBrush(Color.FromRgb(178, 228, 160)) });
                }
                else
                {
                    row.Children.Add(new TextBlock(new Run(i + ". " + newFunctions[i] + " was not found, place in: ")));
                    for (int j = 0; j < numSources; j++)
                    {
                        var thisRadioBtn = new RadioButton { Content = sourceFileNames[j] };
                        thisRadioBtn.Click += radioBtn_Click;
                        row.Children.Add(thisRadioBtn);
                    }
                }
                listPanel.Children.Add(row);
            }
            btnOK.IsEnabled = localfound.All(b => b);
        }

        void radioBtn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < numFuncs; i++)
            {
                if (localfound[i]) continue;
                var row = (StackPanel)listPanel.Children[i];
                for (int j = 0; j < numSources; j++)
                {
                    var btn = (RadioButton)row.Children[j + 1];
                    if (btn.IsChecked==null || !btn.IsChecked.Value) continue;
                    FilesWithFunc[i] = sourceFilePaths[j];
                    localfound[i] = true;
                    break;
                }
            }
            btnOK.IsEnabled = localfound.All(b => b);
        }


        internal static void Show(Boolean isThisRecognize, List<string> newFunctions, string[] filesWithFunc, bool[] found, string[] sourceFiles)
        {
            var diag = new AdditionalFunctionToFileDialog(isThisRecognize, newFunctions, filesWithFunc, found, sourceFiles);
            diag.ShowDialog();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            FilesWithFunc = origFilesWithFunc;
            Close();
        }


    }
}
