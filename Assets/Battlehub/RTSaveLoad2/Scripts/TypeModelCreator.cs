using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.RTSaveLoad2
{
    public static partial class TypeModelCreator
    {
        public static RuntimeTypeModel Create()
        {
            RuntimeTypeModel model = TypeModel.Create();
            RegisterAutoTypes(model);
            RegisterUserDefinedTypes(model);

            Type[] serializableTypes = Reflection.GetAllFromCurrentAssembly()
              .Where(type => type.IsDefined(typeof(ProtoContractAttribute), false)).ToArray();

            MetaType primitiveContract = model.Add(typeof(PrimitiveContract), false);

            int fieldNumber = 16;
            foreach (Type type in serializableTypes)
            {
                if (type.IsGenericType())
                {
                    continue;
                }
                Type derivedType = typeof(PrimitiveContract<>).MakeGenericType(type.MakeArrayType());
                primitiveContract.AddSubType(fieldNumber, derivedType);
                fieldNumber++;
                model.Add(derivedType, true);

                derivedType = typeof(PrimitiveContract<>).MakeGenericType(type);
                primitiveContract.AddSubType(fieldNumber, derivedType);
                fieldNumber++;
                model.Add(derivedType, true);

                model.Add(typeof(List<>).MakeGenericType(type), true);
            }

            Type[] primitiveTypes = new[] {
                typeof(bool),
                typeof(char),
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(string),
                typeof(float),
                typeof(double),
                typeof(decimal) };

            foreach (Type type in primitiveTypes)
            {
                if (type.IsGenericType())
                {
                    continue;
                }
                
                Type derivedType = typeof(PrimitiveContract<>).MakeGenericType(type.MakeArrayType());
                primitiveContract.AddSubType(fieldNumber, derivedType);
                fieldNumber++;
                model.Add(derivedType, true);

                derivedType = typeof(PrimitiveContract<>).MakeGenericType(type);
                primitiveContract.AddSubType(fieldNumber, derivedType);
                fieldNumber++;
                model.Add(derivedType, true);

                model.Add(typeof(List<>).MakeGenericType(type), true);
            }

            return model;
        }

        static partial void RegisterAutoTypes(RuntimeTypeModel model);

        static partial void RegisterUserDefinedTypes(RuntimeTypeModel model);
    }
}

