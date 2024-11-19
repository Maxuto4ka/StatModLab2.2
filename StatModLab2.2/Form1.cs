using System.Windows.Forms.DataVisualization.Charting;

namespace StatModLab2._2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static Random random = new Random();


        // Генерація однієї реалізації Вінерівського процесу
        private double[] GenerateWienerProcess(int steps, double T)
        {
            double[] wienerProcess = new double[steps + 1];
            double dt = T / steps;

            for (int i = 1; i <= steps; i++)
            {
                double dW = Math.Sqrt(dt) * (2 * random.NextDouble() - 1);
                wienerProcess[i] = wienerProcess[i - 1] + dW;
            }

            return wienerProcess;
        }

        // Обчислення часу першого виходу за рівень
        private double ComputeFirstExitTime(double[] process, double level, double dt)
        {
            for (int i = 0; i < process.Length; i++)
            {
                if (Math.Abs(process[i]) >= level)
                {
                    return i * dt; // Час виходу
                }
            }
            return double.PositiveInfinity; // Якщо виходу не відбулося
        }

        private void btnSimulate_Click(object sender, EventArgs e)
        {
            int steps = 1000;        // Кількість кроків
            double T = 1.0;          // Час
            int realizations = 100;  // Кількість реалізацій
            double level = 0.1;      // Рівень для оцінки часу виходу
            double dt = T / steps;

            chart2.Series.Clear();
            // Зберігаємо всі реалізації
            double[][] processes = new double[realizations][];
            for (int i = 0; i < realizations; i++)
            {
                processes[i] = GenerateWienerProcess(steps, T);
                double[] wienerProcess = processes[i];
                // Додаємо серію для кожної реалізації
                var series = chart2.Series.Add($"Реалізація {i + 1}");
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                series.BorderWidth = 1; 
                series.IsVisibleInLegend = false;
                // Додаємо точки для кожної реалізації
                for (int j = 0; j <= steps; j++)
                {
                    series.Points.AddXY(j * dt, wienerProcess[j]);
                }

                chart2.Invalidate();
            }

            // Обчислюємо середнє значення
            double[] mean = new double[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                mean[i] = processes.Select(p => p[i]).Average();
            }

            // Обчислюємо дисперсію
            double[] variance = new double[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                variance[i] = processes.Select(p => p[i]).Average(x => Math.Pow(x - mean[i], 2));
            }

            // Показуємо результати у графіку
            chart1.Series.Clear();
            var seriesMean = chart1.Series.Add("Середнє значення");
            var seriesVariance = chart1.Series.Add("Дисперсія");
            seriesMean.ChartType = SeriesChartType.Line;
            seriesVariance.ChartType = SeriesChartType.Line;

            for (int i = 0; i <= steps; i++)
            {
                seriesMean.Points.AddXY(i, mean[i]);
                seriesVariance.Points.AddXY(i, variance[i]);
            }


            double[] exitTimes = new double[realizations];
            for (int i = 0; i < realizations; i++)
            {
                exitTimes[i] = ComputeFirstExitTime(processes[i], level, dt);
            }

            // Відображаємо емпіричний розподіл у текстовому полі
            var groupedExitTimes = exitTimes.Where(t => !double.IsInfinity(t))
                                            .GroupBy(t => Math.Round(t, 2))
                                            .OrderBy(g => g.Key);

            listBoxResults.Items.Clear();
            foreach (var group in groupedExitTimes)
            {
                listBoxResults.Items.Add($"Час: {group.Key:F2}, Ймовірність: {(double)group.Count() / realizations:F2}");
            }

            MessageBox.Show("Моделювання завершено!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
