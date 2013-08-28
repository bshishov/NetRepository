using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRepository
{
    /// <summary>
    /// Срез данных
    /// </summary>
    public class Slice
    {
        public int Id           { get { return _id; } }
        public int ParentId     { get { return _parentId; } } 

        private int _id;
        private int _parentId;

        public Slice(int id, int parentId = -1)
        {
            _id = id;
            _parentId = parentId;
        }
    }
}
