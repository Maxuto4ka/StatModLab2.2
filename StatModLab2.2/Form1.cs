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


        // ��������� ���� ��������� ³���������� �������
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

        // ���������� ���� ������� ������ �� �����
        private double ComputeFirstExitTime(double[] process, double level, double dt)
        {
            for (int i = 0; i < process.Length; i++)
            {
                if (Math.Abs(process[i]) >= level)
                {
                    return i * dt; // ��� ������
                }
            }
            return double.PositiveInfinity; // ���� ������ �� ��������
        }

        private void btnSimulate_Click(object sender, EventArgs e)
        {
            int steps = 1000;        // ʳ������ �����
            double T = 1.0;          // ���
            int realizations = 100;  // ʳ������ ���������
            double level = 0.1;      // г���� ��� ������ ���� ������
            double dt = T / steps;

            chart2.Series.Clear();
            // �������� �� ���������
            double[][] processes = new double[realizations][];
            for (int i = 0; i < realizations; i++)
            {
                processes[i] = GenerateWienerProcess(steps, T);
                double[] wienerProcess = processes[i];
                // ������ ���� ��� ����� ���������
                var series = chart2.Series.Add($"��������� {i + 1}");
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                series.BorderWidth = 1; 
                series.IsVisibleInLegend = false;
                // ������ ����� ��� ����� ���������
                for (int j = 0; j <= steps; j++)
                {
                    series.Points.AddXY(j * dt, wienerProcess[j]);
                }

                chart2.Invalidate();
            }

            // ���������� ������ ��������
            double[] mean = new double[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                mean[i] = processes.Select(p => p[i]).Average();
            }

            // ���������� ��������
            double[] variance = new double[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                variance[i] = processes.Select(p => p[i]).Average(x => Math.Pow(x - mean[i], 2));
            }

            // �������� ���������� � �������
            chart1.Series.Clear();
            var seriesMean = chart1.Series.Add("������ ��������");
            var seriesVariance = chart1.Series.Add("��������");
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

            // ³��������� ��������� ������� � ���������� ���
            var groupedExitTimes = exitTimes.Where(t => !double.IsInfinity(t))
                                            .GroupBy(t => Math.Round(t, 2))
                                            .OrderBy(g => g.Key);

            listBoxResults.Items.Clear();
            foreach (var group in groupedExitTimes)
            {
                listBoxResults.Items.Add($"���: {group.Key:F2}, ���������: {(double)group.Count() / realizations:F2}");
            }

            MessageBox.Show("����������� ���������!", "������", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
