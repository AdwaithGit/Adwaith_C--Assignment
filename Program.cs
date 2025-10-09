using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.ImageSharp;

class Adwaith_CSharpAssignment
{
    static async Task Main(string[] args)
    {
        HttpClient client = new HttpClient();
        string url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

        try
        {
            string response = await client.GetStringAsync(url);
            JArray data = JArray.Parse(response);

            // Dictionary to hold total hours worked by each employee
            Dictionary<string, double> employeeHours = new Dictionary<string, double>();

            foreach (var item in data)
            {
                string name = item["EmployeeName"]?.ToString()?.Trim();
                double hours = Convert.ToDouble(item["HoursWorked"] ?? 0);

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (employeeHours.ContainsKey(name))
                    employeeHours[name] += hours;
                else
                    employeeHours[name] = hours;
            }

            // ✅ No hardcoding — using only dynamic API data
            using (StreamWriter sw = new StreamWriter("EmployeeTable.html"))
            {
                sw.WriteLine("<html><head><title>Employee Work Hours</title>");
                sw.WriteLine("<style>");
                sw.WriteLine("table, th, td { border: 1px solid black; border-collapse: collapse; padding: 8px; }");
                sw.WriteLine(".low { background-color: #ff9999; }");
                sw.WriteLine("</style></head><body>");
                sw.WriteLine("<h2>Employee Total Work Hours</h2>");
                sw.WriteLine("<table><tr><th>Name</th><th>Total Hours Worked</th></tr>");

                foreach (var emp in employeeHours)
                {
                    string rowClass = emp.Value < 100 ? " class='low'" : "";
                    sw.WriteLine($"<tr{rowClass}><td>{emp.Key}</td><td>{emp.Value}</td></tr>");
                }

                sw.WriteLine("</table></body></html>");
            }

            Console.WriteLine("EmployeeTable.html created successfully!");

            // ✅ Create dynamic Pie Chart from live API data
            PieSeries pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelPosition = 0.5,
                AngleSpan = 360,
                StartAngle = 0,
                OutsideLabelFormat = "{1}: {0}"
            };

            foreach (var emp in employeeHours)
            {
                pieSeries.Slices.Add(new PieSlice(emp.Key, emp.Value));
            }

            PlotModel plotModel = new PlotModel { Title = "Employee Hours Distribution" };
            plotModel.Series.Add(pieSeries);

            using (var fs = File.Create("EmployeePieChart.png"))
            {
                var exporter = new PngExporter(600, 400);
                exporter.Export(plotModel, fs);
            }

            Console.WriteLine("EmployeePieChart.png created successfully!");
            Console.WriteLine("\nNote: All employee names and data are fetched dynamically from the API.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
