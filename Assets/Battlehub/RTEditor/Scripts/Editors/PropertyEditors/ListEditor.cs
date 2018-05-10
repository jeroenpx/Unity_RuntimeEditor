using System;
using System.Collections;

namespace Battlehub.RTEditor
{
    public class ListEditor : IListEditor
    {
        public ListEditor()
        {

        }


        protected override IList Resize(IList list, int size)
        {
            int delta = size;
            if (list != null)
            {
                delta = size - list.Count; 
            }

            bool remove = delta < 0;

            IList newList = (list != null) ?
                (IList)Activator.CreateInstance(MemberInfoType, list):
                (IList)Activator.CreateInstance(MemberInfoType);

            Type elementType = MemberInfoType.GetGenericArguments()[0];

            if (remove)
            {
                for(int i = 0; i < -delta; ++i)
                {
                    newList.RemoveAt(newList.Count - 1);
                }
            }
            else
            {
                for(int i = 0; i < delta; ++i)
                {
                    newList.Add(Reflection.GetDefault(elementType));
                }
            }

            return newList;
        }
    }
}

