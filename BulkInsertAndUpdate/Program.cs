using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Utilities;
using System.Diagnostics;
using System.Data.Entity;

namespace StockUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<Context>());
            //warmup
            using (var ctx = new Context())
            {
                var temp = ctx.Locations.ToList();
            }

            System.IO.Compression.ZipFile.ExtractToDirectory("hourly_14.zip", "data");
            var filePath = "data/hourly_14.json";
            var sw = new Stopwatch();
            var lines = File.ReadLines(filePath);
            var locations = lines.Select(l =>
            {
                dynamic x = JsonConvert.DeserializeObject(l);

                return new Location
                {
                    Id = x.city.id,
                    Name = x.city.name,
                    Country = x.city.country,
                    Temp = 0.0,
                    Pressure = x.data[0].main.pressure,
                };
            }).ToList();
            sw.Start();

            using (var ctx = new Context())
            {
                EFBatchOperation.For(ctx, ctx.Locations).InsertAll(locations);
            }
            sw.Stop();
            using (var ctx = new Context())
            {
                var count = ctx.Locations.Count();
                var top = ctx.Locations.OrderByDescending(l => l.Temp).First();
                Console.WriteLine("Inserted " + count + " locations in " + sw.ElapsedMilliseconds + " ms");
                Console.WriteLine(string.Format("Hottest: {0} {1}K",  top.Name, top.Temp));
            }


            var locations2 = lines.Select(l =>
            {
                dynamic x = JsonConvert.DeserializeObject(l);

                return new Location
                {
                    Id = x.city.id,
                    Temp = x.data[0].main.temp,
                    Pressure = x.data[0].main.pressure,
                };
            }).ToList();

            sw.Restart();
            using (var ctx = new Context())
            {
                EFBatchOperation.For(ctx, ctx.Locations).UpdateAll(locations2, x => x.ColumnsToUpdate(l => l.Pressure, l => l.Temp));
            }
            sw.Stop();
            using (var ctx = new Context())
            {
                var count = ctx.Locations.Count();
                var top = ctx.Locations.OrderByDescending(l => l.Temp).First();
                Console.WriteLine("Updated " + count + " locations in " + sw.ElapsedMilliseconds + " ms");
                Console.WriteLine(string.Format("Hottest: {0} {1}K", top.Name, top.Temp));
            }
        }
    }
}
