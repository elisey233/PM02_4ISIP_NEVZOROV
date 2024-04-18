using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ПМ02_4ИСИП020_Невзоров
{
    public partial class MainWindow : Window
    {
        private int rows;
        private int columns;
        private double[,] table;
        public MainWindow()
        {
            InitializeComponent();

        }



        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            int rows, cols;
            if (int.TryParse(RowsTextBox.Text, out rows) && int.TryParse(ColumnsTextBox.Text, out cols))
            {
                CreateMatrixControls(rows, cols);
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите числовые значения для строк и столбцов.");
            }
        }

        private void CreateMatrixControls(int rows, int cols)
        {
            MatrixPanel.Children.Clear();
            for (int i = 0; i < rows; i++)
            {
                StackPanel rowPanel = new StackPanel { Orientation = Orientation.Horizontal };
                for (int j = 0; j < cols; j++)
                {
                    TextBox textBox = new TextBox { Width = 50, Margin = new Thickness(5) };
                    rowPanel.Children.Add(textBox);
                }
                MatrixPanel.Children.Add(rowPanel);
            }
        }



        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            int rows = MatrixPanel.Children.Count;
            int cols = (MatrixPanel.Children[0] as StackPanel)?.Children.Count ?? 0;

            int[] supply = ParseTextBoxes(SupplyTextBox.Text);
            int[] demand = ParseTextBoxes(DemandTextBox.Text);
            int[,] costs = ParseMatrixTextBoxes(rows, cols);

            BalanceMatrix(ref supply, ref demand, ref costs);

            int totalCost = SolveTransportationProblem(supply, demand, costs);






        }

        private int[] ParseTextBoxes(string text)
        {
            return text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(int.Parse)
                   .ToArray();
        }

        private int[,] ParseMatrixTextBoxes(int rows, int cols)
        {
            int[,] matrix = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                StackPanel rowPanel = MatrixPanel.Children[i] as StackPanel;
                for (int j = 0; j < cols; j++)
                {
                    TextBox textBox = rowPanel.Children[j] as TextBox;
                    matrix[i, j] = int.Parse(textBox.Text);
                }
            }
            return matrix;
        }

        private void BalanceMatrix(ref int[] supply, ref int[] demand, ref int[,] costs)
        {
            // Рассчитываем суммы поставок и потребностей
            int totalSupply = supply.Sum();
            int totalDemand = demand.Sum();

            // Балансируем матрицу, если необходимо
            if (totalSupply > totalDemand)
            {
                // Добавляем фиктивные потребности
                int additionalDemand = totalSupply - totalDemand;
                Array.Resize(ref demand, demand.Length + 1);
                demand[demand.Length - 1] = additionalDemand;

                // Добавляем фиктивные стоимости
                int[,] newCosts = new int[costs.GetLength(0), costs.GetLength(1) + 1];
                for (int i = 0; i < costs.GetLength(0); i++)
                {
                    for (int j = 0; j < costs.GetLength(1); j++)
                    {
                        newCosts[i, j] = costs[i, j];
                    }
                    newCosts[i, costs.GetLength(1)] = 0; // Можно установить любое значение
                }
                costs = newCosts;
            }
            else if (totalDemand > totalSupply)
            {
                // Добавляем фиктивные поставки
                int additionalSupply = totalDemand - totalSupply;
                Array.Resize(ref supply, supply.Length + 1);
                supply[supply.Length - 1] = additionalSupply;

                // Добавляем фиктивные стоимости
                int[,] newCosts = new int[costs.GetLength(0) + 1, costs.GetLength(1)];
                for (int i = 0; i < costs.GetLength(0); i++)
                {
                    for (int j = 0; j < costs.GetLength(1); j++)
                    {
                        newCosts[i, j] = costs[i, j];
                    }
                }
                for (int j = 0; j < costs.GetLength(1); j++)
                {
                    newCosts[costs.GetLength(0), j] = 0; // Можно установить любое значение
                }
                costs = newCosts;
            }
        }

        private int SolveTransportationProblem(int[] supply, int[] demand, int[,] costs)
        {
            int rows = supply.Length;
            int cols = demand.Length;

            // Инициализация опорного плана
            int[,] plan = new int[rows, cols];

            int totalCost = 0;

            // Построение опорного плана
            while (true)
            {
                // Найдем наименьший элемент в матрице стоимостей
                int minCost = int.MaxValue;
                int minI = -1, minJ = -1;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (supply[i] > 0 && demand[j] > 0 && costs[i, j] < minCost)
                        {
                            minCost = costs[i, j];
                            minI = i;
                            minJ = j;
                        }
                    }
                }

                // Если не найдено ни одной ячейки с положительным остатком и нулевой стоимостью, выходим из цикла
                if (minI == -1 || minJ == -1)
                    break;

                // Найдено минимальное значение, перевозим максимально возможное количество
                int quantity = Math.Min(supply[minI], demand[minJ]);

                // Заполнение ячейки опорного плана
                plan[minI, minJ] = quantity;

                // Рассчет общей стоимости
                totalCost += quantity * costs[minI, minJ];

                // Обновление остатков поставок и потребностей
                supply[minI] -= quantity;
                demand[minJ] -= quantity;
            }

            StringBuilder planStringBuilder = new StringBuilder();
            planStringBuilder.AppendLine("Опорный план:");
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    planStringBuilder.Append(plan[i, j] + "\t");
                }
                planStringBuilder.AppendLine();
            }
            planStringBuilder.AppendLine($"Сумма всех перевозок: {totalCost}");
            // Вывод опорного плана в окне сообщения
            MessageBox.Show(planStringBuilder.ToString(), "Опорный план");

            return totalCost;
        }



    }

}
