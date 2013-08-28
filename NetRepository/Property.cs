using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRepository
{
    /// <summary>
    /// Свойство объекта
    /// </summary>
    /// <typeparam name="TObjKey">Тип идентификатора объекта</typeparam>
    /// <typeparam name="TPropKey">Тип идентификатора свойства</typeparam>
    public class Property<TObjKey, TPropKey> : RepoEntity<TObjKey, TPropKey>
    {
        #region Поля

        TObjKey _objectId;
        TPropKey _propertyId;
        object _value; 

        #endregion

        #region Свойства

        public TPropKey Id { get { return _propertyId; } }
        public TObjKey ObjectId { get { return _objectId; } }
        public ObjectEntry<TObjKey, TPropKey> ObjectEntry {
            get
            {
                if (Manager != null)
                    return Manager.GetObject(ObjectId);
                else
                    return null;
            }
        }
        public object Value
        {
            get { return _value; }
            set
            {
                if (State != EntryState.Deleted)
                {
                    _value = value;
                    _state = EntryState.Modified;
                }
            }
        } 

        #endregion        

        #region Конструкторы

        public Property(TObjKey objectId, TPropKey propertyId, object value)
        {
            _objectId = objectId;
            _propertyId = propertyId;
            _value = value;
        } 

        #endregion


        public override void Save()
        {
            if (State == EntryState.Modified || State == EntryState.Detached)
            {
                Manager.SetProperty(this);
            }
        }        

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
