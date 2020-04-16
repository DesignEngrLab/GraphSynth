using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GraphSynth.GraphLayout;
using GraphSynth.Search;
using Path = System.Windows.Shapes.Path;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private const string defaultPluginDir = "defaultPlugins/";
        private const int keyNumOffset = (int)Key.F3;
        private const int endOfFKeys = 12 - 3 + 1;

        private List<Type> GraphLayoutAlgorithms;
        private List<SearchProcess> SearchAlgorithms;
        private FileSystemWatcher glWatcher, sWatcher;

        #region Constructor

        public MainWindow()
        {
            try
            {
                /* invoke description as stored in XAML file MainWindow.xaml through this InitializeComponent() 
                 * function call. */
                InitializeComponent();
                Title += Assembly.GetExecutingAssembly().GetName().Version.ToString();
                /* We need to create a Dictionary of all shortcut keys. This function is NOT included 
                 * in this file. It is included in MainWindow.CommandBindings.cs. */
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    //Icon="/GraphSynth.MainApp;component/Properties/Images/GraphSynth2.ico"
                    var iconUri = new Uri("pack://application:,,,/Properties/GraphSynth2.ico",
                                          UriKind.RelativeOrAbsolute);
                    Icon = BitmapFrame.Create(iconUri);
                }


                setUpCommandBinding();

                /* The node and arc toolbars are not set in MainWindow.XAML  (as all other controls are via
                 * the InitializeComponent() function) since they may differ based on the library. As a 
                 * result we create these in a C# function: setUpNodeArcAddButtons() */
                setUpGraphElementAddButtons();

                /* The graph layout menu items will be found and populated at run time. The following
                 * function simply adds a menu item for each graph layout plugin found. */
                setUpGraphLayoutMenu();

                /* The search process menu items are also populated at runtime by the DLL's that are
                 * included in the search plugins directory. */
                setUpSearchProcessMenu();
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion

        #region Called from the constructor, setUpNodeArcAddButtons, sets up the node and arc buttons

        public void setUpGraphElementAddButtons()
        {
            var dic = new ResourceDictionary();
            try
            {
                dic.Source = new Uri(GSApp.settings.WorkingDirAbsolute + GSApp.settings.CustomShapesFile,
                                     UriKind.Absolute);
            }
            catch
            {
                SearchIO.output("Custom Shapes File was not found or is invalid.");
            }
            foreach (string k in dic.Keys)
                if (!Application.Current.Resources.Contains(k))
                    Application.Current.Resources.Add(k, XamlWriter.Save(dic[k]));

            var dtNIS = (DataTemplate)Application.Current.Resources["NodeIconShape"];
            var dtNHS = (DataTemplate)Application.Current.Resources["HyperArcIconShape"];
            var dtAddBtn = (DataTemplate)Application.Current.Resources["AddButtonTemplate"];
            NodeAddToolbar.Items.Clear();
            ArcAddToolbar.Items.Clear();
            HyperAddToolbar.Items.Clear();
            SelectedAddItems = new List<string>();
            SelectedAddItems.Add("");
            var shortCutKeysList = new List<Key>();
            shortCutKeysList.Add(Key.Escape);
            var temp = (Button)dtAddBtn.LoadContent();
            var imageLength = temp.Width - 8;

            foreach (object key in Application.Current.Resources.Keys)
            {
                var keyString = key as string;
                try
                {
                    var b = (Button)dtAddBtn.LoadContent();
                    var s = (Shape)MyXamlHelpers.Parse((string)Application.Current.Resources[key]);
                    var scStr = "";
                    if (s.Tag != null) scStr = s.Tag.ToString().Split(new[] { ':' })[0];
                    var scKey = findShortCut(ref scStr);
                    shortCutKeysList.Add(scKey);
                    SelectedAddItems.Add(keyString);
                    b.Name = keyString;
                    b.Click += AddNodeArcButton_Click;
                    b.MouseDoubleClick += AddNodeArcButton_DoubleClick;
                    if (keyString.ToLowerInvariant().Contains("node"))
                    {
                        var NIS = (Shape)dtNIS.LoadContent();
                        var NISsize = new Size(NIS.Width, NIS.Height);
                        var scale = imageLength / Math.Max(s.Height, NISsize.Height);
                        s.Height *= scale;
                        s.Width *= scale;
                        NIS.Height *= scale;
                        NIS.Width *= scale;
                        s.VerticalAlignment = NIS.VerticalAlignment = VerticalAlignment.Top;
                        NIS.Margin = new Thickness(0, (s.Height - NIS.Height) / 2, 0, 0);
                        b.Tag = scKey;
                        for (var i = 0; i <= NodeAddToolbar.Items.Count; i++)
                            if (i == NodeAddToolbar.Items.Count)
                            {
                                NodeAddToolbar.Items.Add(b);
                                break;
                            }
                            else if ((int)scKey < (int)((Key)((Button)NodeAddToolbar.Items[i]).Tag))
                            {
                                NodeAddToolbar.Items.Insert(i, b);
                                break;
                            }
                        ((TextBlock)((Grid)b.Content).Children[0]).Text = scStr;
                        ((TextBlock)((Grid)b.Content).Children[1]).Text = keyString;
                        ((Grid)b.Content).Children.Insert(0, s);
                        ((Grid)b.Content).Children.Insert(1, NIS);
                    }
                    else if (keyString.ToLowerInvariant().Contains("arc"))
                    {
                        ((TextBlock)((Grid)b.Content).Children[0]).Text = scStr;
                        ((TextBlock)((Grid)b.Content).Children[1]).Text = keyString;
                        s.VerticalAlignment = VerticalAlignment.Top;
                        Grid.SetRow(s, 0);
                        ((Grid)b.Content).Children.Add(s);
                        if (s as Path != null)
                        {
                            var pathGeo = ((Path)s).Data;
                            if (pathGeo.IsFrozen) pathGeo = pathGeo.Clone();
                            var scale = imageLength /
                                        Math.Max(pathGeo.Bounds.Height, pathGeo.Bounds.Width);
                            pathGeo.Transform = new ScaleTransform(scale, scale);
                        }
                        else s.Height = s.Width = imageLength;
                        b.Tag = scKey;
                        for (var i = 0; i <= ArcAddToolbar.Items.Count; i++)
                            if (i == ArcAddToolbar.Items.Count)
                            {
                                ArcAddToolbar.Items.Add(b);
                                break;
                            }
                            else if ((int)scKey < (int)((Key)((Button)ArcAddToolbar.Items[i]).Tag))
                            {
                                ArcAddToolbar.Items.Insert(i, b);
                                break;
                            }
                    }
                    else if (keyString.ToLowerInvariant().Contains("hyper"))
                    {
                        ((TextBlock)((Grid)b.Content).Children[0]).Text = scStr;
                        ((TextBlock)((Grid)b.Content).Children[1]).Text = keyString;
                        s.VerticalAlignment = VerticalAlignment.Top;
                        Grid.SetRow(s, 0);
                        ((Grid)b.Content).Children.Add(s);
                        if (s as Path != null)
                        {
                            var pathGeo = ((Path)s).Data;
                            if (pathGeo.IsFrozen) pathGeo = pathGeo.Clone();
                            var scale = imageLength /
                                        Math.Max(pathGeo.Bounds.Height, pathGeo.Bounds.Width);
                            pathGeo.Transform = new ScaleTransform(scale, scale);
                        }
                        else s.Height = s.Width = imageLength;
                        b.Tag = scKey;
                        for (var i = 0; i <= HyperAddToolbar.Items.Count; i++)
                            if (i == HyperAddToolbar.Items.Count)
                            {
                                HyperAddToolbar.Items.Add(b);
                                break;
                            }
                            else if ((int)scKey < (int)((Key)((Button)HyperAddToolbar.Items[i]).Tag))
                            {
                                HyperAddToolbar.Items.Insert(i, b);
                                break;
                            }
                    }
                }
                catch
                {
                    SearchIO.output(keyString + " did not load correctly");
                }
            }
            shortCutKeys = shortCutKeysList.Select(k=>k.ToString()).ToArray();
        }

        private Key findShortCut(ref string scStr)
        {
            Key scKey;
            int dummy;
            if (int.TryParse(scStr, out dummy))
                Enum.TryParse("D" + scStr, true, out scKey);
            else if (Enum.TryParse(scStr, true, out scKey)) { }
            else scKey = Key.Escape;
            if (scKey == Key.Escape) scStr = "";
            return scKey;
        }

        #endregion

        /// <summary>
        ///   Sets up graph layout menu. It would have been better to use the .NET 3.5 pipeline approach
        ///   to add-ins. Was I just lazy? I tried to figure it out for 2 whole weeks. It involves up to 7
        ///   projects and would require GraphGUI to be serialized. However, my view of graph layout is
        ///   to make quick changes to postion of nodes (perhaps specific ones may change shapes and back-
        ///   ground but these will be few). Seeing as how XamlWriter and XamlReader are slow and bottleneck
        ///   the entire application, I thought simply to take this approach. All graph layouts will need
        ///   to be in the GraphSynth.GraphLayout assembly and be derived from the GraphLayoutBaseClass. Each
        ///   one found will be given a slot on the layout menu.
        /// </summary>
        public void setUpGraphLayoutMenu()
        {
            SearchIO.output("Setting Up Graph Layout Algorithms");
            var keyNumOffset = (int)Key.D0;
            var k = 0;
            GraphLayoutAlgorithms = new List<Type>();
            GraphLayoutCommands = new List<RoutedUICommand>();
            GraphLayoutSubMenu.Items.Clear();
            var potentialAssemblies = getPotentialAssemblies(GSApp.settings.GraphLayoutDirAbs);
            if (potentialAssemblies.Count == 0) return;
            foreach (string filepath in potentialAssemblies)
            {
                Assembly GraphLayoutAssembly = null;
                try
                {
                    GraphLayoutAssembly = Assembly.LoadFrom(filepath);
                    var layouts = GraphLayoutAssembly.GetTypes();
                    foreach (Type lt in layouts)
                        if (!lt.IsAbstract && GraphLayoutController.IsInheritedType(lt)
                            && !GraphLayoutAlgorithms.Any(w => w.FullName.Equals(lt.FullName)))
                        {
                            var newLayAlgo = GraphLayoutController.Make(lt);
                            if (newLayAlgo != null)
                                try
                                {
                                    KeyGesture kg = null;
                                    if (k < 10) kg = new KeyGesture((Key)(k + keyNumOffset), ModifierKeys.Alt);
                                    else if (k < 20)
                                        kg = new KeyGesture((Key)(k + keyNumOffset - 10),
                                                            ModifierKeys.Alt | ModifierKeys.Shift);
                                    else if (k < 30)
                                        kg = new KeyGesture((Key)(k + keyNumOffset - 20),
                                                            ModifierKeys.Control | ModifierKeys.Alt |
                                                            ModifierKeys.Shift);
                                    else
                                        MessageBox.Show(
                                            "No shortcut has been assigned to " + newLayAlgo.text +
                                            ". That sure is an awful lot "
                                            +
                                            " of graph layout algorithms! Consider reducing the number included in the plugins directory",
                                            "No ShortCut Assigned", MessageBoxButton.OK);
                                    GraphLayoutAlgorithms.Add(lt);
                                    var newGLCommand = new RoutedUICommand(newLayAlgo.text, lt.Name,
                                                                           GetType(),
                                                                           new InputGestureCollection { kg });
                                    GraphLayoutCommands.Add(newGLCommand);
                                    CommandBindings.Add(new CommandBinding(newGLCommand, GraphLayoutOnExecuted,
                                                                           GraphLayoutCanExecute));
                                    GraphLayoutSubMenu.Items.Add(new MenuItem { Command = newGLCommand });
                                    SearchIO.output("\t" + lt.Name + " loaded successfully.");
                                    k++;
                                }
                                catch (Exception exc)
                                {
                                    SearchIO.output("Unable to load " + lt.Name + ": " + exc.Message);
                                }
                        }
                }
                catch
                {
                    // File was either not found are not of the right type.
                }
            }
            if (Directory.Exists(GSApp.settings.GraphLayoutDirAbs))
            {
                glWatcher = new FileSystemWatcher(GSApp.settings.GraphLayoutDirAbs, "*.dll");
                glWatcher.Changed += GraphLayoutDir_Changed;
                glWatcher.Created += GraphLayoutDir_Changed;
                glWatcher.Deleted += GraphLayoutDir_Changed;
                glWatcher.Renamed += GraphLayoutDir_Changed;
                glWatcher.EnableRaisingEvents = true;
                glWatcher.IncludeSubdirectories = true;
                glWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                         | NotifyFilters.FileName;
            }
        }

        private void GraphLayoutDir_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke((ThreadStart)setUpGraphLayoutMenu);
        }

        public void setUpSearchProcessMenu()
        {
            SearchIO.output("Setting Up Search Process Algorithms");
            var k = 0;
            SearchCommands = new List<RoutedUICommand>();
            SearchAlgorithms = new List<SearchProcess>();
            while (DesignDropDown.Items.Count > 14)
                DesignDropDown.Items.RemoveAt(14);

            var potentialAssemblies = getPotentialAssemblies(GSApp.settings.SearchDirAbs);
            potentialAssemblies.Add("thisEXE");

            foreach (string filepath in potentialAssemblies)
            {
                Assembly searchAssembly = null;
                try
                {
                    if (filepath == "thisEXE") searchAssembly = Assembly.GetExecutingAssembly();
                    else searchAssembly = Assembly.LoadFrom(filepath);
                    var searchprocesses = searchAssembly.GetTypes();
                    foreach (Type spt in searchprocesses)
                        if (!spt.IsAbstract && SearchProcess.IsInheritedType(spt)
                            && !SearchAlgorithms.Any(w => w.GetType().FullName.Equals(spt.FullName)))
                        {
                            try
                            {
                                var constructor = spt.GetConstructor(new Type[0]);
                                var searchAlgo = (SearchProcess)constructor.Invoke(null);
                                searchAlgo.settings = GSApp.settings;
                                KeyGesture kg = null;
                                if (k < endOfFKeys) kg = new KeyGesture((Key)(k + keyNumOffset), ModifierKeys.None);
                                else if (k < 2 * endOfFKeys)
                                    kg = new KeyGesture((Key)(k + keyNumOffset - endOfFKeys), ModifierKeys.Shift);
                                else if (k < 3 * endOfFKeys)
                                    kg = new KeyGesture((Key)(k + keyNumOffset - 2 * endOfFKeys),
                                                        ModifierKeys.Control | ModifierKeys.Shift);
                                else
                                    MessageBox.Show(
                                        "No shortcut has been assigned to " + searchAlgo.text +
                                        ". That sure is an awful lot "
                                        +
                                        " of search process algorithms! Consider reducing the number included in the plugins directory",
                                        "No ShortCut Assigned", MessageBoxButton.OK);
                                SearchAlgorithms.Add(searchAlgo);
                                var newSearchCommand = new RoutedUICommand(searchAlgo.text, spt.Name,
                                                                           GetType(), new InputGestureCollection { kg });
                                SearchCommands.Add(newSearchCommand);
                                CommandBindings.Add(new CommandBinding(newSearchCommand, RunSearchProcessOnExecuted,
                                                                       RunSearchProcessCanExecute));
                                DesignDropDown.Items.Add(new MenuItem { Command = newSearchCommand });
                                k++;
                                SearchIO.output("\t" + spt.Name + " loaded successfully.");
                            }
                            catch (Exception exc)
                            {
                                SearchIO.output("Unable to load " + spt.Name + ": " + exc.Message);
                            }
                        }
                }
                catch (Exception exc)
                {
                    if (searchAssembly == null)
                        SearchIO.output("Unable to open " + filepath + ": " + exc.Message);
                    else
                        SearchIO.output("Unable to open " + searchAssembly.FullName + "(" + filepath + "): " +
                                        exc.Message);
                }
            }
            if (Directory.Exists(GSApp.settings.SearchDirAbs))
            {
                sWatcher = new FileSystemWatcher(GSApp.settings.SearchDirAbs, "*.dll");
                sWatcher.Changed += SearchDir_Changed;
                sWatcher.Created += SearchDir_Changed;
                sWatcher.Deleted += SearchDir_Changed;
                sWatcher.Renamed += SearchDir_Changed;
                sWatcher.EnableRaisingEvents = true;
                sWatcher.IncludeSubdirectories = true;
                sWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                        | NotifyFilters.FileName;
            }
        }

        private static List<string> getPotentialAssemblies(string directory)
        {
            var potentialAssemblies = Directory.GetFiles(GlobalSettings.ExecDir, "*.dll", SearchOption.TopDirectoryOnly).ToList();
            if (Directory.Exists(GlobalSettings.ExecDir + defaultPluginDir))
                potentialAssemblies.AddRange(Directory.GetFiles(GlobalSettings.ExecDir + defaultPluginDir,
                                                                "*.dll", SearchOption.AllDirectories));
            if (Directory.Exists(directory))
                potentialAssemblies.AddRange(Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories));
            else SearchIO.output("Plugin directory" + directory + " not found.");

            potentialAssemblies.RemoveAll(fs => fs.Contains("/obj/"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("GraphSynth.CustomControls.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("GraphSynth.BaseClasses.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("StarMath.dll"));
            potentialAssemblies.RemoveAll(fs => fs.Contains("OptimizationToolbox.dll"));

            return potentialAssemblies;
        }

        private void SearchDir_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke((ThreadStart)setUpSearchProcessMenu);
        }
    }
}