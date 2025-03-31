using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;

public static class JsonToDataTableConverter
{
    public static DataTable ConvertJsonToDataTable(string json)
    {
        var resultTable = new DataTable();
        var flattenedRow = new Dictionary<string, string>();

        var root = JsonConvert.DeserializeObject<JObject>(json);

        if (root["Data"] != null)
        {
            FlattenJToken(root["Data"], flattenedRow, "Data");
        }

        // Создание столбцов
        foreach (var key in flattenedRow.Keys)
        {
            resultTable.Columns.Add(key, typeof(string));
        }

        // Добавление строки
        var row = resultTable.NewRow();
        foreach (var kvp in flattenedRow)
        {
            row[kvp.Key] = kvp.Value;
        }
        resultTable.Rows.Add(row);

        return resultTable;
    }

    private static void FlattenJToken(JToken token, Dictionary<string, string> dict, string prefix)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                FlattenJToken(property.Value, dict, $"{prefix}.{property.Name}");
            }
        }
        else if (token is JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                FlattenJToken(array[i], dict, $"{prefix}[{i}]");
            }
        }
        else
        {
            dict[prefix] = token?.ToString();
        }
    }
}
