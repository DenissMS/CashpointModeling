using System;
using System.Windows;
using ELW.Library.Math;
using ELW.Library.Math.Exceptions;
using ELW.Library.Math.Expressions;

namespace CashpointModeling
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ModelingScene _modelingScene;

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            _modelingScene.StartModeling(); 
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _modelingScene.PauseModeling(); 
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _modelingScene.StopModeling(); 
        }
        
        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            var configuration = new ModelingConfiguration
            {
                StepRange = 2,
                CashpointsNumber = Convert.ToInt32(CashpointsCount.Text),
                CashpointsSize = new Size(20, 40),
                ClientSize = new Size(10, 10),
                ClientInteractionLimit =
                    new TimeLimits(new TimeSpan(0, 0, 0, Convert.ToInt32(InteractionTimeLowerLimit.Text)),
                        new TimeSpan(0, 0, 0, Convert.ToInt32(InteractionTimeUpperLimit.Text))),
                ClientWaitingLimit =
                    new TimeLimits(new TimeSpan(0, 0, 0, Convert.ToInt32(WaitingTimeLowerLimit.Text)),
                        new TimeSpan(0, 0, 0, Convert.ToInt32(WaitingTimeLowerLimit.Text))),
                ClientsCount = Convert.ToInt32(ClientsCount.Text),
                QueueDistance = 12,
                WaitingTimeFormula = СompileExpression(WaitingTimeFormula.Text),
                InteractionTimeFormula = СompileExpression(InteractionTimeFormula.Text)
            };
            _modelingScene = new ModelingScene(ClientsInfo, configuration);
            Scene.Children.Add(_modelingScene);
        }

        private CompiledExpression СompileExpression(string expression)
        {
            CompiledExpression compiledExpression = null;
            try
            {
                PreparedExpression preparedExpression = ToolsHelper.Parser.Parse(expression);
                compiledExpression = ToolsHelper.Compiler.Compile(preparedExpression);
            }
            catch (CompilerSyntaxException ex)
            {
                MessageBox.Show(String.Format("Синтаксическая ошибка: {0}", ex.Message));
            }
            catch (MathProcessorException ex)
            {
                MessageBox.Show(String.Format("Ошибка: {0}", ex.Message));
            }
            catch (ArgumentException)
            {
                MessageBox.Show(Properties.Resources.DataInputError);
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.UnknownError);
                throw;
            }
            return compiledExpression;
        }
    }
}
