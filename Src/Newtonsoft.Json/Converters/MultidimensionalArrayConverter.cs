using System;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Convert a multi-dimensional array to and from JSON
  /// </summary>
  public class MultidimensionalArrayConverter : JsonConverter
  {
    /// <summary>
    /// Writes the JSON representation of the object
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      Array array = (Array)value;
      WriteJson(writer, array, serializer, new int[0]);
    }

    private static void WriteJson(JsonWriter writer, Array array, JsonSerializer serializer, int[] indices)
    {
      int N = indices.Length;
      if (array.Rank == N)
      {
        serializer.Serialize(writer, array.GetValue(indices));
        return;
      }
      int[] newIndices = new int[N + 1];
      for (int n = 0; n < N; ++n)
        newIndices[n] = indices[n];
      writer.WriteStartArray();
      for (int i = 0; i < array.GetLength(N); ++i)
      {
        newIndices[N] = i;
        WriteJson(writer, array, serializer, newIndices);
      }
      writer.WriteEndArray();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      int rank = objectType.GetArrayRank();
      Type elementType = objectType.GetElementType();
      Type jaggedArrayType = JaggedArrayType(elementType, rank);
      Array jaggedArray = (Array) serializer.Deserialize(reader, jaggedArrayType);
      int[] lengths = GetLengths(jaggedArray);
      Array array = Array.CreateInstance(elementType, lengths);
      CopyFromJaggedToMultidimensionalArray(jaggedArray, array, new int[0]);
      return array;
    }

    private static int[] GetLengths(Array jaggedArray)
    {
      // Check jagged array is not zero-length
      if (jaggedArray.Length == 0)
        throw new Exception("Cannot deserialize jagged array containing empty array");

      // Get first element of jagged array. If it is not an array, we're done
      object firstElement = jaggedArray.GetValue(0);
      if (! firstElement.GetType().IsArray)
        return new int[] {jaggedArray.Length};

      // Get lengths of remaining dimensions from first element
      int[] lengthsFirst = GetLengths((Array)firstElement);
      int N = lengthsFirst.Length;

      // Check lengths of remaining dimensions are consistent in other elements
      for (int i=1; i<jaggedArray.GetLength(0); ++i)
      {
        object thisElemenet = jaggedArray.GetValue(i);
        int[] lengthsThis = GetLengths((Array)thisElemenet);
        if (lengthsThis.Length != N)
          throw new Exception("Cannot deserialize non-cubical jagged array as multidimensional array");
        for (int n=0; n<N; ++n)
          if (lengthsThis[n] != lengthsFirst[n])
            throw new Exception("Cannot deserialize non-cubical jagged array as multidimensional array");
      }

      // Dimensions of jagged array are length of array, concatenated with length of remaining dimensions
      int[] lengths = new int[lengthsFirst.Length + 1];
      lengths[0] = jaggedArray.Length;
      for (int n = 0; n < N; ++n)
        lengths[n + 1] = lengthsFirst[n];
      return lengths;
    }

    private static void CopyFromJaggedToMultidimensionalArray(Array jaggedArray, Array array, int[] indices)
    {
      int N = indices.Length;
      if (N == array.Rank)
      {
        array.SetValue(JaggedArrayGetValue(jaggedArray, indices), indices);
        return;
      }
      int[] newIndices = new int[N+1];
      for (int n = 0; n < N; ++n)
        newIndices[n] = indices[n];
      for (int i = 0; i < array.GetLength(N); ++i)
      {
        newIndices[N] = i;
        CopyFromJaggedToMultidimensionalArray(jaggedArray, array, newIndices);
      }
    }

    private static object JaggedArrayGetValue(Array jaggedArray, int[] indices)
    {
      if (indices.Length == 1)
        return jaggedArray.GetValue(indices[0]);
      int N = indices.Length - 1;
      int[] newIndices = new int[N];
      for (int n = 0; n < N; ++n)
        newIndices[n] = indices[n + 1];
      return JaggedArrayGetValue((Array) jaggedArray.GetValue(indices[0]), newIndices);
    }

    private static Type JaggedArrayType(Type elementType, int rank)
    {
      return rank == 0 ? elementType : JaggedArrayType(elementType.MakeArrayType(), rank - 1);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified value type.
    /// </summary>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type valueType)
    {
      return typeof(Array).IsAssignableFrom(valueType) && valueType.GetArrayRank() >= 2;
    }
  }
}