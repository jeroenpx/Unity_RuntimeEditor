using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract]
    public class PersistentDescriptor 
    {
        [ProtoIgnore]
        public PersistentDescriptor Parent;

        [ProtoMember(1)]
        public PersistentDescriptor[] Children;

        [ProtoMember(2)]
        public PersistentDescriptor[] Components;

        [ProtoMember(3)]
        public long PersistentID;

        [ProtoMember(4)]
        public string AssemblyQualifiedName;

        public PersistentDescriptor(Type persistentType, long persistentID)
        {
            PersistentID = persistentID;
            AssemblyQualifiedName = persistentType.AssemblyQualifiedName;

            Children = new PersistentDescriptor[0];
            Components = new PersistentDescriptor[0];
        }

        [ProtoAfterDeserialization]
        public void OnDeserialized()
        {
            if (Components != null)
            {
                for (int i = 0; i < Components.Length; ++i)
                {
                    Components[i].Parent = this;
                }
            }

            if (Children != null)
            {
                for (int i = 0; i < Children.Length; ++i)
                {
                    Children[i].Parent = this;
                }
            }
        }

        public override string ToString()
        {
            string pathToDesriptor = string.Empty;
            PersistentDescriptor descriptor = this;
            if (descriptor.Parent == null)
            {
                pathToDesriptor += "/";
            }
            else
            {
                while (descriptor.Parent != null)
                {
                    pathToDesriptor += "/" + descriptor.Parent.PersistentID;
                    descriptor = descriptor.Parent;
                }
            }
            return string.Format("Descriptor InstanceId = {0}, Type = {1}, Path = {2}, Children = {3} Components = {4}", PersistentID, AssemblyQualifiedName, pathToDesriptor, Children != null ? Children.Length : 0, Components != null ? Components.Length : 0);
        }
    }
}



