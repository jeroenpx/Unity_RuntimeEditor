using Battlehub.RTSL.Interface;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;

namespace Battlehub.RTSL
{
    public static partial class TypeModelCreator
    {
        public static RuntimeTypeModel Create()
        {
            RuntimeTypeModel model = TypeModel.Create();
            model.Add(typeof(IntArray), true);
            model.Add(typeof(ProjectItem), true)
                .AddSubType(1025, typeof(AssetItem));
            model.Add(typeof(AssetItem), true);
            model.Add(typeof(AssetBundleItemInfo), true);
            model.Add(typeof(AssetBundleInfo), true);
            model.Add(typeof(ProjectInfo), true);
            model.Add(typeof(PrefabPart), true);
            model.Add(typeof(Preview), true);
            model.Add(typeof(PersistentDescriptor), true);
            model.Add(typeof(PersistentPersistentCall), true);
            model.Add(typeof(PersistentArgumentCache), true);
            
            RegisterAutoTypes(model);
            RegisterUserDefinedTypes(model);

            MetaType primitiveContract = model.Add(typeof(PrimitiveContract), false);
            int fieldNumber = 16;

            //NOTE: Items should be added to TypeModel in exactly the same order!!!
            //It is allowed to append new types, but not to insert new types in the middle.

            

            Type[] types = new[] {
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
                typeof(decimal),
                typeof(PersistentColor),
                typeof(PersistentVector4)};

            foreach (Type type in types)
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

            model.AutoAddMissingTypes = false;
            return model;
        }

        static partial void RegisterAutoTypes(RuntimeTypeModel model);

        static partial void RegisterUserDefinedTypes(RuntimeTypeModel model);
    }
}

