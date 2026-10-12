using System;
using Unity.Netcode;

public static class NetworkVaribaleSerializationBinder
{
     
     public static void Init()
     {
          BindUserNetworkVariableSerialization<PlayerState>();
          
          UserNetworkVariableSerialization<PlayerState>.WriteValue =
               (FastBufferWriter FastBufferWriter, in PlayerState value) =>
               {
                    FastBufferWriter.WriteValueSafe(value);
               };
     }

     public static void BindUserNetworkVariableSerialization<T>() where T : unmanaged, Enum
     {
          NetworkVariableSerializationTypes
               .InitializeEqualityChecker_UnmanagedValueEquals<T>();
          UserNetworkVariableSerialization<T>.WriteValue =
               (FastBufferWriter writer, in T value) =>
               {
                    writer.WriteValueSafe(value);
               };
          
          UserNetworkVariableSerialization<T>.ReadValue =
               (FastBufferReader reader, out T value) =>
               {
                    reader.ReadValueSafe(out value);
               };
          
          UserNetworkVariableSerialization<T>.DuplicateValue =
               (in T value, ref T duplicateValue) =>
               {
                    duplicateValue = value;
               };
          
     }
     
    
}