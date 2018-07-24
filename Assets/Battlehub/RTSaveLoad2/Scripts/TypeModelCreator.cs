using ProtoBuf.Meta;

namespace Battlehub.RTSaveLoad2
{
    public static partial class TypeModelCreator
    {
        public static RuntimeTypeModel Create()
        {
            RuntimeTypeModel model = TypeModel.Create();
            RegisterAutoTypes(model);
            RegisterUserDefinedTypes(model);
            return model;
        }

        static partial void RegisterAutoTypes(RuntimeTypeModel model);

        static partial void RegisterUserDefinedTypes(RuntimeTypeModel model);
    }
}

