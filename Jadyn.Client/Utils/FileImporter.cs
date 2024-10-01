using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Jadyn.Common.Models;
using Jadyn.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;

namespace Jadyn.Client.Windows.Utils
{

    public interface IFileImporter
    {
        Task<IEnumerable<TModel>> ImportModelsFromFileAsync<TModel>(StorageFile file, Action<double> callback) where TModel : BaseModel;
    }

    public class FileImporter : IFileImporter
    {
        public async Task<IEnumerable<TModel>> ImportModelsFromFileAsync<TModel>(StorageFile file, Action<double> callback) where TModel : BaseModel
        {
            var result = new List<TModel>();

            using (var stream = await file.OpenStreamForReadAsync())
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);



                    var columnHeaders = worksheet.Row(1).CellsUsed();
                    var headers = columnHeaders.Select(m => m.Value.GetText())
                        .ToList();

                   
                    int currentRow = 2;
                    while (true)
                    {
                        var row = worksheet.Row(currentRow);

                        if (row.IsEmpty())
                        {
                            break;
                        }

                        var tempModel = Activator.CreateInstance<TModel>();


                        foreach (var column in headers.Select((value, columnIndex) => new { columnIndex, value }))
                        {
                            var prop = typeof(TModel)
                                .GetRuntimeProperties()
                                .Where(m => m.GetCustomAttribute<Jadyn.Common.Attributes.ExcelPropetyAttribute>() != null)
                                .First(m => m.GetCustomAttribute<Jadyn.Common.Attributes.ExcelPropetyAttribute>().Name.ToLower().Equals(column.value.ToLower()));

                            if (!ConvertFunctions.ContainsKey(prop.PropertyType))
                            {
                                throw new Exception($"Undefined Property: {prop.Name} with type: {prop.PropertyType.Name}");
                            }
                            var tempCell = row.Cell(column.columnIndex + 1);
                            var tempCellValue = tempCell.Value.ToString();

                            var valueObj = ConvertFunctions[prop.PropertyType].Invoke(tempCellValue);

                            if (!prop.GetCustomAttribute<Jadyn.Common.Attributes.ExcelPropetyAttribute>().IsLink)
                            {
                                prop.SetValue(tempModel, valueObj);
                            }
                            else
                            {
                                typeof(TModel)
                                .GetRuntimeProperties()
                                .First(m => m.Name.Equals($"{prop.Name}Id"))
                                .SetValue(tempModel, valueObj);
                            }
                        }

                        callback.Invoke(((currentRow + 1) / (double)worksheet.RowCount()) * 100);

                        result.Add(tempModel);

                        currentRow++;
                    }

                    //foreach (var row in rows.Select((value, rowIndex) => new { rowIndex, value }))
                    //{


                    //}
                }
            }

            return result;
        }

        private Dictionary<Type, Func<string, object>> ConvertFunctions = new Dictionary<Type, Func<string, object>>()
        {
            {
                typeof(string),
                (str) =>
                {
                    return str;
                }
            },
            {
                typeof(int),
                (str) =>
                {
                    try
                    {
                        return int.Parse(str);
                    }
                    catch (Exception ex)
                    {
                        //todo add logger
                        return default(int);
                    }                }
            },
            {
                typeof(long),
                (str) =>
                {
                    try
                    {
                        return long.Parse(str);
                    }
                    catch (Exception ex)
                    {
                        //todo add logger
                        return default(long);
                    }

                }
            },
            {
                typeof(Gender),
                (str) =>
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        return Gender.Unknown;
                    }
                    var fstChar = str.ToLower()[0];
                    return fstChar.Equals("м") ? Gender.Male : Gender.Female;
                }
            },
            {
                typeof(DateOnly),
                (str) =>
                {
                    try
                    {
                        var dateList = str.Split('.').Select(m => int.Parse(m)).ToArray();
                        DateOnly date = new DateOnly(dateList[2], dateList[1], dateList[0]);
                        return date;
                    }
                    catch (Exception ex)
                    {
                        //todo add logger
                        return default(DateOnly);
                    }
                }
            },
            {
                typeof(City),
                (str) =>
                {
                    var tempDbContext = ((App)(App.Current)).ServiceProvider.GetService<JadynDbContext>();

                    var city = tempDbContext.Cities.FirstOrDefault(m => m.Name.ToLower().Equals(str.ToLower()));

                    if (city != null)
                    {
                        return city.Id;
                    }

                    city = new City()
                    {
                        Name = str
                    };

                    var result = tempDbContext.Cities.Add(city);
                    tempDbContext.SaveChanges();

                    return result.Entity.Id;
                }
            }
        };
    }
}
