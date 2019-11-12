using System;

namespace Battlehub.RTSL.Interface
{
    public interface ITypeMap
    {
        Type ToPersistentType(Type unityType);
        Type ToUnityType(Type persistentType);
        Type ToType(Guid typeGuid);
        Guid ToGuid(Type type);
    }
}