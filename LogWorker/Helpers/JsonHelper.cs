using System.Text.Json;

namespace LogWorker.Helpers
{
    public class JsonHelper
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            Converters = { new CustomDateConverter() }
        };

        public static List<T> DeserializeJsonObjects<T>(string logs, string delimeter)
        {
            if (string.IsNullOrEmpty(logs))
            {
                return [];
            }

            var resultList = new List<T>();
            var jsonList = logs.Split(delimeter, StringSplitOptions.RemoveEmptyEntries);

            foreach (var json in jsonList)
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    continue;
                }
                var jsonObjects = FindJsonObjects(json);
                foreach (var jsonObject in jsonObjects)
                {
                    try
                    {
                        var deserializedObject = JsonSerializer.Deserialize<T>(jsonObject, _options);
                        if (deserializedObject != null)
                        {
                            resultList.Add(deserializedObject);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Deserialized object is null for {typeof(T).Name}");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON parsing error for {typeof(T).Name}: {ex.Message}");
                        Console.WriteLine($"Problematic JSON: {jsonObject}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error parsing JSON for {typeof(T).Name}: {ex.Message}");
                        Console.WriteLine($"Problematic JSON: {jsonObject}");
                    }
                }
            }

            return resultList;
        }

        private static List<string> FindJsonObjects(string input)
        {
            var result = new List<string>();
            var stack = new Stack<int>();
            var startIndex = -1;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{')
                {
                    if (stack.Count == 0)
                    {
                        startIndex = i;
                    }
                    stack.Push(i);
                }
                else if (input[i] == '}')
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                        if (stack.Count == 0 && startIndex != -1)
                        {
                            var jsonObject = input.Substring(startIndex, i - startIndex + 1);
                            if (IsValidJson(jsonObject))
                            {
                                result.Add(jsonObject);
                            }
                            startIndex = -1;
                        }
                    }
                }
            }

            return result;
        }

        private static bool IsValidJson(string json)
        {
            json = json.Trim();
            if ((json.StartsWith('{') && json.EndsWith('}')) ||
                (json.StartsWith('[') && json.EndsWith(']')))
            {
                try
                {
                    JsonDocument.Parse(json);
                    return true;
                }
                catch (JsonException)
                {
                    // Invalid JSON
                }
            }
            return false;
        }
    }
}
